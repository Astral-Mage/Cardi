using ChatApi;
using ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem;
using ChatBot.Bot.Plugins.LostRPG.RoleplaySystem;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChatBot.Bot.Plugins.LostRPG
{
    public partial class LostRPG : PluginBase
    {
        /// <summary>
        /// just some random base vars
        /// </summary>
        public string BaseColor = "white";

        /// <summary>
        /// grabbing our assembly types early
        /// </summary>
        private readonly Type[] AssTypes = Assembly.GetExecutingAssembly().GetTypes();

        private readonly List<Type> AssActionList;

        /// <summary>
        /// handles all non-commands
        /// </summary>
        /// <param name="channel">the source channel</param>
        /// <param name="user">the sending user</param>
        /// <param name="message">the recieved message</param>
        public void HandleNonCommand(string channel, string user, string message)
        {
            DialogueController.Instance.TryProgressDialogue(user, message, DialogueSystem.Enums.DialogueType.Conversation, string.IsNullOrWhiteSpace(channel) ? DialogueSystem.Enums.DialogueLocale.Private : DialogueSystem.Enums.DialogueLocale.Public);

            if (message.ToLowerInvariant().StartsWith("/me"))
            {
                if (DataDb.CardDb.UserExists(user))
                {
                    RoleplayController.Instance.ParsePost(user, message.Replace("/me", ""), channel);


                }
            }
        }

        //public bool CheckForInlinePalCommand(string post)
        //{
        //    var pal = DataDb.Instance.GetPointActionsDictionary();
        //    List<string> foundCommands = new List<string>();
        //    List<string> words = post.Split(' ').ToList();
        //
        //    foreach (string word in words)
        //    {
        //        if (word.StartsWith(CommandChar) && pal.Any(x => x.actionName.ToLowerInvariant().Equals(word.Replace(CommandChar, ""))))
        //            foundCommands.Add(word.Replace(CommandChar, ""));
        //    }
        //
        //    if (foundCommands.Any())
        //        return true;
        //
        //    return false;
        //}

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
            // handle inline commands and roleplay
            if (string.IsNullOrWhiteSpace(command))
            {
                HandleNonCommand(channel, sendingUser, message);
                return;
            }

            if (AssActionList.Any(x => x.Name.Equals(command + "Action", StringComparison.InvariantCultureIgnoreCase)))
            {
                AssActionList.Where(x => x.Name.Equals(command + "Action", StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach((curAction) =>
                {
                    BaseAction inst = (BaseAction)Activator.CreateInstance(curAction);
                    ExecuteAction(channel, message, sendingUser, isOp, inst);
                    return;
                });
            }
            else
            {
                var aa = AssActionList.Where(x =>
                {
                    BaseAction inst = (BaseAction)Activator.CreateInstance(x);
                    if (inst.AlternateNames.Any(y => y.Equals(command, StringComparison.InvariantCultureIgnoreCase))) return true;
                    return false;
                });

                if (aa.Any())
                {
                    BaseAction inst = (BaseAction)Activator.CreateInstance(aa.First());
                    ExecuteAction(channel, message, sendingUser, isOp, inst);
                    return;
                }
            }
        }

        /// <summary>
        /// Attempts to execute a specific action.
        /// </summary>
        /// <param name="channel">the source channel</param>
        /// <param name="message">the cleaned message</param>
        /// <param name="sendingUser">the user sending the message</param>
        /// <param name="isOp">if the user is an op</param>
        /// <param name="action">the action being performed</param>

        public void ExecuteAction(string channel, string message, string sendingUser, bool isOp, BaseAction action)
        {
            // valid user check
            if (!UserActionValidationCheck(sendingUser, action, out UserCard card))
            {
                SystemController.Instance.Respond(null, $"Sorry, but you must be an active user to use this command! Create a character by using the {CommandChar}create command.", sendingUser);
                return;
            }

            // valid security check
            if (action.SecurityType == Data.Enums.CommandSecurity.Ops && !isOp)
            {
                SystemController.Instance.Respond(null, "Sorry, but you must be a channel op to use that command.", sendingUser);
                return;
            }

            action.Execute(new ActionObject() { Channel = channel, CommandChar = CommandChar, User = sendingUser, Message = message }, card);
        }

        /// <summary>
        /// Checked whether or not a user exists for a called action.
        /// </summary>
        /// <param name="user">sending user</param>
        /// <param name="action">action being performed</param>
        /// <returns>true of user is valid</returns>
        public bool UserActionValidationCheck(string user, BaseAction action, out UserCard card)
        {
            card = null;
            if (!DataDb.CardDb.UserExists(user))
            {
                if (action.RequiresRegisteredUser == false)
                    return true;
                else
                    return false;
            }

            card = DataDb.CardDb.GetCard(user);
            
            return true;
        }

        /// <summary>
        /// our base constructor
        /// </summary>
        /// <param name="api">f-list api interface</param>
        public LostRPG(ApiConnection api, string commandChar) : base(api, commandChar)
        {
            AssActionList = AssTypes.Where(x => x.BaseType == typeof(BaseAction) && x.Name != typeof(BaseAction).Name).ToList();
        }

        /// <summary>
        /// our currently active channels
        /// </summary>
        readonly List<string> ActiveChannels = new List<string>();

        /// <summary>
        /// called whenever we've joined a channel
        /// </summary>
        /// <param name="channel">the channel we've joined</param>
        public override void HandleJoinedChannel(string channel)
        {
            ActiveChannels.Add(channel);
        }
    }
}
