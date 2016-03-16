using System.Linq;

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
			playersTable.ItemsSource = game.Players.OrderBy(p => ((game.Me?.Team ?? 0) == 0 ? 1 : -1) * p.Team); // My team is always on top
		}
	}
}
