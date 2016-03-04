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

namespace StatsDisplay
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : Window
	{
		public bool Enabled { get; set; } = true;
		public bool AutoClose { get; set; } = false;
		public string BattleTag { get; set; }


		public Settings()
		{
			InitializeComponent();
			var mon = new FileMonitor();
			mon.BattleLobbyCreated += (o, e) => Dispatcher.BeginInvoke(new Action(() => { ProcessLobbyFile(e.Data); }));
			mon.StartWatchingForLobby();
		}

		private async void ProcessLobbyFile(string path)
		{
			if (!Enabled)
				return;

			var profiles = await FileProcessor.ProcessLobbyFile(path);

			new MainWindow(profiles, BattleTag, AutoCloseCheck.IsChecked == true).Show();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ProcessLobbyFile(@"replay.server.battlelobby");
		}
	}
}
