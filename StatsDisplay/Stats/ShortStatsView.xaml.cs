using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using StatsDisplay.Stats.Messages;

namespace StatsDisplay.Stats
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class ShortStatsWindow : HeroesWindow
	{
		private ShortStatsVm _viewModel;
		public  ShortStatsWindow(Properties.Settings appSettings)
		{
			InitializeComponent();
			_viewModel = new ShortStatsVm(game,appSettings);
			if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
			{

				this.DataContext = _viewModel;
			}

			if (appSettings.ShortStatsWindowTop <= 0)
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
			}
			Messenger.Default.Register(this,(HideShortStats _)=>Hide());
			Loaded += (_, __) => _viewModel.OnActivated();
		}
	}
}
