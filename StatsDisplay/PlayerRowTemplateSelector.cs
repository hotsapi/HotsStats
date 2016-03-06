using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StatsFetcher;


namespace StatsDisplay
{

	public class PlayerRowTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null && item is PlayerProfile) {
				PlayerProfile p = item as PlayerProfile;
				return element.FindResource(p.IsMyTeam ? "BlueRow" : "RedRow") as DataTemplate;
			}

			return null;
		}
	}
}
