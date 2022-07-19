using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Cards.Floor;
using ChatBot.Bot.Plugins.GatchaGame.Core.Rooms;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public class Room
    {
        public List<EnemyCard> Enemies;
        public int RoomProgress;
        public int TurnCounter;

        public Room()
        {
            Enemies = new List<EnemyCard>();
            RoomProgress = 0;
            TurnCounter = 0;
        }

        public RoomResults Execute(Cards.PlayerCard pc, FloorCard fc, out string responseExtra)
        {
            RoomResults _roomResults = new RoomResults();
            List<EnemyCard> Enemies = new List<EnemyCard>();
            Enemies = RngGeneration.GenerateEnemyCluster(fc, pc);
            responseExtra = string.Empty;
            responseExtra += $"NumEn: {Enemies.Count} ";
            DateTime now = DateTime.Now;

            // order us for combat
            Dictionary<int, List<BaseCard>> Combatants = new Dictionary<int, List<BaseCard>>
            {
                [pc.GetStat(Enums.StatTypes.Spd)] = new List<BaseCard>()
            };
            Combatants[pc.GetStat(Enums.StatTypes.Spd)].Add(pc);
            Encounter encounter = new Encounter(new TimeSpan(), pc.Name);
            encounter.AddParticipant(1, pc);

            foreach (var enemy in Enemies)
            {
                encounter.AddParticipant(2, enemy);

                if (Combatants.ContainsKey(enemy.GetStat(Enums.StatTypes.Spd)))
                {
                    Combatants[enemy.GetStat(Enums.StatTypes.Spd)].Add(enemy);
                }
                else
                {
                    Combatants[enemy.GetStat(Enums.StatTypes.Spd)] = new List<BaseCard>
                    {
                        enemy
                    };
                }
            }
            var ordered = Combatants.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            // room size
            int roomsize = 1 + (int)Math.Sqrt(RngGeneration.Rng.Next(1, 10));
            if (roomsize == 1)
            {
                RoomProgress = RngGeneration.Rng.Next(75, 110);
            }
            else if (roomsize == 2)
            {
                RoomProgress = RngGeneration.Rng.Next(44, 87);
            }
            else
            {
                RoomProgress = RngGeneration.Rng.Next(25, 57);
            }

            encounter.StartEncounter(Enums.EncounterTypes.Room);
            _roomResults.EncounterResults = encounter.RunEncounter();

            if (encounter.Participants.First(x => x.Participant.Name.Equals(pc.Name, StringComparison.InvariantCultureIgnoreCase)).Team == _roomResults.EncounterResults.WinningTeam)
            {
                _roomResults.RoomCleared = this;
                if (_roomResults.AllRewards.ContainsKey(pc.Name))
                    _roomResults.AllRewards[pc.Name].Add(new Reward(Enums.RewardTypes.Stat, Enums.StatTypes.Prg, RoomProgress));
                else
                    _roomResults.AllRewards.Add(pc.Name, new List<Reward>() { new Reward(Enums.RewardTypes.Stat, Enums.StatTypes.Prg, RoomProgress) });
            }

            return _roomResults;
        }
    }
}