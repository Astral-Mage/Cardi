using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Bot.Plugins.LostRPG.EquipmentSystem.EquipmentObjects;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CombatSystem
{
    public static class CombatController
    {
        static readonly int _basehit = 50;
        static readonly int _basecrit = 0;
        static readonly int _basespread = 15;
        static readonly int _basecritmult = 50;
        static readonly int _baseevasion = 10;
        static readonly int _basemitigation = 0;
        static readonly int _baseglancespread = 7;
        static readonly int _baseeffectresist = 5;

        static readonly int _maxhit = 95;
        static readonly int _maxcrit = 50;
        static readonly int _maxeva = 95;



        static List<ImpendingAttack> ImpendingAttacks = new List<ImpendingAttack>();

        public static bool CreateImpendingAttack(UserCard attacker, UserCard defender, Skill attackerSkill)
        {
            ImpendingAttack iat = new ImpendingAttack(attacker, defender, attackerSkill);
            foreach (var atk in ImpendingAttacks)
            {
                if (atk.Attacker.UserId == attacker.UserId)
                {
                    return false;
                }
            }

            ImpendingAttacks.Add(iat);
            return true;
        }

        static void CheckTimeouts()
        {
            List<ImpendingAttack> toDelete = new List<ImpendingAttack>();
            foreach (var v in ImpendingAttacks)
            {
                if (v.GetRemainingTime().TotalMilliseconds <= 0) toDelete.Add(v);
            }
            toDelete.ForEach(x => ImpendingAttacks.Remove(x));
        }

        public static ImpendingAttack ConfirmImpendingAttack(UserCard defender, Skill defenderSkill, bool decline = false)
        {
            CheckTimeouts();
            ImpendingAttack attack = null;
            foreach (var atk in ImpendingAttacks)
            {
                if (atk.Defender.UserId == defender.UserId)
                {
                    atk.DefenderSkill = defenderSkill;
                    attack = atk;
                }
            }

            if (attack == null)
            {
                return null;
            }

            ImpendingAttacks.Remove(attack);

            if (decline)
            {
                ImpendingAttacks.Remove(attack);
                return attack;

            }

            return attack;
        }

        static int GetFixedStat(ImpendingAttack ia, StatTypes type, bool attacker = true)
        {
            int toreturn = 0;
            UserCard target = (attacker) ? ia.Attacker : ia.Defender;
            Skill tSkill = (attacker) ? ia.AttackerSkill : ia.DefenderSkill;

            toreturn = target.GetMultipliedStat(type, true, tSkill);
            return toreturn;
        }

        public static int CalculateHitChance(ImpendingAttack ia, bool attacker = true)
        {
            UserCard card = (attacker) ? ia.Attacker : ia.Defender;
            StatTypes hittype = StatTypes.Dexterity;
            if (card.GetDamageType() == Data.Enums.DamageTypes.Magic) hittype = StatTypes.Wisdom;
            if (card.GetDamageType() == Data.Enums.DamageTypes.Lust) hittype = StatTypes.Charisma;

            return 15 + (int)(_basehit * (1 + (.01 * (int)Math.Ceiling(7 * Math.Sqrt(GetFixedStat(ia, hittype, attacker))))));
        }

        public static int CalculateCritChance(ImpendingAttack ia, bool attacker = true)
        {
            UserCard card = (attacker) ? ia.Attacker : ia.Defender;
            StatTypes hittype = StatTypes.Dexterity;
            if (card.GetDamageType() == Data.Enums.DamageTypes.Magic) hittype = StatTypes.Wisdom;

            double totalbase = Math.Ceiling(GetFixedStat(ia, hittype, attacker) * 0.7);
            totalbase += Math.Ceiling(GetFixedStat(ia, StatTypes.Perception, attacker) * 0.3);
            totalbase = Math.Pow(totalbase, 1.22);
            double sqrt = Math.Ceiling(1.5 * Math.Sqrt(totalbase));
            int toreturn = (int)Math.Ceiling(sqrt * 0.9);
            return toreturn;
        }

        public static int CalculateCritDamage(ImpendingAttack ia, int basedamage, bool attacker = true)
        {
            return (int)Math.Ceiling(basedamage * (.01 * _basecritmult));
        }

        public static int CalculateBaseDamage(ImpendingAttack ia, bool attacker = true, bool glancinghit = false)
        {
            UserCard card = (attacker) ? ia.Attacker : ia.Defender;
            StatTypes hittype = StatTypes.Strength;
            int basedmg = 0;

            Skill stu = (attacker) ? ia.AttackerSkill : ia.DefenderSkill;
            bool matchingDamageType = false;
            if (card.GetDamageType() == Data.Enums.DamageTypes.Magic) 
                hittype = StatTypes.Intelligence;
            if (card.GetDamageType() == Data.Enums.DamageTypes.Lust) 
                hittype = StatTypes.Libido;
            if (card.ActiveSockets.Any(x => x.SocketType == Data.Enums.SocketTypes.Weapon))
            {
                matchingDamageType = card.GetDamageType() == (card.ActiveSockets.First(x => x.SocketType == SocketTypes.Weapon) as WeaponSocket).DamageType;
                basedmg = card.ActiveSockets.First(x => x.SocketType == Data.Enums.SocketTypes.Weapon).Stats.GetStat(StatTypes.Damage);
            }

            int basedmgstat = GetFixedStat(ia, hittype, attacker);
            int totalmult = card.GetTotalStatMultiplier(StatTypes.Damage);
            if (stu != null && stu.Stats.Stats.ContainsKey(StatTypes.Damage)) 
                totalmult += stu.Stats.GetStat(StatTypes.Damage);

            int totalbasedmg = basedmgstat + basedmg;
            double tmd = totalbasedmg * (1 + (.01 * totalmult));
            double multiplieddmg = (totalmult > 0) ? tmd : totalbasedmg;

            if (matchingDamageType) multiplieddmg = multiplieddmg * 1.1;

            return (int)Math.Ceiling(multiplieddmg);
        }

        public static int CalculateDamageSpreadPercent(ImpendingAttack ia, bool attacker = true)
        { // damage spread: tui and per
            UserCard card = (attacker) ? ia.Attacker : ia.Defender;
            double per = card.GetMultipliedStat(StatTypes.Perception) * .5;
            double tui = card.GetMultipliedStat(StatTypes.Intuition) * .5;
            double totalbase = per + tui;
            totalbase = Math.Pow(totalbase, 1.1);
            totalbase *= .2;
            return (int)totalbase;
        }

        public static int CalculateEvasionChance(ImpendingAttack ia, bool defender = true)
        {
            UserCard card = (defender) ? ia.Defender : ia.Attacker;
            // cha and dex

            double totalbasestat = (GetFixedStat(ia, StatTypes.Dexterity, !defender) + GetFixedStat(ia, StatTypes.Charisma, !defender)) * .5;

            return (int)(_baseevasion * (1 + (.01 * (int)Math.Ceiling(7 * Math.Sqrt(totalbasestat))))); ;
        }

        public static int CalculateDamageMitigation(ImpendingAttack ia, bool defender = true)
        {
            // mitigation: per and con
            // lustresist: cha and tui
            DamageTypes enemydtype = (defender) ? ia.Attacker.GetDamageType() : ia.Defender.GetDamageType();
            UserCard card = (defender) ? ia.Defender : ia.Attacker;

            StatTypes t1 = StatTypes.Charisma;
            StatTypes t2 = StatTypes.Intuition;

            if (enemydtype != DamageTypes.Lust)
            {
                t1 = StatTypes.Constitution;
                t2 = StatTypes.Perception;
            }

            double stat1 = GetFixedStat(ia, t1, !defender);
            double stat2 = GetFixedStat(ia, t2, !defender);

            double totalstats = 0;
            if (enemydtype == DamageTypes.Magic)
            {
                totalstats = (stat1 * .3) + (stat2 * .7);
            }
            else if (enemydtype == DamageTypes.Physical)
            {
                totalstats = (stat1 * .7) + (stat2 * .3);
            }
            else
            {
                totalstats = (stat1 * .5) + (stat2 * .5);
            }


            double totalbase = Math.Pow(totalstats, 1.6);
            double sqrt = Math.Ceiling(.5 * Math.Sqrt(totalbase));
            int toreturn = (int)Math.Ceiling(sqrt);
            return toreturn;
        }

        public static string CalculateSingleAttack(ImpendingAttack ia, List<Effect> attackerDebuffs, bool attacker = true)
        {
            // deal attacker damage
            string tosend = "";
            bool crit = false;
            bool hit = false;
            bool glancinghit = false;
            int hitchance = 0;
            int hitroll = 0;
            int critchance = 0;
            int critroll = 0;
            int critdamage = 0;
            int totaldamage = 0;
            int dmgspread = _basespread;
            int evasion = 0;
            int mitigation = 0;
            int combinedhitchance = 0;
            int damage = 0;
            UserCard tdefender = (attacker) ? ia.Defender : ia.Attacker;
            UserCard tattacker = (!attacker) ? ia.Defender : ia.Attacker;
            Skill taskill = (attacker) ? ia.AttackerSkill : ia.DefenderSkill;

            // attacker
            // hit and evade
            hitchance = CalculateHitChance(ia, attacker);
            evasion = CalculateEvasionChance(ia, !attacker);
            if (evasion > _maxeva) evasion = _maxeva;
            combinedhitchance = hitchance - evasion;
            if (combinedhitchance > _maxhit) combinedhitchance = _maxhit;
            hitroll = RNG.Seed.Next(0, 101);
            if (hitroll < combinedhitchance)
            {
                hit = true;
                if (hitchance - hitroll < _baseglancespread)
                    glancinghit = true;
            }

            if (hit)
            {
                // damage: str or int or lust
                damage = CalculateBaseDamage(ia, attacker, glancinghit);
                if (glancinghit) damage = (int)Math.Ceiling(damage * .75);

                // damage spread: tui and per
                int dmglow = (int)(damage * (1 - (.01 * _basespread)));
                int dmghigh = (int)(damage * (1 + (.01 * _basespread)));

                dmgspread = CalculateDamageSpreadPercent(ia, attacker);
                if (dmgspread > 90) dmgspread = 90;
                int diff = dmghigh - dmglow;
                int idk = (int)Math.Ceiling(dmglow + (diff * ((.01 * dmgspread))));
                damage = RNG.Seed.Next(idk, dmghigh + 1);

                // crit: per and wis/dex - flip on dtype
                critchance = CalculateCritChance(ia, attacker);
                if (critchance > _maxcrit) critchance = _maxcrit;
                critroll = RNG.Seed.Next(0, 101);
                critdamage = 0;
                if (critroll < critchance)
                {
                    crit = true;
                    critdamage = CalculateCritDamage(ia, damage, attacker);
                }

                totaldamage = damage + critdamage;

                if (tattacker.GetDamageType() == DamageTypes.Lust)
                    totaldamage = (int)Math.Ceiling(totaldamage * 2.0);
            }

            // stat mitigation
            mitigation = CalculateDamageMitigation(ia, !attacker);
            totaldamage = (int)(totaldamage * (1 + (.01 * mitigation)));

            // barrier
            int barriertotal = GetFixedStat(ia, StatTypes.Barrier, !attacker);
            if (barriertotal > 100) barriertotal = 100;
            totaldamage = (int)(totaldamage * (1 + (0.1 * barriertotal)));

            // shield
            int shieldtotal = GetFixedStat(ia, StatTypes.Shield, !attacker);
            int amountshielded = 0;
            int postshielddmg = totaldamage;
            if (shieldtotal > 0)
            {
                postshielddmg = totaldamage - shieldtotal;
                if (postshielddmg < 0)
                {
                    postshielddmg = 0;
                    amountshielded = totaldamage;
                }
                else
                {
                    amountshielded = shieldtotal;
                }
            }


            // apply damage
            bool orgasmed = false;
            int lustlifelost = 0;
            if (tattacker.GetDamageType() == DamageTypes.Lust)
            {
                int defenderLust = tdefender.GetMultipliedStat(StatTypes.CurrentLust);
                defenderLust = defenderLust + postshielddmg;
                int maxlust = tdefender.GetMultipliedStat(StatTypes.Lust);
                if (defenderLust >= maxlust)
                {
                    defenderLust = defenderLust - maxlust;
                    int defenderlife = tdefender.GetStat(StatTypes.CurrentLife);
                    lustlifelost = (int)(.45 * tdefender.GetMultipliedStat(StatTypes.Life));
                    defenderlife -= lustlifelost;
                    tdefender.Stats.SetStat(StatTypes.CurrentLife, defenderlife);
                    orgasmed = true;
                }

                tdefender.Stats.SetStat(StatTypes.CurrentLust, defenderLust);
            }
            else
            {
                int defenderlife = tdefender.Stats.GetStat(StatTypes.CurrentLife);
                defenderlife = defenderlife - postshielddmg;

                tdefender.Stats.SetStat(StatTypes.CurrentLife, defenderlife);
            }


            // apply attacker procs
            foreach (var debuff in attackerDebuffs)
            {
                int totalProcChance = 0;

                if (RNG.Seed.Next(0, 101) < totalProcChance)
                {
                    if (debuff.ProcTrigger == ProcTriggers.Crit && crit ||
                        debuff.ProcTrigger == ProcTriggers.Hit && hit)
                    {
                        EffectDetails ed = new EffectDetails(debuff.EffectId, debuff.Duration, EffectTypes.Debuff);
                        debuff.UserDetails = ed;
                        tdefender.ActiveEffects.Add(ed);
                    }
                }
            }

            if (attacker) ia.Defender = tdefender;

            // create reply string
            if (hit)
            {
                string hitstr = "hit";
                if (crit) hitstr = "[color=red][b]CRIT[/b][/color]";
                string skillstr = "";
                if (taskill != null) skillstr = $"{taskill.GetShortDescription()}";
                tosend += $"{tattacker.Alias} {hitstr} {tdefender.Alias} for {postshielddmg} {tattacker.GetDamageType()} damage";
                if (taskill != null) tosend += $" {skillstr}";
                if (barriertotal > 0) tosend += $" ({barriertotal}% absorbed)";
                if (shieldtotal > 0) tosend += $" ({amountshielded}% shielded)";
                tosend += ".";
                if (orgasmed)
                    tosend += $" {tdefender.Alias} was unable to resist their lust and had an orgasm, losing {lustlifelost} life!";

            }
            else
            {
                tosend += $"{tattacker.Alias} missed {tdefender.Alias}!";
            }


            return tosend;
        }

        public static void Attack(ImpendingAttack ia, string channel)
        {
            string outputstr = string.Empty;

            List<Effect> attackerdebuffsToApply = new List<Effect>();
            List<Effect> defenderdebuffsToApply = new List<Effect>();

            // apply any buff effects to attacker
            if (ia.AttackerSkill != null)
            {
                foreach (var e in ia.AttackerSkill.SkillEffects)
                {
                    Effect eft = DataDb.EffectDb.GetEffect(e);
                    if (eft.EffectType == Data.Enums.EffectTypes.Buff)
                    {
                        var ed = new EffectDetails(eft.EffectId, eft.Duration, Data.Enums.EffectTypes.Buff);
                        eft.UserDetails = ed;
                        ia.Attacker.ActiveEffects.Add(ed);
                    }
                    else attackerdebuffsToApply.Add(eft);
                }
            }

            // apply any buff effects to defender
            if (ia.DefenderSkill != null)
            {
                foreach (var e in ia.DefenderSkill.SkillEffects)
                {
                    Effect eft = DataDb.EffectDb.GetEffect(e);
                    if (eft.EffectType == Data.Enums.EffectTypes.Buff)
                    {
                        var ed = new EffectDetails(eft.EffectId, eft.Duration, Data.Enums.EffectTypes.Buff);
                        eft.UserDetails = ed;
                        ia.Defender.ActiveEffects.Add(ed);
                    }
                    else defenderdebuffsToApply.Add(eft);
                }
            }

            // healing
            int healing = 0;
            if (GetFixedStat(ia, StatTypes.Healing) > 0)
            {
                foreach (var debuff in ia.Attacker.GetActiveEffectByType(EffectTypes.Buff, 4))
                { // periodic damage
                    if (debuff.Stats.Stats.ContainsKey(StatTypes.Healing))
                    {
                        healing += debuff.UserDetails.HealingSnapshotValue;

                        int clife = ia.Attacker.GetStat(StatTypes.CurrentLife);
                        int mlife = ia.Attacker.GetMultipliedStat(StatTypes.Life);

                        clife += healing;
                        if (clife > mlife) clife = mlife;
                        ia.Attacker.Stats.SetStat(StatTypes.CurrentLife, clife);
                    }
                }
            }

            // debuff cleansing
            int debuffstocleanse = 0;
            if (GetFixedStat(ia, StatTypes.DebuffsToCleanse) > 0)
            {
                foreach (var debuff in ia.Attacker.GetActiveEffectByType(EffectTypes.Buff, 4))
                { // periodic damage
                    if (debuff.Stats.Stats.ContainsKey(StatTypes.DebuffsToCleanse))
                    {
                        debuffstocleanse += debuff.Stats.GetStat(StatTypes.DebuffsToCleanse);
                    }
                }
            }

            var dbl = ia.Attacker.GetActiveEffectByType(EffectTypes.Debuff).OrderBy(x => x.GetRemainingDuration()).ToList();
            for (int x = 0; x < debuffstocleanse; x++)
            {
                var tr = dbl.Last();
                ia.Attacker.ActiveEffects.Remove(tr.UserDetails);
                dbl.ToList().Remove(tr);
            }


            // apply active damage debuffs to attacker
            int damage = 0;

            foreach (var debuff in ia.Attacker.GetActiveEffectByType(EffectTypes.Debuff, 4))
            { // periodic damage
                if (debuff.Stats.Stats.ContainsKey(StatTypes.Damage) && debuff.UserDetails.DamageType != Data.Enums.DamageTypes.None)
                {
                    damage += debuff.UserDetails.DamageSnapshotValue;

                    int lustlifelost = 0;
                    if (debuff.UserDetails.DamageType == DamageTypes.Lust)
                    {
                        int defenderLust = ia.Attacker.GetMultipliedStat(StatTypes.CurrentLust);
                        defenderLust = defenderLust + damage;
                        int maxlust = ia.Attacker.GetMultipliedStat(StatTypes.Lust);
                        if (defenderLust >= maxlust)
                        {
                            defenderLust = defenderLust - maxlust;
                            int defenderlife = ia.Attacker.GetStat(StatTypes.CurrentLife);
                            lustlifelost = (int)(.45 * ia.Attacker.GetMultipliedStat(StatTypes.Life));
                            defenderlife -= lustlifelost;
                            ia.Attacker.Stats.SetStat(StatTypes.CurrentLife, defenderlife);
                            outputstr += $" {ia.Attacker.Alias} was unable to resist their lust and had an orgasm, losing {lustlifelost} life!";
                        }

                        ia.Attacker.Stats.SetStat(StatTypes.CurrentLust, defenderLust);
                    }
                    else
                    {
                        int defenderlife = ia.Attacker.Stats.GetStat(StatTypes.CurrentLife);
                        defenderlife = defenderlife - damage;

                        ia.Attacker.Stats.SetStat(StatTypes.CurrentLife, defenderlife);
                    }
                }
            }

            // check for attacker life
            if (ia.Attacker.GetStat(StatTypes.CurrentLife) <= 0)
            {
                ia.Attacker.Stats.SetStat(StatTypes.CurrentLife, 0);
                ia.Attacker.Stats.AddStat(StatTypes.Loss, 1);


                ia.Defender.Stats.AddStat(StatTypes.Win, 1);
                ia.Defender.Stats.AddStat(StatTypes.Kills, 20 * ia.Attacker.GetStat(StatTypes.Level));

                outputstr += $" {ia.Attacker.Alias} has been downed by {ia.Defender.Alias}!";

                // apply changes
                DataDb.CardDb.UpdateUserCard(ia.Defender);
                DataDb.CardDb.UpdateUserCard(ia.Attacker);

                // respond
                SystemController.Instance.Respond(channel, outputstr, ia.Attacker.Name);
                return;
            }

            // attack
            if (GetFixedStat(ia, StatTypes.Damage) > 0)
            {
                outputstr += CalculateSingleAttack(ia, attackerdebuffsToApply, true);
            }

            // check for defender life
            if (ia.Defender.GetStat(StatTypes.CurrentLife) <= 0)
            {
                ia.Defender.Stats.SetStat(StatTypes.CurrentLife, 0);
                ia.Defender.Stats.AddStat(StatTypes.Loss, 1);


                ia.Attacker.Stats.AddStat(StatTypes.Win, 1);
                ia.Attacker.Stats.AddStat(StatTypes.Kills, 20 * ia.Defender.GetStat(StatTypes.Level));

                outputstr += $" {ia.Defender.Alias} has been downed by {ia.Attacker.Alias}!";

                // apply changes
                DataDb.CardDb.UpdateUserCard(ia.Defender);
                DataDb.CardDb.UpdateUserCard(ia.Attacker);

                // respond
                SystemController.Instance.Respond(channel, outputstr, ia.Attacker.Name);
                return;
            }

            // healing
            healing = 0;
            if (GetFixedStat(ia, StatTypes.Healing, false) > 0)
            {
                foreach (var debuff in ia.Defender.GetActiveEffectByType(EffectTypes.Buff, 4))
                { // periodic damage
                    if (debuff.Stats.Stats.ContainsKey(StatTypes.Healing))
                    {
                        healing += debuff.UserDetails.HealingSnapshotValue;

                        int clife = ia.Defender.GetStat(StatTypes.CurrentLife);
                        int mlife = ia.Defender.GetMultipliedStat(StatTypes.Life);

                        clife += healing;
                        if (clife > mlife) clife = mlife;
                        ia.Defender.Stats.SetStat(StatTypes.CurrentLife, clife);
                    }
                }
            }

            // debuff cleansing
            debuffstocleanse = 0;
            if (GetFixedStat(ia, StatTypes.DebuffsToCleanse) > 0)
            {
                foreach (var debuff in ia.Defender.GetActiveEffectByType(EffectTypes.Buff, 4))
                { // periodic damage
                    if (debuff.Stats.Stats.ContainsKey(StatTypes.DebuffsToCleanse))
                    {
                        debuffstocleanse += debuff.Stats.GetStat(StatTypes.DebuffsToCleanse);
                    }
                }
            }

            dbl = ia.Defender.GetActiveEffectByType(EffectTypes.Debuff).OrderBy(x => x.GetRemainingDuration()).ToList();
            for (int x = 0; x < debuffstocleanse; x++)
            {
                var tr = dbl.Last();
                ia.Defender.ActiveEffects.Remove(tr.UserDetails);
                dbl.ToList().Remove(tr);
            }

            // check for defender counterattack
            bool counterattack = false;
            if (ia.DefenderSkill != null && ia.DefenderSkill.Stats.Stats.ContainsKey(StatTypes.Damage))
                counterattack = true;

            // if defender counterattack
            if (counterattack && GetFixedStat(ia, StatTypes.Damage, false) > 0)
            {
                CalculateSingleAttack(ia, defenderdebuffsToApply, false);
            }

            // check for attacker life
            if (ia.Attacker.GetStat(StatTypes.CurrentLife) <= 0)
            {
                ia.Attacker.Stats.SetStat(StatTypes.CurrentLife, 0);
                ia.Attacker.Stats.AddStat(StatTypes.Loss, 1);


                ia.Defender.Stats.AddStat(StatTypes.Win, 1);
                ia.Defender.Stats.AddStat(StatTypes.Kills, 20 * ia.Attacker.GetStat(StatTypes.Level));

                outputstr += $" {ia.Attacker.Alias} has been downed by {ia.Defender.Alias}!";
            }

            // apply changes
            DataDb.CardDb.UpdateUserCard(ia.Defender);
            DataDb.CardDb.UpdateUserCard(ia.Attacker);

            // respond
            SystemController.Instance.Respond(channel, outputstr, ia.Attacker.Name);

        }
    }
}