using System.Windows;
using System.Windows.Controls;
using StatsFetcher;

namespace StatsDisplay.Helpers
{

    public class PlayerRowTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item is PlayerProfile) {
                PlayerProfile p = (PlayerProfile) item;
                return element.FindResource(p.IsMyTeam ? "BlueRow" : "RedRow") as DataTemplate;
            }

            return null;
        }
    }
}
