using System;
using System.Collections.Generic;
using System.Linq;
using Heroes.ReplayParser;

namespace StatsFetcher
{
	public class Game
	{
		public List<PlayerProfile> Players { get; set; }
		public string Map { get; set; }
		public GameMode GameMode { get; set; }
		public Region Region { get; set; }
		public PlayerProfile Me { get; set; }
	}
}
