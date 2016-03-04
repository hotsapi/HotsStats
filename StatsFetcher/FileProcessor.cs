using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatsFetcher
{
	public static class FileProcessor
	{
		public static async Task<List<PlayerProfile>> ProcessLobbyFile(string path)
		{
			var p = new BattleLobbyParser(path);
			var region = p.ExtractRegion();
			var tags = p.ExtractBattleTags();
			var f = new ProfileFetcher(tags, region);
			var profiles = await f.FetchBasicProfiles();
			for (int i = 0; i < profiles.Count; i++) {
				profiles[i].Team = i >= 5 ? 0 : 1;
			}
			return profiles;
		}
	}
}
