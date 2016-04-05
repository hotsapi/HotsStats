using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Heroes.ReplayParser;
using StatsFetcher;

namespace StatsDisplay.Helpers
{
    // There is no way to use bindings as array indexers in WPF so we have to create a converter
    public class MmrValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try {
                var ranks = (Dictionary<GameMode, PlayerProfile.MmrValue>)values[0];
                var mode = (GameMode)values[1];

                return ranks[mode].Mmr;
            }
            catch {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
