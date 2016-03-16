﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Heroes.ReplayParser;
using StatsDisplay.Stats.Messages;
using StatsFetcher;

namespace StatsDisplay.Stats
{
	public class ShortStatsVm : ViewModelBase
	{
		protected Game Game;
		protected Properties.Settings AppSettings;
		private string _teamOneAverageMmr;
		private string _teamTwoAverageMmr;
		private TeamVm _teamOne;
		private TeamVm _teamTwo;

		public string TeamTwoAverageMmr
		{
			get { return _teamTwoAverageMmr; }
			set
			{
				if (_teamTwoAverageMmr != value)
				{
					_teamTwoAverageMmr = value;
					RaisePropertyChanged();
				}
			}
		}

		public string TeamOneAverageMmr
		{
			get { return _teamOneAverageMmr; }
			set
			{
				if (_teamOneAverageMmr != value)
				{
					_teamOneAverageMmr = value;
					RaisePropertyChanged();
				}
			}
		}

		public TeamVm TeamOne
		{
			get { return _teamOne; }
			set
			{
				if (_teamOne != value)
				{
					_teamOne = value;
					RaisePropertyChanged();
				}
			}
		}

		public TeamVm TeamTwo
		{
			get { return _teamTwo; }
			set
			{
				if (_teamTwo != value)
				{
					_teamTwo = value;
					RaisePropertyChanged();
				}
			}
		}

		public ShortStatsVm()
		{
			
		}
		public ShortStatsVm(Game game, Properties.Settings appSettings)
		{
			Game = game;
			AppSettings = appSettings;
		}

		public async void OnActivated()
		{
			var me = Game.Me;
			var myTeam = me?.Team ?? 0;

			// time for some quick ugly hacks
			var teamOneMmebers = Game.Players.Where(p => p.Team == 0).ToList();
			var teamTwoMembers = Game.Players.Where(p => p.Team == 1).ToList();
			if (teamOneMmebers.Contains(me))
			{
				teamOneMmebers.Remove(me);
				teamOneMmebers.Insert(0, me);
			}
			else
			{
				teamTwoMembers.Remove(me);
				teamTwoMembers.Insert(0, me);
			}
			TeamOne = new TeamVm(teamOneMmebers);
			TeamTwo = new TeamVm(teamTwoMembers);

			TeamOne.TeamType = myTeam == 0 ? TeamTypes.Friendly : TeamTypes.Enemy;
			TeamTwo.TeamType = myTeam == 1 ? TeamTypes.Friendly : TeamTypes.Enemy;

			Game.PropertyChanged += (o, e) =>
			{
				TeamOneAverageMmr = "Average MMR: " + (int?)TeamOne.AverageMmr(AppSettings.MmrDisplayMode);
				TeamTwoAverageMmr = "Average MMR: " + (int?)TeamTwo.AverageMmr(AppSettings.MmrDisplayMode);			
			};

			if (AppSettings.AutoClose)
			{
				await Task.Delay(10000);
				Messenger.Default.Send(new HideShortStats());
			}
		}

	}

	public class TeamVm : ViewModelBase
	{
		private TeamTypes _teamType;

		public TeamTypes TeamType
		{
			get
			{
				return _teamType; 
				
			}
			set
			{
				_teamType = value;
				RaisePropertyChanged();
			}
		}

		public TeamVm(List<PlayerProfile> members)
		{
			Members = members;
		}

		public string Style { get; set; }
		public List<PlayerProfile> Members { get; set; }

		public double? AverageMmr(GameMode mmrMode)
		{
			return Members.Average(p => p.Ranks[mmrMode]?.Mmr);
		}
	}

	public enum TeamTypes
	{
		Friendly,
		Enemy
	}
}

