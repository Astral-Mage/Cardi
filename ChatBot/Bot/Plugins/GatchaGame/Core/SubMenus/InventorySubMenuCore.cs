using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public partial class GatchaGame : PluginBase
    {
        public void InventoryAction(string channel, string message, string user)
        {
            Command cmder;
            string cmd = message.Split(' ').First();

            if (!Data.DataDb.UserExists(user))
            {
                Respond(channel, $"You need to create a character first to roll in the gatcha.", user);
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    ShowInventoryAction(user, channel);
                    return;
                }

                cmder = GetInventorySubCommandList().First(x => x.command.Equals(cmd, StringComparison.OrdinalIgnoreCase));
                if (cmder == null)
                {
                    Respond(null, $"I didn't understand your command. Use -box help to see all available commands!", user);
                }

                if (message.Split(' ').Length > 1)
                {
                    message = message.Split(new[] { ' ' }, 2).Last();
                }
                else
                {
                    message = "";
                }
            }
            catch(Exception)
            {
                Respond(null, $"I didn't understand your command. Use -box help to see all available commands!", user);
                return;
            }

            InventorySubMenu(GetInventorySubCommandList().First(x => x.command.Equals(cmd)), message, user, channel);
        }

        public void ShowInventoryAction(string user, string channel)
        {
            RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);

            string replyString = $"[b]- {pc.DisplayName} -[/b]\\n";

            for (int i = 0; i < pc.MaxInventory; i++)
            {
                if (i + 1 <= 9)
                    replyString += "  ";
                else if ((i + 1) % 10 == 1)
                    replyString += " ";

                replyString += $"        {i+1} ";
                if (pc.Inventory.Count > i)
                {
                    string tName = string.Empty;
                    tName = pc.Inventory[i].GetName();
                    

                    replyString += $"{pc.Inventory[i].GetRarityString()} {tName} {pc.Inventory[i].GetShortDescription()}";
                }

                if (i + 1 < pc.MaxInventory) replyString += $"\\n";
            }

            Respond(null, replyString, user);
        }

        public void InventoryHelpAction(string sendingUser)
        {
            string toSend = "[b]Inventory Help[/b]\\nThis is where you can customize your card to your liking!";
            Respond(null, toSend, sendingUser);
        }

        public void InventorySubMenu(Command command, string message, string user, string channel)
        {
            message = message.Replace("&gt;", ">");
            message = message.Replace("&lt;", "<");

            if (message.Contains("\\n"))
            {
                Respond(null, $"Sorry, but you can't insert newlines into any set commands!", user);
                return;
            }

            switch (command.command)
            {
                case CommandStrings.Help:
                    {
                        SetHelpAction(user);
                    }
                    break;
                case CommandStrings.AutoTrash:
                    {
                        if (!int.TryParse(message.Trim(), out int rarity))
                        {
                            Respond(channel, $"Please enter a valid rarity level. Example: box autotrash 3", user);
                            return;
                        }

                        InventoryAutoTrashAction(user, rarity);
                        Respond(channel, $"Gatcha Autotrash Rarity set to: {rarity}. Items of this rarity or lower will be auto-converted to currency.", user);
                    }
                    break;
                case CommandStrings.Trash:
                    {
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            Respond(channel, "Please enter a valid number value.", user);
                            return;
                        }
                        List<int> iVals = new List<int>();

                        if (message.Contains(','))
                        {
                            message = message.Replace(" ","");
                            List<string> vals = message.Split(',').ToList();
                            vals.ForEach(x => {
                                if (!int.TryParse(x, out int res))
                                    return;

                                iVals.Add(res);
                                });

                        }
                        else if (int.TryParse(message.Trim(), out int val))
                        {
                            iVals.Add(val);
                        }
                        else
                        {
                            Respond(channel, "Please enter a valid number value.", user);
                            return;
                        }

                        InventoryTrashAction(user, iVals, channel);
                    }
                    break;
            }
        }

        public void InventoryTrashAction(string user, List<int> values, string channel)
        {
            RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);
            values = values.OrderByDescending(x => x).ToList();
            int totalConvertedStardust = 0;
            int totalItemsRemoved = 0;
            List<Socket> toRemove = new List<Socket>();
            foreach (var value in values)
            {
                if (pc.MaxInventory < value)
                {
                    if (values.Count == 1) Respond(channel, $"Your inventory isn't that large.", user);
                    continue;
                }

                if (pc.Inventory.Count < value)
                {
                    if (values.Count == 1) Respond(channel, $"That slot is already empty.", user);
                    continue;
                }
                if (value <= 0)
                    continue;

                var item = pc.Inventory[value - 1];
                int convertedStardust;
                if ((int)item.SocketRarity <= 15)
                    convertedStardust = 1;
                else if ((int)item.SocketRarity <= 27)
                    convertedStardust = 2;
                else
                    convertedStardust = 3;

                totalConvertedStardust += convertedStardust;
                totalItemsRemoved++;

                toRemove.Add(item);
                pc.SetStat(StatTypes.Sds, pc.GetStat(StatTypes.Sds) + convertedStardust);
                if (values.Count == 1) Respond(channel, $"{pc.DisplayName}, you destroyed your {item.GetRarityString()}{item.GetName()}" + $" ➤ [b][color=black][color=purple]{convertedStardust}[/color][/color] Stardust", user);
            }

            foreach (var v in toRemove)
            {
                pc.Inventory.Remove(v);
            }
            Data.DataDb.UpdateCard(pc);

            if (values.Count > 1)
                Respond(channel, $"{pc.DisplayName}, you destroyed {totalItemsRemoved} item(s)" + $" ➤ [b][color=black][color=purple]{totalConvertedStardust}[/color][/color] Stardust", user);
        }

        public void InventoryAutoTrashAction(string user, int value)
        {
            RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);
            pc.SetStat(Enums.StatTypes.Adr, value);
            Data.DataDb.UpdateCard(pc);
        }

        List<Command> GetInventorySubCommandList()
        {
            return new List<Command>
            {
                new Command(CommandStrings.Help, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of help options"),
                new Command(CommandStrings.AutoTrash, BotCommandRestriction.Whisper, CommandSecurity.None, "sets an autotrash rarity for converting items when rolling"),
                new Command(CommandStrings.Trash, BotCommandRestriction.Whisper, CommandSecurity.None, "converts an inventory item into stardust or gold"),
            };
        }
    }
}
