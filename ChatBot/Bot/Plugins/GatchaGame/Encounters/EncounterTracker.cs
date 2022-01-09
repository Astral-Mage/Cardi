using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{
    public class EncounterTracker
    {
        public Dictionary<string, Encounter> PendingEncounters;
        readonly TimeSpan ENCOUNTER_PREP_TIMEOUT = new TimeSpan(0, 30, 0);


        public void AddEncounter(Encounter enc)
        {
            if (!PendingEncounters.Any(x => x.Key == enc.EncounterCode))
            {
                PendingEncounters.Add(enc.EncounterCode, enc);
            }
        }

        public void KillEncounter(Encounter enc)
        {
            if (PendingEncounters.Any(x => x.Key == enc.EncounterCode))
            {
                PendingEncounters.Remove(enc.EncounterCode);
            }
        }


        public EncounterTracker()
        {
            PendingEncounters = new Dictionary<string, Encounter>();


        }
    }
}
