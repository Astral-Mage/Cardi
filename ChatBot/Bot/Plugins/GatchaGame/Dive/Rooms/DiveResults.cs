using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Core.Rooms
{
    public class DiveResults
    {
        public List<RoomResults> CombinedRoomResults;
        public int LevelUps;
        public FloorCard FloorCard;
        public int ClearedFloors = 0;

        public DiveResults()
        {
            CombinedRoomResults = new List<RoomResults>();
            LevelUps = 0;
            FloorCard = null;
        }
    }
}
