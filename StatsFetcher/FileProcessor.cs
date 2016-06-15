using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Heroes.ReplayParser;
using Foole.Mpq;
using NLog;

namespace StatsFetcher
{
    public static class FileProcessor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static Game ProcessLobbyFile(string path)
        {
            var game = new BattleLobbyParser(path).Parse();
            FetchProfiles(game);
            return game;
        }

        public static async Task ProcessReplayFile(string path, Game game)
        {
            var tmpPath = Path.GetTempFileName();
            // todo: we need a way to detect when HotS finished writing this file
            // For now we will just wait 1 sec and hope it is enough
            await Task.Delay(1000);
            await SafeCopy(path, tmpPath, true);
            var replay = DataParser.ParseReplay(tmpPath, true, true).Item2;

            foreach (var profile in game.Players) {
                var player = replay.Players.FirstOrDefault(p => p.Name == profile.Name);
                if (player == null)
                    continue;
                profile.Stats = player.ScoreResult;
            }
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

        public static async Task ProcessRejoinAsync(string path, Game game)
        {
            var tmpPath = Path.GetTempFileName();
            await SafeCopy(path, tmpPath, true);

            var replay = ParseRejoin(tmpPath);
            foreach (var profile in game.Players){
                var player = replay.Players.FirstOrDefault(p => p.Name == profile.Name);
                if (player == null)
                    continue;
                profile.Hero = player.Character;
                profile.HeroLevel = player.CharacterLevel;
                //profile.Team = player.Team; // this should fix possible mistakes made by battlelobby analyzer
            }
            game.Map = replay.Map;
            game.GameMode = replay.GameMode;
            ExtractFullData(game);
        }


        private static async Task SafeCopy(string source, string dest, bool overwrite)
        {
            var watchdog = 10;
            var retry = false;
            do
            {
                try
                {
                    File.Copy(source, dest, overwrite);
                    retry = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to copy ${source} to ${dest}. Counter at ${watchdog} CAUSED BY ${ex}");
                    if (watchdog <= 0)
                    {
                        throw;
                    }
                    retry = true;
                }
                await Task.Delay(1000);
            } while (watchdog-- > 0 && retry);
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
            catch (Exception ex) {
                //TODO: WE REALLY DON't want to do this
                Debug.WriteLine(ex);
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
                    p.GamesCount = int.Parse(p.HotsLogsProfile.GetElementbyId("ctl00_MainContent_RadGridGeneralInformation").SelectSingleNode(".//td[text()='Total Games Played']/../td[2]").InnerText);
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
