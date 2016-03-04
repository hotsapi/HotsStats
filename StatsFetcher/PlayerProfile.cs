using System;
using System.Linq;

namespace StatsFetcher
{
	public class PlayerProfile
	{
		public string BattleTag { get; set; }
		public Region Region { get; set; }
		public int? HotslogsId { get; set; }
		public int Team { get; set; }
		public string Hero { get; set; }

		public int? QmMmr { get; set; }

		public float? MapWinRate { get; set; }
		public float? HeroWinRate { get; set; }
		public int? HeroLevel { get; set; }

		public string Name { get { return BattleTag.Split('#')[0]; } }
		public string Link { get { return HotslogsId == null ? null : $"http://www.hotslogs.com/Player/Profile?PlayerID={HotslogsId}"; } }
	}
}
