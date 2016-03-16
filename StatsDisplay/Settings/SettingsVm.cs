using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Heroes.ReplayParser;
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
		private readonly Properties.Settings _appSettings;

		private string _battleTAg;
		private bool _isEnabled;
		private GameMode _selectedGameMode;
		private bool _canMinimizeToTray;
		private readonly Assembly _currentAssembly;
		private WindowState _windowState;
		private bool _isVisible;
		private static HotKey _hotKey;
		private readonly SynchronizationContext _currentSyncContext;
		private Window _currentWindow;
		private bool _canAutoShow;
		private NotifyIcon _trayIcon;
		private bool _canAutoClose;
		private int _windowTop;
		private int _windowLeft;
		
		#region Bindable Properties
		public ICommand NavigateToHotsLogs { get; set; }

		//Gets or sets a value indicating wether the window can be autoshown
		public bool CanAutoShow
		{
			get { return _canAutoShow; }
			set
			{
				_canAutoShow = value;
				if (_canAutoShow != value)
				{
					_appSettings.AutoShow = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets orr sets the battle tag
		/// </summary>
		public string BattleTag
		{
			get { return _battleTAg; }
			set
			{
				if (_battleTAg != value)
				{
					_battleTAg = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating weather the app is enabled
		/// </summary>
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if (_isEnabled != value)
				{
					_isEnabled = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets the available game modes
		/// </summary>
		public IEnumerable<GameMode> GameModes
		{
			private set;
			get;
		}

		/// <summary>
		/// Gets or sets the selected game mode
		/// </summary>
		public GameMode SelectedGameMode
		{
			get { return _selectedGameMode; }
			set
			{
				if (_selectedGameMode != value)
				{
					_selectedGameMode = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets the title
		/// </summary>
		public String Title
		{
			get; private set;
		}

		/// <summary>
		/// Gets or sets a value indicating wether the settings window is minimized to the tray
		/// </summary>
		public bool CanMinimizeToTray
		{
			get { return _canMinimizeToTray; }
			set
			{
				if (_canMinimizeToTray != value)
				{
					_canMinimizeToTray = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets the  hotslogs home url
		/// </summary>
		public string LogHotsUri => "http://www.hotslogs.com";

		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					RaisePropertyChanged();
				}

			}
		}

		//Gets or sets the window state
		public WindowState WindowState
		{
			get { return _windowState; }
			set
			{
				if (_windowState != value)
				{
					_windowState = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets the window's left value
		/// </summary>
		public int WindowLeft
		{
			get { return _windowLeft; }
			set
			{
				if (_windowLeft != value)
				{
					_windowLeft = value;
					RaisePropertyChanged();
				}
			}
		}


		/// <summary>
		/// Gets or sets the window's top value
		/// </summary>
		public int WindowTop
		{
			get { return _windowTop; }
			set
			{
				if (_windowTop != value)
				{
					_windowTop = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicatin wether or not the window can autoclose
		/// </summary>
		public bool CanAutoClose
		{
			get { return _canAutoClose; }
			set
			{
				if (_canAutoClose != value)
				{
					_canAutoClose = value;
					RaisePropertyChanged();
				}

			}
		}
		#endregion

		public SettingsVm(Properties.Settings appSettings)
		{
			_appSettings = appSettings;
			SetExceptionHandlers();
			_currentSyncContext = SynchronizationContext.Current;
			GameModes = new List<GameMode> {
				GameMode.QuickMatch,
				GameMode.HeroLeague,
				GameMode.TeamLeague
			};

			_currentAssembly = Assembly.GetExecutingAssembly();
			var currentVersion = _currentAssembly.GetName().Version;
			LoadSettings(appSettings);
			Title = $"HotsStats v{currentVersion.Major}.{currentVersion.Minor}" + (currentVersion.Build == 0 ? "" : $".{currentVersion.Build}");
			NavigateToHotsLogs = new RelayCommand(() => OnNavigate(LogHotsUri));
			if (!App.Debug && appSettings.AutoUpdate)
			{
				CheckForUpdates();
			}
		}

		private async void CheckForUpdates()
		{
			try
			{
				var mgr = await UpdateManager.GitHubUpdateManager(_appSettings.UpdateRepository);
				await mgr.UpdateApp();
			}
			catch { /* quietly eat some errors */ }
		}

		/// <summary>
		/// Saves the current settings
		/// </summary>
		public void SaveSettings()
		{
			_appSettings.MmrDisplayMode = SelectedGameMode;
			_appSettings.AutoShow = CanAutoShow;
			_appSettings.AutoClose = CanAutoClose;
			_appSettings.MinimizeToTray = CanMinimizeToTray;
			_appSettings.BattleTag = BattleTag;
			_appSettings.Enabled = IsEnabled;
			_appSettings.SettingsWindowTop = WindowTop;
			_appSettings.SettingsWindowLeft = WindowLeft;
			_appSettings.Save();
		}

		private void OnNavigate(string logHotsUri)
		{
			Process.Start(new ProcessStartInfo(logHotsUri));
		}

		/// <summary>
		/// Called when the Settings windows activated
		/// </summary>
		public void OnActivated()
		{
			SetupTrayIcon();
			SetupFileMonitor();
			SetupHotkeys();
		}

		private void SetupHotkeys()
		{
			_hotKey = new HotKey(Key.Tab, KeyModifier.Shift | KeyModifier.NoRepeat);
			_hotKey.Pressed += (o, e) =>
			{
				if (_currentWindow?.IsVisible != null && (_currentWindow?.IsVisible).Value)
				{
					_currentWindow?.Hide();
				}
				else
				{
					_currentWindow?.Show();
				}
				
			};
		}

		private void SetupFileMonitor()
		{
			var mon = new FileMonitor();
			//TODO Temporary solution to run on main thread. Refactor filemon to use TPL in order to achieve this
			mon.BattleLobbyCreated += (_, e) => RunOnMainThread(() => ProcessLobbyFile(e.Data));
			mon.RejoinFileCreated += (_, e) => RunOnMainThread(() => ProcessRejoinFile(e.Data));
			mon.StartMonitoring();
		}


		private void ProcessLobbyFile(string path)
		{
			if (!IsEnabled)
				return;

			//TODO: remove global state
			App.game = FileProcessor.ProcessLobbyFile(path);
			App.game.Me = App.game.Players.FirstOrDefault(p => p.BattleTag == BattleTag || p.Name == BattleTag);

			_currentWindow?.Close();
			_currentWindow = new Stats.ShortStatsWindow(_appSettings);
			if (CanAutoShow)
				_currentWindow.Show();
		}


		private void LoadSettings(Properties.Settings appSettings)
		{
			SelectedGameMode = appSettings.MmrDisplayMode;
			CanAutoShow = appSettings.AutoShow;
			CanAutoClose = appSettings.AutoClose;
			CanMinimizeToTray = appSettings.MinimizeToTray;
			BattleTag = appSettings.BattleTag;
			IsEnabled = appSettings.Enabled;
			WindowTop = appSettings.SettingsWindowTop;
			WindowLeft = appSettings.SettingsWindowLeft;
		}

		private void ProcessRejoinFile(string path)
		{
			FileProcessor.ProcessRejoin(path, App.game);
			_currentWindow?.Close();
			_currentWindow = new FullStatsWindow();
		}

		private void SetupTrayIcon()
		{
			_trayIcon = new NotifyIcon
			{
				Icon = Icon.ExtractAssociatedIcon(_currentAssembly.Location),
				Visible = false
			};
			_trayIcon.Click += (o, e) =>
			{
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
			if (CanMinimizeToTray && WindowState == WindowState.Minimized)
			{
				SendMessage<HideSettingsWindow>();
				_trayIcon.Visible = true;
			}
		}

		private void SetExceptionHandlers()
		{
			Application.Current.DispatcherUnhandledException += (o, e) => {
				File.AppendAllText("log.txt", $"[{DateTime.Now}] Unhandled exception: {e.Exception}");
				try
				{
					MessageBox.Show(e.Exception.ToString(), "Unhandled exception");
				}
				catch { /* probably not gui thread */ }
			};

			AppDomain.CurrentDomain.UnhandledException += (o, e) => {
				File.AppendAllText("log.txt", $"[{DateTime.Now}] Critical exception: {e.ExceptionObject}");
				try
				{
					MessageBox.Show(e.ExceptionObject.ToString(), "Critical exception");
				}
				catch { /* probably not gui thread */ }
			};
		}
	}
}
