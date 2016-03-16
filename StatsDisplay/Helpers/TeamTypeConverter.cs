using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using StatsDisplay.Stats;

namespace StatsDisplay.Helpers
{
	public class TeamTypeConverter :IValueConverter
	{
		public  object FriendlyTeamStyle { get; set; }
		public object EnemyTeamStyle { get; set; }


		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is TeamTypes)
			{
				var teamType = (TeamTypes) value;
				return teamType == TeamTypes.Friendly ? FriendlyTeamStyle : EnemyTeamStyle;
			}
			return TeamTypes.Friendly;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
