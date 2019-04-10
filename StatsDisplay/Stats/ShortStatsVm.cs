using System.Collections.Generic;
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
        public Game Game => App.Game;
        public Properties.Settings Settings => App.Settings;
        private int? _teamOneAverageMmr;
        private int? _teamTwoAverageMmr;
        private TeamVm _teamOne;
        private TeamVm _teamTwo;

        public int? TeamTwoAverageMmr
        {
            get { return _teamTwoAverageMmr; }
            set
            {
                if (_teamTwoAverageMmr != value) {
                    _teamTwoAverageMmr = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int? TeamOneAverageMmr
        {
            get { return _teamOneAverageMmr; }
            set
            {
                if (_teamOneAverageMmr != value) {
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
                if (_teamOne != value) {
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
                if (_teamTwo != value) {
                    _teamTwo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ShortStatsVm()
        {

        }

        public async void OnActivated()
        {
            var me = Game.Me;
            var myTeam = me?.Team ?? 0;

            // time for some quick ugly hacks
            var teamOneMmebers = Game.Players.Where(p => p.Team == 0).ToList();
            var teamTwoMembers = Game.Players.Where(p => p.Team == 1).ToList();
            if (teamOneMmebers.Contains(me)) {
                teamOneMmebers.Remove(me);
                teamOneMmebers.Insert(0, me);
            }
            if (teamTwoMembers.Contains(me)) {
                teamTwoMembers.Remove(me);
                teamTwoMembers.Insert(0, me);
            }
            TeamOne = new TeamVm(teamOneMmebers);
            TeamTwo = new TeamVm(teamTwoMembers);

            TeamOne.TeamType = myTeam == 0 ? TeamTypes.Friendly : TeamTypes.Enemy;
            TeamTwo.TeamType = myTeam == 1 ? TeamTypes.Friendly : TeamTypes.Enemy;

            TeamOneAverageMmr = TeamOne.AverageMmr(Settings.MmrDisplayMode);
            TeamTwoAverageMmr = TeamTwo.AverageMmr(Settings.MmrDisplayMode);

            Game.PropertyChanged += (o, e) => {
                TeamOneAverageMmr = TeamOne.AverageMmr(Settings.MmrDisplayMode);
                TeamTwoAverageMmr = TeamTwo.AverageMmr(Settings.MmrDisplayMode);
            };
            Settings.PropertyChanged += (o, e) => {
                if (e.PropertyName == nameof(Settings.MmrDisplayMode)) {
                    TeamOneAverageMmr = TeamOne.AverageMmr(Settings.MmrDisplayMode);
                    TeamTwoAverageMmr = TeamTwo.AverageMmr(Settings.MmrDisplayMode);
                }
            };

            if (Settings.AutoClose) {
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

        public int? AverageMmr(GameMode mmrMode)
        {
            return (int?)Members.Average(p => p.Ranks[mmrMode]?.Mmr);
        }
    }

    public enum TeamTypes
    {
        Friendly,
        Enemy
    }
}

