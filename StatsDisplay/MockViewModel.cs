using System;
using System.Collections.Generic;
using System.Linq;
using Heroes.ReplayParser;
using StatsFetcher;

namespace StatsDisplay
{

	public class MockViewModel
	{
		public List<PlayerProfile> Players { get; set; }
		public Region Region { get; set; }

		public string MyAccount { get; set; } = "Player 4#123";
		public int MyTeam { get; set; }
		public PlayerProfile Me { get; set; }

		public MockViewModel()
		{
			Players = new List<PlayerProfile> {
				new PlayerProfile("Player 1#123", Region.EU),
				new PlayerProfile("Player 2#123", Region.EU),
				new PlayerProfile("Player 3#123", Region.EU),
				new PlayerProfile("Player 4#123", Region.EU),
				new PlayerProfile("Player 5#123", Region.EU),
			};
			foreach (var p in Players) {
				p.Ranks[GameMode.QuickMatch] = new PlayerProfile.MmrValue(GameMode.QuickMatch, 2200, null, null);
			}
			Me = Players.Where(p => p.BattleTag == MyAccount).FirstOrDefault();
			MyTeam = 0;
		}
	}
}
