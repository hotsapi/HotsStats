using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
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

			if (!Debug && Settings.AutoUpdate) {
				CheckForUpdates();
			}

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
				if (_currentWindow?.IsVisible ?? false) {
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

		internal void ProcessReplayFile(string path)
		{
			FileProcessor.ProcessReplayFile(path, Game);
		}

		private async void CheckForUpdates()
		{
			try {
				using (var mgr = await UpdateManager.GitHubUpdateManager(Settings.UpdateRepository)) {
					// for some reason "using" do not correctly dispose update manager if app exits before release check finishes (1-2 sec after starting)
					// which causes AbandonedMutexException at app exit
					// promoting it to private field and ensuring that dispose is called on app close seems to fix the problem
					_updateManager = mgr;
					var release = await mgr.UpdateApp();
				}
			}
			catch { /* quietly eat some errors */ }
		}

		/// <summary>
		/// Curry function to allow execution of an action on the main thread using the synchronizatio context
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
