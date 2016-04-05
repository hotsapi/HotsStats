using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Heroes.ReplayParser;
using HtmlAgilityPack;

namespace StatsFetcher
{
    public class ProfileFetcher
    {
        private Game game;
        private HttpClient web;

        public ProfileFetcher(Game game)
        {
            this.game = game;
            this.web = new HttpClient();
        }

        public async Task FetchBasicProfiles()
        {
            var tasks = new List<Task>();

            // start all requests in parallel
            foreach (var p in game.Players) {
                tasks.Add(FetchBasicProfile(p));
            }

            foreach (var task in tasks) {
                await task;
            }
        }

        public async Task FetchFullProfiles()
        {
            var tasks = new List<Task>();

            // start all requests in parallel
            foreach (var p in game.Players) {
                tasks.Add(FetchFullProfile(p));
            }

            foreach (var task in tasks) {
                await task;
            }
        }

        private async Task FetchBasicProfile(PlayerProfile p)
        {
            try {
                var url = $"https://www.hotslogs.com/API/Players/{(int)game.Region}/{p.BattleTag.Replace('#', '_')}?utm_source=HotsStats&utm_medium=api";
                var str = await web.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(str) || str == "null")
                    return;
                dynamic json = JObject.Parse(str);
                p.HotslogsId = json.PlayerID;
                foreach (var r in json.LeaderboardRankings) {
                    var mode = (GameMode)Enum.Parse(typeof(GameMode), (string)r.GameMode);
                    p.Ranks[mode] = new PlayerProfile.MmrValue(mode, (int)r.CurrentMMR, (PlayerProfile.League?)(int?)r.LeagueID, (int?)r.LeagueRank);
                }
            }
            catch (Exception e) { /* some dirty exception swallow */ }
        }

        private async Task FetchFullProfile(PlayerProfile p)
        {
            if (p.HotslogsId == null)
                return;
            var url = $"http://www.hotslogs.com/Player/Profile?PlayerID={p.HotslogsId}&utm_source=HotsStats&utm_medium=api";
            var str = await web.GetStringAsync(url);
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(str);
                p.HotsLogsProfile = doc;
            }
            catch (Exception e) { /* some dirty exception swallow */ }
        }
    }
}
