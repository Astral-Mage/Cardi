using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{
    public class Rotation
    {
        public string _Attacker;
        public string _Defender;

        public double _BaseDamage;
        public double _AddedDmg;

        public Dictionary<RawDamageType, double> _DmgMultByType;
        public Dictionary<RawDamageType, double> _DamageByType;
        public Dictionary<RawDamageType, double> _TotalDamageByType;

        public double _HitChance;
        public double _CritChance;
        public double _CritMult;

        public double _TotalCombinedDamage;
        public double _CritDamage;

        public double _EvasionChance;
        public Dictionary<RawDamageType, double> _FlatDamageReductionByType;
        public Dictionary<RawDamageType, double> _DamageReductionMultByType;

        public bool _HitConnected;
        public bool _CritConnected;

        public double _AtkStartingVit;
        public double _DefStartingVit;

        public double _TotalOverallDamageDealt;

        public Rotation(EncounterCard attacker, List<EncounterCard> defenders)
        {
            Setup();
        }

        public void Setup()
        {
            _Attacker = string.Empty;
            _Defender = string.Empty;
            _BaseDamage = 0;
            _AddedDmg = 0;
            _HitChance = 0;
            _CritChance = 0;
            _CritMult = 0;
            _TotalCombinedDamage = 0;
            _CritDamage = 0;
            _EvasionChance = 0;
            _TotalOverallDamageDealt = 0;
            _DmgMultByType = new Dictionary<RawDamageType, double>();
            _DamageByType = new Dictionary<RawDamageType, double>();
            _TotalDamageByType = new Dictionary<RawDamageType, double>();
            _FlatDamageReductionByType = new Dictionary<RawDamageType, double>();
            _DamageReductionMultByType = new Dictionary<RawDamageType, double>();
            _HitConnected = false;
            _CritConnected = false;
            _AtkStartingVit = 0;
            _DefStartingVit = 0;
        }

        public Rotation()
        {
            Setup();
        }
    }
}
