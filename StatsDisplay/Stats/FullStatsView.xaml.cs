using System.Linq;
using System.Windows;

namespace StatsDisplay.Stats
{
	/// <summary>
	/// Interaction logic for FullStatsWindow.xaml
	/// </summary>
	public partial class FullStatsWindow : HeroesWindow
	{
		public FullStatsWindow()
		{
			InitializeComponent();
			playersTable.ItemsSource = Game.Players.OrderBy(p => ((Game.Me?.Team ?? 0) == 0 ? 1 : -1) * p.Team); // My team is always on top

			if (App.Settings.FullStatsWindowTop <= 0) {
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
			}
		}
	}
}
