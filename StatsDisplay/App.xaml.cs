﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using StatsFetcher;

namespace StatsDisplay
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		// introduce some spaghetti with static globals
		public static Game game { get; set; }
		public Properties.Settings Settings { get { return StatsDisplay.Properties.Settings.Default; } }
#if DEBUG
		public const bool Debug = true;
#else
		public const bool Debug = false;
#endif
	}
}
