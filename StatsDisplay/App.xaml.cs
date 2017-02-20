using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NLog;
using Squirrel;
using StatsDisplay.Helpers;
using StatsDisplay.Stats;
using StatsFetcher;

namespace StatsDisplay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if DEBUG
        public const bool Debug = true;
#else
        public const bool Debug = false;
#endif

        // introduce some spaghetti with static globals
        public static Game Game { get; set; }
        public static Properties.Settings Settings { get { return StatsDisplay.Properties.Settings.Default; } }

        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private IUpdateManager _updateManager;
        private static HotKey _hotKey;
        private SynchronizationContext _currentSyncContext;
        private Window _currentWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetExceptionHandlers();
            _logger.Info("App started");
            _currentSyncContext = SynchronizationContext.Current;

            if (Settings.UpgradeRequired) {
                Settings.Upgrade();
                Settings.UpgradeRequired = false;
                Settings.Save();
            }

            new DispatcherTimer() {
                Interval = TimeSpan.FromHours(1),
                IsEnabled = true
            }.Tick += (_, __) => CheckForUpdates();
            CheckForUpdates();

            SetupFileMonitor();
            SetupHotkeys();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Save();
            _updateManager?.Dispose();
        }

        private void SetupHotkeys()
        {
            _hotKey = new HotKey(Key.Tab, KeyModifier.Shift | KeyModifier.NoRepeat);
            _hotKey.Pressed += (o, e) => {
                if (_currentWindow == null) {
                    return;
                }
                    
                if (_currentWindow.IsVisible) {
                    _currentWindow.Hide();
                } else {
                    _currentWindow.Show();
                }

            };
        }

        private void SetupFileMonitor()
        {
            var mon = new FileMonitor();
            //TODO Temporary solution to run on main thread. Refactor filemon to use TPL in order to achieve this
            mon.BattleLobbyCreated += (_, e) => RunOnMainThread(() => ProcessLobbyFile(e.Data));
            mon.RejoinFileCreated += (_, e) => RunOnMainThread(() => ProcessRejoinFile(e.Data));
            mon.ReplayFileCreated += (_, e) => RunOnMainThread(() => ProcessReplayFile(e.Data));
            mon.StartMonitoring();
        }


        internal void ProcessLobbyFile(string path)
        {
            if (!Settings.Enabled)
                return;

            //TODO: remove global state
            Game = FileProcessor.ProcessLobbyFile(path);
            Game.Me = Game.Players.FirstOrDefault(p => p.BattleTag == Settings.BattleTag || p.Name == Settings.BattleTag);

            _currentWindow?.Close();
            _currentWindow = new ShortStatsWindow();
            if (Settings.AutoShow)
                _currentWindow.Show();
        }

        internal async void ProcessRejoinFile(string path)
        {
            try {
				if (Game == null) {
					return;
				}
				await FileProcessor.ProcessRejoinAsync(path, Game);
                _currentWindow?.Close();
                _currentWindow = new FullStatsWindow();

                if (Debug) {
                    try {
                        var path1 = Path.Combine(@"saves", Path.GetRandomFileName());
                        Directory.CreateDirectory(path1);
                        File.Copy(path, path1 + "\\save.StormSave");
                        File.Copy(Path.Combine(Path.GetTempPath(), @"Heroes of the Storm\TempWriteReplayP1\replay.server.battlelobby"), path1 + "\\replay.server.battlelobby");
                    }
                    catch { }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }

        }

        internal async void ProcessReplayFile(string path)
        {
            if (Settings.ShowRecap) {
				if (Game == null) {
					return;
				}
                await FileProcessor.ProcessReplayFile(path, Game);
                _currentWindow?.Close();
                _currentWindow = new RecapStatsWindow();
                if (Settings.AutoShow)
                    _currentWindow.Show();
            }
        }

        private async void CheckForUpdates()
        {
            if (Debug || !Settings.AutoUpdate)
                return;

            try {
                if (_updateManager == null) {
                    _updateManager = await UpdateManager.GitHubUpdateManager(Settings.UpdateRepository);
                }

                var release = await _updateManager.UpdateApp();
            }
            catch { /* quietly eat some errors */ }
        }

        /// <summary>
        /// Curry function to allow execution of an action on the main thread using the synchronization context
        /// </summary>
        /// <param name="action"></param>
        private void RunOnMainThread(Action action)
        {
            _currentSyncContext.Post(_ => action(), null);
        }

        private void SetExceptionHandlers()
        {
            DispatcherUnhandledException += (o, e) => {
                _logger.Error(e.Exception, "Dispatcher unhandled exception");
                try {
                    MessageBox.Show(e.Exception.ToString(), "Unhandled exception");
                }
                catch { /* probably not gui thread */ }
            };

            AppDomain.CurrentDomain.UnhandledException += (o, e) => {
                _logger.Fatal(e.ExceptionObject as Exception, "Domain unhandled exception");
                try {
                    MessageBox.Show(e.ExceptionObject.ToString(), "Critical exception");
                }
                catch { /* probably not gui thread */ }
            };
        }
    }
}
