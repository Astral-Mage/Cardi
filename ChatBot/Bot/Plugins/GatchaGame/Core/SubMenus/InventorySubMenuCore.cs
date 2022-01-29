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
                    ShowInventoryAction(user);
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

        public void ShowInventoryAction(string user)
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
                    string tName;
                    tName = pc.Inventory[i].GetName();
                    

                    replyString += $"{pc.Inventory[i].GetRarityString()} {tName} {pc.Inventory[i].GetShortDescription()}";
                }

                if (i + 1 < pc.MaxInventory) replyString += $"\\n";
            }

            Respond(null, replyString, user);
        }

        public void UpgradeStuff(string user, string message, string channel)
        {
            // get user
            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
            {
                return;
            }

            if (message.Equals(CommandStrings.Help, StringComparison.InvariantCultureIgnoreCase))
            {
                UpgradeHelpAction(user);
                return;
            }

            // determine which item is to be upgraded
            if (!Int32.TryParse(message, out int res))
            {
                return;
            }

            if (res < 1) return;
            if (res > 3) return;

            SocketTypes st = SocketTypes.Active;
            if (res == 1) st = SocketTypes.Weapon;
            if (res == 2) st = SocketTypes.Armor;
            if (res == 3) st = SocketTypes.Passive;
            if (!card.ActiveSockets.Any(x => x.SocketType == st))
            {
                return;
            }

            Socket sock = card.ActiveSockets.First(x => x.SocketType == st);

            if (sock.SocketRarity >= sock.MaxRarity)
            {
                Respond(channel, $"Already Max Rarity.", card.Name);
                return;
            }

            // check if user has enough gold
            var costToLevel = Convert.ToInt32(sock.BaseLevelUpCost * (1.0 + (.2 * (int)sock.SocketRarity)));
            if (card.GetStat(StatTypes.Gld) >= costToLevel && sock.SocketRarity < sock.MaxRarity)
            {
                // if user has enough gold, call item's upgrade
                string extraInfo = sock.LevelUp();
                card.SetStat(StatTypes.Gld, card.GetStat(StatTypes.Gld) - costToLevel);
                Data.DataDb.UpdateCard(card);
                Respond(channel, $"Congratulations, {card.DisplayName}! You've upgraded up your {sock.NameOverride}'s rarity! {((string.IsNullOrWhiteSpace(extraInfo)) ? "" : "Gained " + extraInfo)}", card.Name);
            }
            else
            {
                Respond(channel, $"Sorry, {card.DisplayName}, but you need more gold to upgrade your {sock.NameOverride}. ([color=red]{card.GetStat(StatTypes.Gld)}[/color]/{costToLevel})", card.Name);
            }
        }

        public void InventoryHelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                $"         Welcome to [b][color={"red"}]{CommandStrings.Box} {CommandStrings.Help}[/color][/b]!" +
                $"\\n    " +
                $"\\nAll commands in this section are called in format: [color={"red"}]{CommandChar}{CommandStrings.Box} «Command» ⁕Value⁕[/color]." +
                $"\\nExample: Type [color={"red"}]{CommandChar}{CommandStrings.Box} {CommandStrings.AutoTrash} 1[/color]" +
                $"\\n" +
                $"\\n[{CommandStrings.Box}] is a system that allows you to manage the items you'll collect while using" +
                $"\\nthe Gatcha. Your box can store a maximum number of items. Once your box fills" +
                $"\\nup, additional items won't be saved, so make sure you're careful! You can use your" +
                $"\\nbox to delete items or upgrade items. You can not use the [set] command on items" +
                $"\\nin your box. Trashing an item refunds a small amount of stardust." +
                $"\\n" +
                $"\\nType [color={"red"}]{CommandChar}{CommandStrings.Box}[/color] to view your box." +
                $"\\n" +
                $"\\nThe list of [{CommandStrings.Box}] options are as follows:" +
                $"\\n" +
                $"\\n[color={"red"}]{CommandChar}{CommandStrings.Box} {CommandStrings.AutoTrashLong} ⁕Value⁕[/color] automatically trashes items this rarity or lower." +
                $"\\n[color={"red"}]{CommandChar}{CommandStrings.Box} {CommandStrings.Trash} ⁕Value⁕[/color] trashes the item in target box slot." +
                $"\\n         [sub][color={"red"}]{CommandChar}{CommandStrings.Box} {CommandStrings.Trash} ⁕Value⁕,⁕Value⁕,⁕Value⁕,...[/color] trashes multiple box slots at once.[/sub]" +
                $"\\n         [sub][color={"red"}]{CommandChar}{CommandStrings.Box} {CommandStrings.Trash} all[/color] trashes all box slots at once.[/sub]" +

                $"\\n" +
                $"\\n[color={"red"}]{CommandChar}{CommandStrings.Box} {CommandStrings.Upgrade} ⁕Value⁕[/color] Attempts to upgrade the item in the target box slot." +
                $"[/color]";

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
                        InventoryHelpAction(user);
                    }
                    break;
                case CommandStrings.AutoTrashLong:
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
                case CommandStrings.Upgrade:
                    {
                        UpgradeStuff(user, message, channel);
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
                        else if (message.Equals("all", StringComparison.InvariantCultureIgnoreCase))
                        {
                            InventoryTrashAction(user, null, channel);
                            return;
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

            if (values == null)
            {
                values = new List<int>();

                for (int i = 0; i <= pc.MaxInventory; i++)
                {
                    values.Add(i);
                }
            }

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
                Respond(channel, $"[/color]{pc.DisplayName}[color={BASE_COLOR}], you destroyed {totalItemsRemoved} item(s)" + $" ➤ [/color][b][color=black][color=purple]{totalConvertedStardust}[/color][/color] [color={BASE_COLOR}]Stardust", user);
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
                new Command(CommandStrings.AutoTrashLong, BotCommandRestriction.Whisper, CommandSecurity.None, "sets an autotrash rarity for converting items when rolling"),
                new Command(CommandStrings.Trash, BotCommandRestriction.Whisper, CommandSecurity.None, "converts an inventory item into stardust or gold"),
                new Command(CommandStrings.Upgrade, BotCommandRestriction.Whisper, CommandSecurity.None, "upgrades your sockets for gold"),
            };
        }
    }
}
