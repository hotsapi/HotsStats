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
using System.Windows.Navigation;
using System.Windows.Shapes;
using StatsFetcher;
using System.Threading;
using Heroes.ReplayParser;


namespace StatsDisplay
{
	public class HeroesWindow : Window
	{
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
			Close();
		}

		protected void window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}
	}
}
