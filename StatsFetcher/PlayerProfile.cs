using System;
using System.Linq;
using System.Collections.Generic;

namespace StatsFetcher
{
	public class PlayerProfile
	{
		public PlayerProfile(string battleTag, Region region)
		{
			BattleTag = battleTag;
			Region = region;
			Ranks = new Dictionary<GameMode, MmrValue> {
				{ GameMode.QuickMatch, null },
				{ GameMode.HeroLeague, null },
				{ GameMode.TeamLeague, null }
			};
		}

		public string BattleTag { get; private set; }
		public Region Region { get; private set; }
		public int? HotslogsId { get; set; }
		public int Team { get; set; }
		public string Hero { get; set; }

		public Dictionary<GameMode, MmrValue> Ranks { get; private set; }

		public float? MapWinRate { get; set; }
		public float? HeroWinRate { get; set; }
		public int? HeroLevel { get; set; }

		public string Name { get { return BattleTag.Split('#')[0]; } }
		public string Link { get { return HotslogsId == null ? null : $"http://www.hotslogs.com/Player/Profile?PlayerID={HotslogsId}"; } }

		
		public class MmrValue
		{
			public int Mmr { get; set; }
			public League? League { get; set; }
			public GameMode GameMode { get; set; }
			public int? LeagueRank { get; set; }

			public MmrValue()
			{
			}

			public MmrValue(GameMode gameMode, int mmr, League? league, int? leagueRank)
			{
				Mmr = mmr;
				League = league;
				GameMode = gameMode;
				LeagueRank = leagueRank;
			}			
		}

		public enum League
		{
			Master = 1,
			Diamond= 2,
			Platinum = 3,
			Gold = 4,
			Silver = 5,
			Bronze = 6
		}

		public enum GameMode
		{
			QuickMatch,
			HeroLeague,
			TeamLeague
		}
	}
}
