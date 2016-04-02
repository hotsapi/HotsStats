using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using StatsDisplay.Stats;

namespace StatsDisplay.Helpers
{
    public class TeamTypeConverter : GenericValueConverter<TeamTypes, object>
    {
        public object FriendlyTeamStyle { get; set; }
        public object EnemyTeamStyle { get; set; }

        protected override object Convert(TeamTypes value)
        {
            return value == TeamTypes.Friendly ? FriendlyTeamStyle : EnemyTeamStyle;
        }
    }
}
