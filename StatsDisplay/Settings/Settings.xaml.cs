using System;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using StatsDisplay.Settings.Messages;

namespace StatsDisplay.Settings
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public Properties.Settings Settings => Properties.Settings.Default;
		private readonly SettingsVm _viewModel;
		
		public SettingsWindow()
		{
			_viewModel = new SettingsVm(Properties.Settings.Default);
			DataContext = _viewModel;

			Messenger.Default.Register(this,(ShowSettingsWindow _)=>Show());
			Messenger.Default.Register(this, (HideSettingsWindow _) => Hide());

			Closing += (o, e) => {
		      _viewModel.OnShutdown();
				Application.Current.Shutdown();
			};
			Loaded += (_, __) => _viewModel.OnActivated();
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
			_viewModel.OnWindowStateChanged();
	}
	}
}
