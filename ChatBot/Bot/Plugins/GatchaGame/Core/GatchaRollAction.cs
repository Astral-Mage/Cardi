using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public partial class GatchaGame : PluginBase
    {
        const int COST_TO_ROLL = 10;

        public void RollAction(int rollCount, string user, string channel)
        {
            if (!Data.DataDb.UserExists(user))
            {
                Respond(channel, $"You need to create a character first to roll in the gatcha.", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard pc))
                return;

            int starDust = pc.GetStat(StatTypes.Sds);
            if (starDust < COST_TO_ROLL)
            {
                Respond(channel, $"You need [color=red]{starDust}[/color][b][color=black][color=purple]/{COST_TO_ROLL}[/color][/color][/b] Stardust to roll in the gatcha. Collect more during events, from other players, or from diving.", user);
                return;
            }

            string replyString = string.Empty;

            // debug
            rollCount = 1;
            for (int curRoll = 0; curRoll < rollCount; curRoll++)
            {
                if (pc.Inventory.Count >= pc.MaxInventory)
                {
                    Respond(channel, $"Your inventory is full. Please clean up space before rolling.", user);
                    return;
                }

                if (starDust < COST_TO_ROLL)
                {
                    Respond(channel, $"You need [color=red]{starDust}[/color][b][color=black][color=purple]/{COST_TO_ROLL}[/color][/color][/b] Stardust to roll in the gatcha. Collect more during events, from other players, or from diving.", user);
                    break;
                }

                starDust -= COST_TO_ROLL;

                Socket generatedItem;
                int valRolled = -1;
                SocketTypes st = (SocketTypes)RngGeneration.Rng.Next(Enum.GetNames(typeof(SocketTypes)).Length);
                switch (st)
                {
                    case SocketTypes.Equipment:
                    case SocketTypes.Weapon:
                    case SocketTypes.Armor:
                        {
                            generatedItem = RngGeneration.GenerateRandomEquipment(out valRolled);
                        }
                        break;
                    case SocketTypes.Active:
                    case SocketTypes.Passive:
                        {
                            st = SocketTypes.Passive;
                            generatedItem = RngGeneration.GenerateRandomPassive(out valRolled);
                        }
                        break;
                    default:
                        throw new Exception("Error generating item. Contact admin.");
                }
                if (curRoll > 0) replyString += "\\n";
                replyString += $"{pc.DisplayName} rolled: [sup]({valRolled}/79)[/sup][b]{generatedItem.GetRarityString()} {generatedItem.GetName()}[/b] {generatedItem.GetShortDescription()}";

                if ((int)generatedItem.SocketRarity <= pc.GetStat(StatTypes.Adr))
                {
                    int convertedStardust;
                    if ((int)generatedItem.SocketRarity <= 15)
                        convertedStardust = 1;
                    else if ((int)generatedItem.SocketRarity <= 27)
                        convertedStardust = 2;
                    else
                        convertedStardust = 3;

                    replyString += $" ➤ [b][color=black][color=purple]{convertedStardust}[/color][/color][/b] Stardust";
                    starDust += convertedStardust;
                }
                else
                {
                    pc.Inventory.Add(generatedItem);
                }
            }
            pc.SetStat(StatTypes.Sds, starDust);
            Data.DataDb.UpdateCard(pc);
            Respond(channel, replyString, user);
        }
    }
}