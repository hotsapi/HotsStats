using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Heroes.ReplayParser;
using NLog;
using Squirrel;
using StatsDisplay.Settings.Messages;
using StatsDisplay.Stats;
using StatsFetcher;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace StatsDisplay.Settings
{
	/// <summary>
	/// Viewmodel for the settings window
	/// </summary>
	public class SettingsVm : ViewModelBase
	{
		public Properties.Settings Settings { get { return App.Settings; } }
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly Assembly _currentAssembly;
		private WindowState _windowState;
		private static HotKey _hotKey;
		private readonly SynchronizationContext _currentSyncContext;
		private Window _currentWindow;
		private NotifyIcon _trayIcon;
		private IUpdateManager _updateManager;

		#region Bindable Properties
		public ICommand NavigateToHotsLogs { get; set; }

		public ICommand Test1 { get; set; }
		public ICommand Test2 { get; set; }

		/// <summary>
		/// Gets the title
		/// </summary>
		public String Title
		{
			get; private set;
		}

		/// <summary>
		/// Gets the  hotslogs home url
		/// </summary>
		public string LogHotsUri => "http://www.hotslogs.com/Default?utm_source=HotsStats&amp;utm_medium=link";
		
		//Gets or sets the window state
		public WindowState WindowState
		{
			get { return _windowState; }
			set
			{
				if (_windowState != value) {
					_windowState = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets the available game modes
		/// </summary>
		public IEnumerable<GameMode> GameModes { private set; get; }
		#endregion

		public SettingsVm()
		{
			SetExceptionHandlers();

			if (Settings.UpgradeRequired) {
				Settings.Upgrade();
				Settings.UpgradeRequired = false;
				Settings.Save();
			}

			_currentSyncContext = SynchronizationContext.Current;
			GameModes = new List<GameMode> {
				GameMode.QuickMatch,
				GameMode.HeroLeague,
				GameMode.TeamLeague
			};

			_currentAssembly = Assembly.GetExecutingAssembly();
			var currentVersion = _currentAssembly.GetName().Version;
			Title = $"HotsStats v{currentVersion.Major}.{currentVersion.Minor}" + (currentVersion.Build == 0 ? "" : $".{currentVersion.Build}");
			NavigateToHotsLogs = new RelayCommand(() => OnNavigate(LogHotsUri));
			Test1 = new RelayCommand(() => OnTest1());
			Test2 = new RelayCommand(() => OnTest2());
			if (!App.Debug && Settings.AutoUpdate) {
				CheckForUpdates();
			}
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
		/// Saves the current settings
		/// </summary>
		public void OnShutdown()
		{
			Settings.Save();
			_updateManager?.Dispose();
		}

		private void OnNavigate(string uri)
		{
			Process.Start(new ProcessStartInfo(uri));
		}

		private void OnTest1()
		{
			ProcessLobbyFile(@"replay.server.battlelobby");
		}

		private void OnTest2()
		{
			ProcessRejoinFile(@"save.StormSave");
		}

		/// <summary>
		/// Called when the Settings windows activated
		/// </summary>
		public void OnActivated()
		{
			_logger.Info("App started");
			SetupTrayIcon();
			SetupFileMonitor();
			SetupHotkeys();
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


		private void ProcessLobbyFile(string path)
		{
			if (!Settings.Enabled)
				return;

			//TODO: remove global state
			App.Game = FileProcessor.ProcessLobbyFile(path);
			App.Game.Me = App.Game.Players.FirstOrDefault(p => p.BattleTag == Settings.BattleTag || p.Name == Settings.BattleTag);

			_currentWindow?.Close();
			_currentWindow = new Stats.ShortStatsWindow();
			if (Settings.AutoShow)
				_currentWindow.Show();
		}

		private async void ProcessRejoinFile(string path)
		{
			try {
				await FileProcessor.ProcessRejoinAsync(path, App.Game);
				_currentWindow?.Close();
				_currentWindow = new FullStatsWindow();

				if (App.Debug) {
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

		private void ProcessReplayFile(string path)
		{
			// not implemented
		}

		private void SetupTrayIcon()
		{
			_trayIcon = new NotifyIcon {
				Icon = Icon.ExtractAssociatedIcon(_currentAssembly.Location),
				Visible = false
			};
			_trayIcon.Click += (o, e) => {
				SendMessage<ShowSettingsWindow>();
				WindowState = WindowState.Normal;
				_trayIcon.Visible = false;
			};
		}

		/// <summary>
		/// Curry function to allow execution of an action on the main thread using the synchronizatio context
		/// </summary>
		/// <param name="action"></param>
		private void RunOnMainThread(Action action)
		{
			_currentSyncContext.Post(_ => action(), null);
		}

		/// <summary>
		/// Sends a  simple message using the default messenger
		/// </summary>
		/// <typeparam name="T">the message type</typeparam>
		private void SendMessage<T>()
			where T : new()
		{
			Messenger.Default.Send<T>(new T());
		}

		public void OnWindowStateChanged()
		{
			if (Settings.MinimizeToTray && WindowState == WindowState.Minimized) {
				SendMessage<HideSettingsWindow>();
				_trayIcon.Visible = true;
			}
		}

		private void SetExceptionHandlers()
		{
			Application.Current.DispatcherUnhandledException += (o, e) => {
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
