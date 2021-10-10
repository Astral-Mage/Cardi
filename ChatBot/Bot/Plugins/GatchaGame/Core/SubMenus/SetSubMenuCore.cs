using Accord;
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
        public void SetAction(string channel, string message, string user)
        {
            Command cmder;
            string cmd = message.Split(' ').First();

            if (!Data.DataDb.UserExists(user))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                cmd = CommandStrings.Help;
            }

            try
            {
                cmder = GetSetSubCommandList().First(x => x.command.Equals(cmd, StringComparison.OrdinalIgnoreCase));
                if (cmder == null)
                {
                    Respond(null, $"I didn't understand your command. Use set help to see all available commands!", user);
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
            catch
            {
                Respond(null, $"I didn't understand your command. Use set help to see all available commands!", user);
                return;
            }

            SetSubMenu(GetSetSubCommandList().First(x => x.command.Equals(cmd)), message, user, channel);
        }

        public void SetHelpAction(string sendingUser)
        {
            string toSend = "[b]Set Help[/b]\\nThis is where you can customize your card to your liking!";
            Respond(null, toSend, sendingUser);
        }

        public void SetSubMenu(Command command, string message, string user, string channel)
        {
            if (message.Length > 300)
            {
                Respond(null, $"Sorry, but you're over the character limit: [color=red]{message.Length}[/color]/300", user);
                return;
            }

            message = message.Replace("&gt;", ">");
            message = message.Replace("&lt;", "<");

            //if (message.Contains('\'')) message = message.Replace("\'", "\'\'");
            //if (message.Contains('\"')) message = message.Replace("\"", "");

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
                case CommandStrings.Weapon:
                    {
                        RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);
                        if (!(pc.ActiveSockets.Count(x => x.SocketType == Enums.SocketTypes.Weapon) == 1))
                            return;
                        WeaponSocket ws = (pc.ActiveSockets.First(x => x.SocketType == Enums.SocketTypes.Weapon) as WeaponSocket);
                        Respond(channel, $"{pc.DisplayName}, you've renamed your {ws.GetName()} to {(string.IsNullOrWhiteSpace(message) ? ws.GetName(false) : message)}", user);
                        ws.NameOverride = message;
                        Data.DataDb.UpdateCard(pc);
                    }
                    break;
                case CommandStrings.Armor:
                    {
                        RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);
                        if (!(pc.ActiveSockets.Count(x => x.SocketType == Enums.SocketTypes.Armor) == 1))
                            return;
                        ArmorSocket ws = (pc.ActiveSockets.First(x => x.SocketType == Enums.SocketTypes.Armor) as ArmorSocket);
                        Respond(channel, $"{pc.DisplayName}, you've renamed your {ws.GetName()} to {(string.IsNullOrWhiteSpace(message) ? ws.GetName(false) : message)}", user);
                        ws.NameOverride = message;
                        Data.DataDb.UpdateCard(pc);
                    }
                    break;
                case CommandStrings.Special:
                    {
                        RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);
                        if (!(pc.ActiveSockets.Count(x => x.SocketType == Enums.SocketTypes.Passive) == 1))
                            return;
                        PassiveSocket ws = (pc.ActiveSockets.First(x => x.SocketType == Enums.SocketTypes.Passive) as PassiveSocket);
                        Respond(channel, $"{pc.DisplayName}, you've renamed your {ws.GetName()} to {(string.IsNullOrWhiteSpace(message) ? ws.GetName(false) : message)}", user);
                        ws.NameOverride = message;
                        Data.DataDb.UpdateCard(pc);
                    }
                    break;
                case CommandStrings.Class:
                    {
                        RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);

                        if (string.IsNullOrWhiteSpace(message))
                        {
                            pc.ClassDisplayName = string.Empty;
                            Data.DataDb.UpdateCard(pc);

                            return;
                        }
                        pc.ClassDisplayName = message;
                        ClassTypes baseClassStr = (ClassTypes)pc.GetStat(StatTypes.Cs1);
                        Respond(channel, $"{pc.DisplayName}, {((string.IsNullOrWhiteSpace(message)) ? $"you've reverted your class name back to {baseClassStr.GetDescription()}" : $"you've customized your class name to {message}.")}", user);
                        Data.DataDb.UpdateCard(pc);

                    }
                    break;
                case CommandStrings.Species:
                    {
                        RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);

                        if (string.IsNullOrWhiteSpace(message))
                        {
                            pc.SpeciesDisplayName = string.Empty;
                            Data.DataDb.UpdateCard(pc);

                            return;
                        }
                        pc.SpeciesDisplayName = message;
                        SpeciesTypes baseSpeciesStr = (SpeciesTypes)pc.GetStat(StatTypes.Sps);
                        Respond(channel, $"{pc.DisplayName}, {((string.IsNullOrWhiteSpace(message)) ? $"you've reverted your species name back to {baseSpeciesStr.GetDescription()}" : $"you've customized your species name to {message}.")}", user);
                        Data.DataDb.UpdateCard(pc);

                    }
                    break;
                case CommandStrings.Nickname:
                    {
                        RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);

                        if (string.IsNullOrWhiteSpace(message))
                        {
                            pc.DisplayName = pc.Name;
                            Data.DataDb.UpdateCard(pc);
                            return;
                        }
                        pc.DisplayName = message;
                        Respond(channel, $"{pc.DisplayName}, {((string.IsNullOrWhiteSpace(message)) ? $"you've reverted your name back to {pc.Name}" : $"you've customized your name to {message}.")}", user);
                        Data.DataDb.UpdateCard(pc);

                    }
                    break;
                case CommandStrings.Signature:
                    {
                        RngGeneration.TryGetCard(user, out Cards.PlayerCard pc);

                        if (string.IsNullOrWhiteSpace(message))
                        {
                            pc.Signature = string.Empty;
                            Data.DataDb.UpdateCard(pc);
                            return;
                        }

                        pc.Signature = message;
                        Respond(channel, $"{pc.DisplayName}, {((string.IsNullOrWhiteSpace(message)) ? $"you've deleted your signature." : $"you've customized your signature to {message}.")}", user);
                        Data.DataDb.UpdateCard(pc);
                    }
                    break;
            }
        }

        List<Command> GetSetSubCommandList()
        {
            return new List<Command>
            {
                new Command(CommandStrings.Help, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of help options"),
                new Command(CommandStrings.Weapon, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of Weapon options"),
                new Command(CommandStrings.Armor, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of Armor options"),
                new Command(CommandStrings.Special, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of Special options"),
                new Command(CommandStrings.Species, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of Species options"),
                new Command(CommandStrings.Class, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of Class options"),
                new Command(CommandStrings.Nickname, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of Nickname options"),
                new Command(CommandStrings.Signature, BotCommandRestriction.Whisper, CommandSecurity.None, "returns the list of Signature options"),
            };
        }
    }
}
