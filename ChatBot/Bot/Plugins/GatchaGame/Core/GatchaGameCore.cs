using Accord;
using ChatApi;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public partial class GatchaGame : PluginBase
    {
        public string BASE_COLOR = "white";

        /// <summary>
        /// handles all received messages, parsing them where appropriate
        /// </summary>
        /// <param name="command">command being sent, if any</param>
        /// <param name="channel">the source channel</param>
        /// <param name="message">the cleaned message</param>
        /// <param name="sendingUser">the user sending the message</param>
        /// <param name="isOp">if the user is an op</param>
        public override void HandleRecievedMessage(string command, string channel, string message, string sendingUser, bool isOp)
        {
            // handle non-command conversation
            if (string.IsNullOrWhiteSpace(command))
            {
                HandleNonCommand(channel, sendingUser, message);
                return;
            }

            // check if the command is an actual command
            if (!GetCommandList().Any(x => x.command.Equals(command)))
            {
                return;
            }

            // handle op commands
            if (HandleOpCommands(command, channel, isOp, message))
            {
                return;
            }

            // handle user commands
            if (HandleUserCommands(command, channel, message, sendingUser))
            {
                return;
            }
        }

        /// <summary>
        /// creates a list of valid commands for this plugin
        /// </summary>
        /// <returns></returns>
        public List<Command> GetCommandList()
        {
            return new List<Command>
            {
                new Command(CommandStrings.Help, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of help options"),
                new Command(CommandStrings.Create, BotCommandRestriction.Both, CommandSecurity.None, "creates a new character"),
                new Command(CommandStrings.Roll, BotCommandRestriction.Both, CommandSecurity.None, "rolls in the gatcha"),
                new Command(CommandStrings.Dive, BotCommandRestriction.Both, CommandSecurity.None, "dives into the dungeon"),
                new Command(CommandStrings.Set, BotCommandRestriction.Both, CommandSecurity.None, "handles various set commands"),
                new Command(CommandStrings.Inventory, BotCommandRestriction.Both, CommandSecurity.None, "handles various inventory commands"),
                new Command(CommandStrings.Card, BotCommandRestriction.Both, CommandSecurity.None, "displays details about the character"),

                new Command(CommandStrings.Equip, BotCommandRestriction.Both, CommandSecurity.None, "equips items"),
                new Command(CommandStrings.Unequip, BotCommandRestriction.Both, CommandSecurity.None, "unequips items"),
                new Command(CommandStrings.Divefloor, BotCommandRestriction.Both, CommandSecurity.None, "sets your default dive floor"),

            };
        }

        /// <summary>
        /// responsible for handling commands any op can access
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="channel">the source channel</param>
        /// <param name="isOp">if the user is an op</param>
        /// <param name="message">the cleaned up message</param>
        /// <returns>true if an op command was handled</returns>
        public bool HandleOpCommands(string command, string channel, bool isOp, string message)
        {
            return false;
        }

        public void CreateCharacter(string channel, string user)
        {
            if (Data.DataDb.UserExists(user))
                return;

            Cards.PlayerCard pc = new Cards.PlayerCard();
            pc.Name = user;
            pc.DisplayName = user;
            pc.MaxInventory = 10;

            pc.AvailableSockets.Add(SocketTypes.Weapon);
            pc.AvailableSockets.Add(SocketTypes.Armor);
            pc.AvailableSockets.Add(SocketTypes.Passive);
            pc.AvailableSockets.Add(SocketTypes.Active);

            RngGeneration.GenerateNewCharacterStats(pc);
            Data.DataDb.AddNewUser(pc);
            Respond(channel, $"Welcome, {user}. Type -help to learn how to play!", user);
        }

        /// <summary>
        /// responsible for handling commands any user can access
        /// </summary>
        /// <param name="command">the command</param>
        /// <param name="channel">the source channel</param>
        /// <param name="message">the cleaned up message</param>
        /// <returns>true if a user command was handled</returns>
        public bool HandleUserCommands(string command, string channel, string message, string user)
        {
            switch(command)
            {
                case CommandStrings.Roll:
                    {
                        if (string.IsNullOrWhiteSpace(message) || !int.TryParse(message, out int rollCount))
                        {
                            rollCount = 1;
                        }

                        RollAction(rollCount, user, channel);
                    }
                    break;
                case CommandStrings.Help:
                    {
                        HelpAction(user);
                    }
                    break;
                case CommandStrings.Create:
                    {
                        CreateCharacter(channel, user);
                    }
                    break;
                case CommandStrings.Dive:
                    {
                        DiveAction(channel, user, message);
                    }
                    break;
                case CommandStrings.Set:
                    {
                        SetAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Inventory:
                    {
                        InventoryAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Card:
                    {
                        CardAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Equip:
                    {
                        EquipAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Unequip:
                    {
                        UnequipAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Divefloor:
                    {
                        SetDiveFloorAction(channel, message, user);
                    }
                    break;
                case CommandStrings.Status:
                    {
                        int result = -1;
                        Status sta = Status.Online;

                        try
                        {
                            result = int.Parse(message.Split(' ').First());
                            sta = (Status)result;
                        }
                        catch (Exception)
                        {
                            Respond(channel, $"Unable to parse status. Please try again.", user);
                            return true;
                        }

                        Api.SetStatus(message.Split(" ".ToCharArray(), 2).Last(), sta, user);
                        Respond(channel, $"Setting your status to: [{sta.ToString()}] [{message.Split(" ".ToCharArray(), 2).Last()}]", user);
                    }
                    break;
            }

            return false;
        }

        public void SetDiveFloorAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res) || FloorDb.GetAllFloors().Count < res || res <= 0)
            {
                Respond(channel, $"{user}, you must specify a valid floor", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard pc))
                return;

            pc.SetStat(StatTypes.Dff, res);
            Data.DataDb.UpdateCard(pc);
            Respond(channel, $"{pc.DisplayName}, you'll now dive to floor {res} by default.", user);
        }

        public void EquipAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res))
            {
                Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: equip 1", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                return;

            if (card.Inventory.Count < res || res < 1)
            {
                Respond(channel, $"{user}, you must specify a valid box slot you're attempting to equip. Ex: equip 1", user);
                return;
            }

            Socket toAdd = card.Inventory[res - 1];
            if (!card.AvailableSockets.Contains(toAdd.SocketType))
            {
                Respond(channel, $"{user}, you must specify an equipment type you're allowed to equip. Ex: equip 1", user);
                return;
            }

            // CHECK FOR CLASS RESTRICTIONS HERE

            // ---------------------------------


            int numAllowed = card.AvailableSockets.Count(x => x == toAdd.SocketType);
            int numEquipped = card.ActiveSockets.Count(x => x.SocketType == toAdd.SocketType);
            Socket toRemove = null;

            if (numEquipped >= numAllowed)
            {
                toRemove = card.ActiveSockets.First(x => x.SocketType == toAdd.SocketType);
                card.ActiveSockets.Remove(toRemove);
                card.Inventory.Add(toRemove);
            }

            card.ActiveSockets.Add(toAdd);
            card.Inventory.Remove(toAdd);

            string replyMessage = $"{card.DisplayName}, you've equipped your {toAdd.GetName()}.";
            if (toRemove != null) replyMessage += $" You had to unequip your {toRemove.GetName()} to do so.";

            Data.DataDb.UpdateCard(card);
            Respond(channel, replyMessage, user);
        }

        public void UnequipAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!int.TryParse(message, out int res))
            {
                Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: unequip 1", user);
                return;
            }

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard card))
                return;

            if (res < 1)
            {
                Respond(channel, $"{user}, you must specify a valid equipment slot you're attempting to un-equip. Ex: unequip 1", user);
                return;
            }

            if (card.Inventory.Count >= card.MaxInventory)
            {
                Respond(channel, $"{user}, you must have free inventory space to unequip gear.", user);
                return;
            }

            Socket toUnequip = null;
            SocketTypes unequipType;
            if (res == 1)
                unequipType = SocketTypes.Weapon;
            else if (res == 2)
                unequipType = SocketTypes.Armor;
            else if (res == 3)
                unequipType = SocketTypes.Passive;
            else
                return;

            if (card.ActiveSockets.Count(x => x.SocketType == unequipType) > 0)
            {
                toUnequip = card.ActiveSockets.First(x => x.SocketType == unequipType);
            }
            else
            {
                return;
            }

            card.ActiveSockets.Remove(toUnequip);
            card.Inventory.Add(toUnequip);
            Data.DataDb.UpdateCard(card);
            Respond(channel, $"{card.DisplayName}, you've unequipped your {toUnequip.GetRarityString()} {toUnequip.GetName()}", user);
        }

        public void CardAction(string channel, string message, string user)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            Cards.PlayerCard pc;
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (!RngGeneration.TryGetCard(message, out pc))
                {
                    Respond(channel, $"Invalid user specified.", user);
                    return;
                }
                else
                {
                    RngGeneration.TryGetCard(user, out pc);
                }
            }
            else
            {
                RngGeneration.TryGetCard(user, out pc);
            }

            // display card
            string cardStr = string.Empty;

            string displayname = (string.IsNullOrWhiteSpace(pc.DisplayName)) ? pc.Name : pc.DisplayName;
            string species = (string.IsNullOrWhiteSpace(pc.SpeciesDisplayName)) ? ((SpeciesTypes)pc.GetStat(StatTypes.Sps)).GetDescription() : pc.SpeciesDisplayName;
            string cClass = (string.IsNullOrWhiteSpace(pc.ClassDisplayName)) ? ((ClassTypes) pc.GetStat(StatTypes.Cs1)).GetDescription() : pc.ClassDisplayName;

            //string upgradesStr = string.Empty;
            //int upgradesAvail = pc.GetStat(StatTypes.Upe) - pc.GetStat(StatTypes.Upg);
            //if (upgradesAvail > 0)
            //{
            //    upgradesStr += $" [sup][b][color=green]{upgradesAvail}⇡[/color][/b][/sup] ";
            //}

            cardStr += $"[sup][color=pink]({(int)(pc.GetStat(StatTypes.Sta) / 1200)}/{90})[/color][/sup] [b]Name: [/b]{displayname} [b]Species: [/b]{species} [b]Class: [/b]{cClass}" +
                $"\\n                      [sup][b]Lvl: [/b]{pc.GetStat(StatTypes.Lvl)} | [b]Gold: [/b]{pc.GetStat(StatTypes.Gld)} | [b]Stardust: [/b]{pc.GetStat(StatTypes.Sds)} | [b]Defeated: [/b]{pc.GetStat(StatTypes.Kil) + pc.GetStat(StatTypes.KiB)} 〰 [b]Stats: [/b]";


            cardStr += $"{StatTypes.Vit.GetDescription()} ‣ {pc.GetStat(StatTypes.Vit)}" + " | ";
            cardStr += $"{StatTypes.Atk.GetDescription()} ‣ {pc.GetStat(StatTypes.Atk)}" + " | ";
            cardStr += $"{StatTypes.Dmg.GetDescription()} ‣ {pc.GetStat(StatTypes.Dmg)}" + " | ";
            cardStr += $"{StatTypes.Con.GetDescription()} ‣ {pc.GetStat(StatTypes.Con)}" + " | ";
            cardStr += $"{StatTypes.Pdf.GetDescription()} ‣ {pc.GetStat(StatTypes.Pdf)}" + " | ";
            cardStr += $"{StatTypes.Mdf.GetDescription()} ‣ {pc.GetStat(StatTypes.Mdf)}" + " | ";
            cardStr += $"{StatTypes.Dex.GetDescription()} ‣ {pc.GetStat(StatTypes.Dex)}" + " | ";
            cardStr += $"{StatTypes.Int.GetDescription()} ‣ {pc.GetStat(StatTypes.Int)}" + " | ";
            cardStr += $"{StatTypes.Spd.GetDescription()} ‣ {pc.GetStat(StatTypes.Spd)}" + " | ";
            cardStr += $"{StatTypes.Eva.GetDescription()} ‣ {pc.GetStat(StatTypes.Eva)}" + " | ";
            cardStr += $"{StatTypes.Crc.GetDescription()} ‣ {pc.GetStat(StatTypes.Crc)}" + " | ";
            cardStr += $"{StatTypes.Crt.GetDescription()} ‣ {pc.GetStat(StatTypes.Crt)}";

            cardStr += "[/sup]";

            string boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Sharpness)) ? $"[color=cyan]•[/color]" : string.Empty;
            if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Weapon) > 0)
            {
                foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Weapon))
                {
                    cardStr += $"\\n                      {boonAddition} [b]🗡️ [/b]";
                    cardStr += $"{v.GetRarityString()} {v.GetName()}";
                }
            }
            else
            {
                cardStr += $"\\n                      {boonAddition} [b]🗡️ [/b]";
                cardStr += "Bare Hands";
            }

            boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Resiliance)) ? $"[color=cyan]•[/color]" : string.Empty;
            if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Armor) > 0)
            {
                foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Armor))
                {
                    cardStr += $"\\n                      {boonAddition} [b]🛡️  [/b]";
                    cardStr += $"{v.GetRarityString()} {v.GetName()}";
                }
            }
            else
            {
                cardStr += $"\\n                      {boonAddition} [b]🛡️  [/b]";
                cardStr += "Birthday Suit";
            }

            boonAddition = (pc.BoonsEarned.Contains(BoonTypes.Empowerment)) ? $"[color=cyan]•[/color]" : string.Empty;
            if (pc.ActiveSockets.Count(x => x.SocketType == SocketTypes.Passive) > 0)
            {

                foreach (var v in pc.ActiveSockets.Where(x => x.SocketType == SocketTypes.Passive))
                {
                    cardStr += $"\\n                      {boonAddition} [b]✨ [/b]";
                    cardStr += $"{v.GetRarityString()} {v.GetName()}";
                }
            }
            else
            {
                cardStr += $"\\n                      {boonAddition} [b]✨ [/b]";
                cardStr += "Bashful Gaze";
            }

            if (!string.IsNullOrWhiteSpace(pc.Signature))
            {
                cardStr += $"\\n                      {pc.Signature}";
            }

            Respond(channel, cardStr, user);
        }

        /// <summary>
        /// Sends a basic help blurb
        /// </summary>
        /// <param name="sendingUser">User that made the request</param>
        public void HelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += $"\\nType [color=pink]{CommandChar}create[/color] to register as an Adventurer!" +
                        $"\\nThis will give you a card, which you can see by typing[color=pink] {CommandChar}card[/color]!" +
                        $"\\n" +
                        $"\\nThe point of this game is to level up, dive, get gear, fight, and maybe lewd!" +
                        $"\\n If you type [color=pink]{CommandChar}dive[/color], you'll dive into the dungeon and have fun encounters." +
                        $"\\nThis costs stamina, which you regain over time and by roleplaying in the channel. (Anything using /me)" +
                        $"\\nYou can check the current progress of the dungeon floor with [color=pink]{CommandChar}prog[/color]";

            Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// handles parsing non-command interactions
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="user">source user</param>
        /// <param name="message">message the user sent</param>
        public void HandleNonCommand(string channel, string user, string message)
        {

        }

        /// <summary>
        /// replies via the f-list api
        /// </summary>
        /// <param name="channel">channel to reply to</param>
        /// <param name="message">message to reply with</param>
        /// <param name="recipient">person to reply to</param>
        public override void Respond(string channel, string message, string recipient)
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                recipient = string.Empty;
            }

            message = $"[color={BASE_COLOR}]{message}[/color]";

            Api.SendReply(channel, message, recipient);
        }

        /// <summary>
        /// our base constructor
        /// </summary>
        /// <param name="api">f-list api interface</param>
        public GatchaGame(ApiConnection api, string commandChar) : base(api, commandChar)
        {

        }
    }
}
