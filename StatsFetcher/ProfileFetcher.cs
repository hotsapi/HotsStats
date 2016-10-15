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
        private readonly Game game;
        private readonly HttpClient web = new HttpClient();

        public ProfileFetcher(Game game)
        {
            this.game = game;
        }

        public async Task FetchBasicProfiles()
        {
            // start all requests in parallel

            await Task.WhenAll(game.Players.Select(FetchBasicProfile)).ConfigureAwait(false);
        }

        public async Task FetchFullProfiles()
        {
            // start all requests in parallel

            await Task.WhenAll(game.Players.Select(FetchFullProfile)).ConfigureAwait(false);
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
