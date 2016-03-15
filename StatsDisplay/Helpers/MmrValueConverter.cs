using System;
using System.Collections.Generic;
using System.Linq;
using Heroes.ReplayParser;
using StatsFetcher;

namespace StatsDisplay
{
	// There is no way to use bindings as array indexers in WPF so we have to create a converter
	public class MmrValueConverter : GenericValueConverter<Dictionary<GameMode, PlayerProfile.MmrValue>, int>
	{
		protected override int Convert(Dictionary<GameMode, PlayerProfile.MmrValue> value)
		{
			return value[Properties.Settings.Default.MmrDisplayMode].Mmr;
		}
	}
}
