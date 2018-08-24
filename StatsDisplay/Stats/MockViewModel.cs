using System.Collections.Generic;
using Heroes.ReplayParser;
using StatsFetcher;
using System.Linq;
using System.IO;
using System.Reflection;
using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace StatsDisplay.Stats
{
	public class MockViewModel : ShortStatsVm
	{
		public MockViewModel()
		{

		}

        private async void Initialize()
        {
            // todo: find a way to detect project dir at design time
            string path = @"C:\Dev\HotsStats\StatsDisplay\bin\Debug\";
            App.Game = await FileProcessor.ProcessLobbyFile(path + @"replay.server.battlelobby");
            await FileProcessor.ProcessReplayFile(path + @"replay.StormSave", App.Game);
            OnActivated();
        }
    }
}
