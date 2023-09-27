using Accord;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CardAction : BaseAction
    {
        public CardAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Both;
            AlternateNames.Add(ActionNames.c);
            RequiresRegisteredUser = true;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            string toSend = string.Empty;
            UserCard cardToUse = card;
            bool verbose = false;

            if (ao.Message.Contains("-v"))
            {
                verbose = true;
                ao.Message = ao.Message.Replace("-v", "");
            }

            if (!string.IsNullOrWhiteSpace(ao.Message) && DataDb.CardDb.UserExists(ao.Message))
            {
                cardToUse = DataDb.CardDb.GetCard(ao.Message);
            }

            // main stats
            string lifestr = $"Life: {cardToUse.Stats.GetStat(StatTypes.CurrentLife)}/{cardToUse.GetMultipliedStat(StatTypes.Life)}{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Life)}[/sup]")}";
            string expstr = $"[sub]Exp: {cardToUse.Stats.GetStat(StatTypes.Experience)}[/sub]";
            string killsstr = $"[sub]Kills: {cardToUse.Stats.GetStat(StatTypes.Kills)}[/sub]";
            string duststr = $"[sub]Stardust: {cardToUse.Stats.GetStat(StatTypes.Stardust)}[/sub]";

            string strstr = $"[sub]{StatTypes.Strength.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Strength)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Strength)}[/sup]")}";
            string dexstr = $"[sub]{StatTypes.Dexterity.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Dexterity)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Dexterity)}[/sup]")}";
            string constr = $"[sub]{StatTypes.Constitution.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Constitution)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Constitution)}[/sup]")}";

            string intstr = $"[sub]{StatTypes.Intelligence.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Intelligence)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Intelligence)}[/sup]")}";
            string wisstr = $"[sub]{StatTypes.Wisdom.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Wisdom)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Wisdom)}[/sup]")}";
            string perstr = $"[sub]{StatTypes.Perception.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Perception)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Perception)}[/sup]")}";

            string libstr = $"[sub]{StatTypes.Libido.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Libido)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Libido)}[/sup]")}";
            string chastr = $"[sub]{StatTypes.Charisma.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Charisma)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Charisma)}[/sup]")}";
            string tuistr = $"[sub]{StatTypes.Intuition.GetDescription().Capitalize()}: {cardToUse.GetMultipliedStat(StatTypes.Intuition)}[/sub]{(verbose != true ? "" : $"[sup]{cardToUse.Stats.GetStat(StatTypes.Intuition)}[/sup]")}";

            //   ⋯   

            toSend += $"                               [icon]{cardToUse.Name}[/icon]" +
                $"\\n                                               [ {cardToUse.Alias} ]  [ {cardToUse.CurrentTitle} ]" +
                $"\\n                                ⟨{cardToUse.Archetype.Name}⟩ ⟪{cardToUse.Spec.Name}⟫ ⟨{cardToUse.Calling.Name}⟩" +
                $"\\n                                               [color=orange][b]{expstr} {killsstr} {duststr}[/b][/color]" +
                $"\\n                                                  [color=green][b]{lifestr}[/b][/color]" +
                $"\\n                   [color=brown][b]{strstr} {dexstr} {constr}[/b][/color] [color=cyan][b]{intstr} {wisstr} {perstr}[/b][/color] [color=pink][b]{libstr} {chastr} {tuistr}[/b][/color]";

            string extra = $"\\n";

            // equipment
            if (cardToUse.ActiveSockets.Any(x => x.SocketType == SocketTypes.Weapon))
            {
                var weapon = cardToUse.ActiveSockets.First(x => x.SocketType == SocketTypes.Weapon);
                extra += $"\\n⚔️ {weapon.GetName()}{weapon.GetShortDescription()}";
            }

            if (cardToUse.ActiveSockets.Any(x => x.SocketType == SocketTypes.Armor))
            {
                var armor = cardToUse.ActiveSockets.First(x => x.SocketType == SocketTypes.Armor);
                extra += $"\\n🛡 {armor.GetName()}{armor.GetShortDescription()}";
            }

            if (cardToUse.ActiveSockets.Any(x => x.SocketType == SocketTypes.Passive))
            {
                var passive = cardToUse.ActiveSockets.First(x => x.SocketType == SocketTypes.Passive);
                extra += $"\\n💫 {passive.GetName()}{passive.GetShortDescription()}";
            }

            // construct skill list
            List<string> skillnames = new List<string>();
            string skilllist = string.Empty;
            if (cardToUse.Skills != null && cardToUse.Skills.Count > 0)
            {
                foreach (var v in cardToUse.Skills)
                {
                    Skill skill = DataDb.SkillsDb.GetSkill(Convert.ToInt32(v));
                    if (!skillnames.Contains(skill.Name)) skillnames.Add(skill.Name);
                }
            }
            if (cardToUse.Calling.Skill > -1)
            {
                var ta = DataDb.SkillsDb.GetSkill(cardToUse.Calling.Skill).Name;
                if (!skillnames.Contains(ta)) skillnames.Add(ta);
            }
            foreach (var s in cardToUse.Archetype.Skills)
            {
                var ta = DataDb.SkillsDb.GetSkill(s).Name;
                if (!skillnames.Contains(ta)) skillnames.Add(ta);
            }
            foreach (var s in cardToUse.Spec.Skills)
            {
                var ta = DataDb.SkillsDb.GetSkill(s).Name;
                if (!skillnames.Contains(ta))  skillnames.Add(ta);
            }
            foreach (var sn in skillnames)
            {
                skilllist += $"⟨ {sn} ⟩";
            }
            if (!string.IsNullOrWhiteSpace(skilllist)) extra += $"\\n❇️ {skilllist}";

            // buffs and debuffs
            List<string> enames = new List<string>();
            var eBuffs = cardToUse.GetPassiveEffectsByType(EffectTypes.Buff);
            if (eBuffs.Any()) eBuffs.ForEach(x => enames.Add(x.Name));

            if (enames.Any())
            {
                extra += $"\\n⏫ ";
                foreach (var v in enames)
                {
                    extra += $"⟪ {v} ⟫";
                }
            }

            // extra += $"\\n⏬ ";

            if (!verbose) toSend += extra;
            SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
        }
    }
}
