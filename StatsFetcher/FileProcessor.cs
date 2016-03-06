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
		public static async Task<Game> ProcessLobbyFile(string path)
		{
			var game = new BattleLobbyParser(path).Parse();
			await new ProfileFetcher(game).FetchBasicProfiles();
			return game;
		}

		public static void ProcessRejoin(string path, Game game)
		{
			var tmpPath = Path.GetTempFileName();
			File.Copy(path, tmpPath, overwrite: true);
			var replay = ParseRejoin(tmpPath);
			foreach (var profile in game.Players) {
				var player = replay.Players.Where(p => p.Name == profile.Name).Single();
				profile.Hero = player.Character;
				profile.HeroLevel = player.CharacterLevel;
			}
			game.Map = replay.Map;
			game.GameMode = replay.GameMode;
		}

		public static Replay ParseRejoin(string fileName)
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
