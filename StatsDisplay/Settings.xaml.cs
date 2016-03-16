using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StatsFetcher;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Heroes.ReplayParser;

namespace StatsDisplay
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public Properties.Settings Settings { get { return Properties.Settings.Default; } }
		private Window currentWindow;
		private HotKey hotKey;
		private System.Windows.Forms.NotifyIcon icon;


		public SettingsWindow()
		{
			if (Settings.UpgradeRequired) {
				Settings.Upgrade();
				Settings.UpgradeRequired = false;
				Settings.Save();
			}

			InitializeComponent();
			var v = Assembly.GetExecutingAssembly().GetName().Version;
			Title = $"HotsStats v{v.Major}.{v.Minor}" + (v.Build == 0 ? "" : $".{v.Build}");
			if (Settings.SettingsWindowTop <= 0)
				WindowStartupLocation = WindowStartupLocation.CenterScreen;

			GameModeCombo.ItemsSource = new List<GameMode> {
				GameMode.QuickMatch,
				GameMode.HeroLeague,
				GameMode.TeamLeague
			};

			icon = new System.Windows.Forms.NotifyIcon();
			icon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
			icon.Visible = false;
			icon.Click += (o,e) => {
				Show();
				WindowState = WindowState.Normal;
				icon.Visible = false;
			};

			var mon = new FileMonitor();
			mon.BattleLobbyCreated += (o, e) => Dispatcher.BeginInvoke(new Action(() => { ProcessLobbyFile(e.Data); }));
			mon.RejoinFileCreated += (o,e) => Dispatcher.BeginInvoke(new Action(() => { ProcessRejoinFile(e.Data); }));
			mon.ReplayFileCreated += (o, e) => Dispatcher.BeginInvoke(new Action(() => { ProcessReplayFile(e.Data); }));
			mon.StartMonitoring();

			hotKey = new HotKey(Key.Tab, KeyModifier.Shift | KeyModifier.NoRepeat);
			hotKey.Pressed += (o, e) => {
				if (currentWindow != null)
					currentWindow.Visibility = currentWindow.IsVisible ? Visibility.Collapsed : Visibility.Visible;
			};

			Closing += (o, e) => {
				Settings.Save();
				Application.Current.Shutdown();
			};
		}

		private void ProcessLobbyFile(string path)
		{
			if (!Settings.Enabled)
				return;

			App.game = FileProcessor.ProcessLobbyFile(path);
			App.game.Me = App.game.Players.Where(p => p.BattleTag == Settings.BattleTag || p.Name == Settings.BattleTag).FirstOrDefault();

			currentWindow?.Close();
			currentWindow = new ShortStatsWindow();
			if (Settings.AutoShow)
				currentWindow.Show();
		}

		private void ProcessRejoinFile(string path)
		{
			FileProcessor.ProcessRejoin(path, App.game);
			currentWindow?.Close();
			currentWindow = new FullStatsWindow();
		}

		private void ProcessReplayFile(string path)
		{
			
		}

		private void Test1_Click(object sender, RoutedEventArgs e)
		{
			ProcessLobbyFile(@"replay.server.battlelobby");
		}

		private async void Test2_Click(object sender, RoutedEventArgs e)
		{
			ProcessRejoinFile(@"save.StormSave");
			currentWindow.Show();
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control) {
				testButtons.Visibility = testButtons.IsVisible ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
			if (Settings.MinimizeToTray && WindowState == WindowState.Minimized) {
				Hide();
				icon.Visible = true;
			}
		}
	}
}
