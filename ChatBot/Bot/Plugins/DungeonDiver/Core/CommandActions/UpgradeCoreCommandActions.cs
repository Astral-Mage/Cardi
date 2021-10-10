using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins
{
    public partial class GameCore : PluginBase
    {
        /// <summary>
        /// basic help summary of upgrade sub menu
        /// </summary>
        /// <param name="sendingUser">user requesting help</param>
        public void UpgradeHelpAction(string sendingUser)
        {
            string toSend = "\\nUpgrading your equipment takes gold! You collect gold by using the [color=pink]-dive[/color] command to go dungeon diving!" +
                "\\nOnce you have gold, you can upgrade any slot you want by typing [color=pink]-upgrade equipment[/color]." +
                "\\n\\nYou have three equipment slots: [color=orange]weapon, gear, special[/color]." +
                "\\n For example, if you wanted to upgrade your weapon you would type [color=pink]-upgrade weapon[/color]" +
                "\\n\\nYou can tell your equipment's current upgrade level by the number beside it! +2 means your weapon is level 2." +
                "\\nUpgraded equipment helps you gain more [color=green]progress[/color] as you're diving in the dungeon." +
                "\\n\\nLevel 1 upgrade costs [color=yellow]50[/color] gold. Level 2 costs [color=yellow]200[/color]. Level 3 costs [color=yellow]450[/color], and it keeps going up from there!" +
                "\\n\\nApparently you can even find rare [color=cyan]boons[/color] for your equipment if you get lucky enough~" +
                "\\nIf you find a boon for your weapon, it will show your bonus in [color=cyan]cyan[/color] on your card.";

            Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// handles upgrade an item if there's enough gold
        /// </summary>
        /// <param name="pc">user requesting the upgrade</param>
        /// <param name="itemType">type of item being upgraded</param>
        public void UpgradeItemAction(PlayerCard pc, ItemType itemType)
        {
            int goldNeeded = 0;
            int currentItemLevel = 0;
            string customItemString = string.Empty;
            string nickname = (string.IsNullOrWhiteSpace(pc.nickname)) ? $"[color=white]{pc.name}[/color]" : $"[color=white]{pc.nickname}[/color]";

            // collect info
            if (itemType == ItemType.Weapon)
            {
                currentItemLevel = pc.weaponlvl;
                customItemString = pc.weapon;
            }
            if (itemType == ItemType.Gear)
            {
                currentItemLevel = pc.gearlvl;
                customItemString = pc.gear;
            }
            if (itemType == ItemType.Special)
            {
                currentItemLevel = pc.speciallvl;
                customItemString = pc.special;
            }

            // check gold requirement and if we have enough
            goldNeeded += UpgradeBaseCost * (int)Math.Pow(currentItemLevel + 1, 2);

            // if we leveled up
            if (pc.gold >= goldNeeded)
            {
                if (itemType == ItemType.Weapon)
                {
                    pc.weaponlvl += 1;
                }
                if (itemType == ItemType.Gear)
                {
                    pc.gearlvl += 1;
                }
                if (itemType == ItemType.Special)
                {
                    pc.speciallvl += 1;
                }

                pc.gold -= goldNeeded;
                GameDb.UpdateCard(pc);
                Respond(null, $"Congratulations, {nickname}! You have leveled your {Enum.GetName(typeof(ItemType), itemType)} up to level: [b]{currentItemLevel + 1}[/b]. You spent [b][color=yellow]{goldNeeded}[/color][/b] gold to upgrade your {customItemString}.", pc.name);
            }
            else
            {
                Respond(null, $"Sorry, {nickname}. You don't have the required gold [color=red]{pc.gold}[/color] [b]/ [color=yellow]{goldNeeded}[/color][/b] to upgrade your {customItemString} further.", pc.name);
            }
        }
    }
}
