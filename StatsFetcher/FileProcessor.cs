using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Heroes.ReplayParser;
using Foole.Mpq;

namespace StatsFetcher
{
	public static class FileProcessor
	{
		public static Game ProcessLobbyFile(string path)
		{
			var game = new BattleLobbyParser(path).Parse();
			FetchProfiles(game);
			return game;
		}

		public static async Task FetchProfiles(Game game)
		{
			var f = new ProfileFetcher(game);
			await f.FetchBasicProfiles();
			game.TriggerPropertyChanged();
			await f.FetchFullProfiles();
			ExtractBasicData(game);
			game.TriggerPropertyChanged();
		}

		public static async Task ProcessRejoin(string path, Game game)
		{
			var tmpPath = Path.GetTempFileName();

			// probably client is still writing this file so we retry a few times
			for (int i = 0; i < 5; i++) {
				try {
					File.Copy(path, tmpPath, overwrite: true);
					break;
				}
				catch (IOException e) {
					if (i < 4) {
						await Task.Delay(1000);
					} else {
						File.AppendAllText("log.txt", $"[{DateTime.Now}] Replay copy error ({i}): {e}\n\n");
						throw new ApplicationException("Can't access replay file", e);
					}
				}
			}

			var replay = ParseRejoin(tmpPath);
			foreach (var profile in game.Players) {
				var player = replay.Players.Where(p => p.Name == profile.Name).Single();
				profile.Hero = player.Character;
				profile.HeroLevel = player.CharacterLevel;
				//profile.Team = player.Team; // this should fix possible mistakes made by battlelobby analyzer
			}
			game.Map = replay.Map;
			game.GameMode = replay.GameMode;
			ExtractFullData(game);
		}

		public static Replay ParseRejoin(string fileName)
		{
			try {
				var replay = new Replay();

				var archive = new MpqArchive(fileName);
				archive.AddListfileFilenames();

				// Replay Details
				ReplayDetails.Parse(replay, DataParser.GetMpqFile(archive, "save.details"), true);

				// Player level is stored there
				ReplayAttributeEvents.Parse(replay, DataParser.GetMpqFile(archive, "replay.attributes.events"));

				return replay;
			}
			catch (Exception e) {
				// todo: eating exceptions is bad
				return null;
			}
		}

		// Extract data from HotsLogs profile while we don't know Heroes and Map
		public static void ExtractBasicData(Game game)
		{
			foreach (var p in game.Players) {
				if (p.HotsLogsProfile == null)
					continue;
				try {
					p.GamesCount = int.Parse(p.HotsLogsProfile.GetElementbyId("MainContent_tdTotalGamesPlayed").InnerText);
				}
				catch (Exception e) { /* some dirty exception swallow */ }
			}
		}

		// Extract data from HotsLogs profile when we know Heroes and Map
		public static void ExtractFullData(Game game)
		{
			foreach (var p in game.Players) {
				if (p.HotsLogsProfile == null)
					continue;
				// who wants to look at some dirty html parsing?
				try {					
					p.MapWinRate = float.Parse(p.HotsLogsProfile.GetElementbyId("mapStatistics").SelectSingleNode($".//tr/td[text()='{game.Map}']").SelectSingleNode("./../td[last()]").InnerText.Replace("%", ""), CultureInfo.InvariantCulture);
				}
				catch { }
				try {
					p.HeroWinRate = float.Parse(p.HotsLogsProfile.GetElementbyId("heroStatistics").SelectSingleNode($".//tr/td/a[text()='{p.Hero}']").SelectSingleNode("./../../td[last()]").InnerText.Replace("%", ""), CultureInfo.InvariantCulture);
				}
				catch { }
				p.HotsLogsProfile = null; // release memory taken by large hotslogs page
			}
		}
	}
}
