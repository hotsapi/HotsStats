using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StatsFetcher
{
	public class ProfileFetcher
	{
		Region region;
		List<string> battleTags;
		HttpClient web;

		public ProfileFetcher(List<string> battleTags, Region region)
		{
			this.region = region;
			this.battleTags = battleTags;
			this.web = new HttpClient();
		}

		public async Task<List<PlayerProfile>> FetchBasicProfiles()
		{
			var tasks = new List<Task<PlayerProfile>>();
			var result = new List<PlayerProfile>();

			// start all requests in parallel
			foreach (var t in battleTags) {
				tasks.Add(FetchBasicProfile(t));
			}

			foreach (var task in tasks) {
				result.Add(await task);
			}
			
			return result;
		}

		private async Task<PlayerProfile> FetchBasicProfile(string tag)
		{
			var url = $"https://www.hotslogs.com/API/Players/{(int)region}/{tag.Replace('#', '_')}";
			var str = await web.GetStringAsync(url);
			var p = new PlayerProfile(tag, region);
			if (string.IsNullOrWhiteSpace(str) || str == "null")
				return p;
			try {
				dynamic json = JObject.Parse(str);
				p.HotslogsId = json.PlayerID;
				foreach (var r in json.LeaderboardRankings) {
					var mode = (PlayerProfile.GameMode)Enum.Parse(typeof(PlayerProfile.GameMode), (string)r.GameMode);
					p.Ranks[mode] = new PlayerProfile.MmrValue(mode, (int)r.CurrentMMR, (PlayerProfile.League?)(int?)r.LeagueID, (int?)r.LeagueRank);
				}
			}
			catch (Exception e) { /* some dirty exception swallow */ }
			return p;
		}

		// todo: add ability to fetch full profile by parsing HTML
	}
}
