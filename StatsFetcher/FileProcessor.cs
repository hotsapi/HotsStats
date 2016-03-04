using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Heroes.ReplayParser;
using Foole.Mpq;

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

		public static void ProcessReplay(string path, List<PlayerProfile> profiles)
		{
			var tmpPath = Path.GetTempFileName();
			File.Copy(path, tmpPath, overwrite: true);
			var replay = ParseReplay(tmpPath);
			foreach (var profile in profiles) {
				var player = replay.Players.Where(p => p.Name == profile.Name).Single();
				profile.Hero = player.Character;
				profile.HeroLevel = player.CharacterLevel;
			}
			// todo: return replay.Map; replay.GameMode;
		}

		public static Replay ParseReplay(string fileName)
		{
			try {
				var replay = new Replay();

				var archive = new MpqArchive(fileName);
				archive.AddListfileFilenames();

				// Replay Details
				ReplayDetails.Parse(replay, DataParser.GetMpqFile(archive, "save.details"));

				// Player level is stored there
				ReplayAttributeEvents.Parse(replay, DataParser.GetMpqFile(archive, "replay.attributes.events"));

				return replay;
			}
			catch {
				// todo: eating exceptions is bad
				return null;
			}
		}
	}
}
