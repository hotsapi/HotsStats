using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StatsFetcher;

namespace StatsDisplay.Stats
{
	public class HeroesWindow : Window
	{
		public Properties.Settings Settings { get { return App.Settings; } }
		public Game Game { get { return App.Game; } }

		public HeroesWindow()
		{
			Style = Application.Current.FindResource("HeroesWindow") as Style;
		}

		public override void OnApplyTemplate()
		{
			Image closeButton = GetTemplateChild("closeButton") as Image;
			if (closeButton != null)
				closeButton.MouseDown += CloseClick;

			this.MouseDown += window_MouseDown;

			base.OnApplyTemplate();
		}

		protected void CloseClick(object sender, RoutedEventArgs e)
		{
			Hide();
		}

		protected void window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}

		protected void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			var link = (sender as Image).Tag as string;
			if (link != null)
				System.Diagnostics.Process.Start(link);
		}
	}
}
