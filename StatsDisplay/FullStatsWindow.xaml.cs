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
using System.Windows.Shapes;

namespace StatsDisplay
{
	/// <summary>
	/// Interaction logic for FullStatsWindow.xaml
	/// </summary>
	public partial class FullStatsWindow : HeroesWindow
	{
		public FullStatsWindow()
		{
			InitializeComponent();
			playersTable.ItemsSource = game.Players.OrderBy(p => (game.Me?.Team == 0 ? 1 : -1) * p.Team); // My team is always on top
		}
	}
}
