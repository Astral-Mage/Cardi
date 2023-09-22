using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System;
using System.Collections.Generic;

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
            if (!string.IsNullOrWhiteSpace(ao.Message) && DataDb.CardDb.UserExists(ao.Message))
            {
                cardToUse = DataDb.CardDb.GetCard(ao.Message);
            }

            // construct skill list
            List<string> skillnames = new List<string>();
            string skilllist = string.Empty;
            if (cardToUse.Skills != null && cardToUse.Skills.Count > 0)
            {
                foreach (var v in cardToUse.Skills)
                {
                    Skill skill = DataDb.SkillsDb.GetSkill(Convert.ToInt32(v));
                    skillnames.Add(skill.Name);
                }
                skilllist = string.Join(" | ", skillnames);
            }


            toSend += $"" +
                $"\\n{cardToUse.Alias}      ⋯      [sup]⌈[/sup]{cardToUse.CurrentTitle}[sub]⌋[/sub]      ⋯      ⟪{cardToUse.Archetype.Name}⟫ ⟨{cardToUse.Spec.Name}⟩" +
                $"\\n[b]Life: {cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife)}/{cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Life)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Life)}[/sup] | Experience: {cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Experience)} | Kills: {cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Kills)} | Stardust: {cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Stardust)}[/b]" +
                $"\\n[color=brown][b]Strength: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Strength)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Strength)}[/sup]  Dexterity: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Dexterity)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Dexterity)}[/sup]  Constitution: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Constitution)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Constitution)}[/sup][/b][/color]" +
                $"\\n[color=cyan][b]Intelligence: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Intelligence)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Intelligence)}[/sup]  Wisdom: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Wisdom)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Wisdom)}[/sup]  Perception: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Perception)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Perception)}[/sup][/b][/color]" +
                $"\\n[color=pink][b]Libido: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Libido)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Libido)}[/sup]  Charisma: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Charisma)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Charisma)}[/sup]  Intuition: {cardToUse.GetMultipliedStat(CardSystem.UserData.StatTypes.Intuition)}[sup]{cardToUse.Stats.GetStat(CardSystem.UserData.StatTypes.Intuition)}[/sup][/b][/color]" +
                $"\\nWeapon: " +
                $"\\nArmor: " +
                $"\\nSpecial: " +
                $"\\nBuffs: " +
                $"\\nDebuffs: " +
                $"\\nSkills: {skilllist}";
            SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
        }
    }
}
