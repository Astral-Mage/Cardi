using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Dive.Results
{
    public class TurnResult
    {

        public bool _WinnerFound;
        public string _Winner;

        public List<Rotation> _Rotations;
        public int _LivingParticipants;

        public TurnResult()
        {
            _LivingParticipants = 0;
            _Rotations = new List<Rotation>();
            _WinnerFound = false;
            _Winner = string.Empty;
        }
    }
}
