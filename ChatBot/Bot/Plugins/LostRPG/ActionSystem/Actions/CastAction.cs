using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CombatSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CastAction : BaseAction
    {
        public CastAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = true;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            List<string> split = new List<string>();
            if (ao.Message.Contains(ao.CommandChar)) split = ao.Message.Split(ao.CommandChar.ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).ToList();
            else if (ao.Message.ToLowerInvariant().Contains("on")) split = ao.Message.ToLowerInvariant().Split(new string[] { "on" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            else split.Add(ao.Message.Trim());

            if (!DataDb.CardDb.UserExists(split.First().Trim()))
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"User {split.First()} not a valid target.", ao.User);
                return;
            }
            UserCard enemy = DataDb.CardDb.GetCard(split.First().Trim());

            // check for skills
            Skill skillToUse = null;
            if (split.Count > 1)
            {
                string stu = split.Last();
                foreach (var s in card.GetUsableSkills())
                {
                    if (s.Name.ToLowerInvariant().Equals(stu.Trim().ToLowerInvariant()))
                    {
                        skillToUse = s;
                        break;
                    }
                    else if (int.TryParse(stu, out int skillid) && s.SkillId == skillid)
                    {
                        skillToUse = s;
                        break;
                    }
                }
            }

            ImpendingAttack ia = new ImpendingAttack();
            ia.Attacker = card;
            ia.AttackerSkill = skillToUse;
            ia.Defender = enemy;

            bool badskill = false;
            if (CombatController.GetFixedStat(ia, CardSystem.UserData.StatTypes.Damage) > 0)
            {
                badskill = true;
            }

            foreach(var v in skillToUse.SkillEffects)
            {
                if (DataDb.EffectDb.GetEffect(v).EffectType == EffectTypes.Debuff)
                {
                    badskill = true;
                    break;
                }
            }

            if (badskill)
            {
                // respond and return

                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"This is an offensive skill. You must -attack with it.", ao.User);
                return;
            }

            CombatController.Cast(ia, ao.Channel, ia.Defender == null);
        }
    }
}
