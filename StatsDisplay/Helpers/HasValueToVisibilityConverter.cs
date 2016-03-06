using System;
using System.Linq;
using System.Windows;

namespace StatsDisplay
{

	public class HasValueToVisibilityConverter:GenericValueConverter<object, Visibility>
	{
		protected override Visibility Convert(object value)
		{
			return value != null ? Visibility.Visible : Visibility.Hidden;
		}
	}
}
