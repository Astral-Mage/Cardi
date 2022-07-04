using ChatApi;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Enums;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.DialogueSystem
{
    /// <summary>
    /// our dialogue system
    /// </summary>
    public class DialogueController
    {
        /// <summary>
        /// instance of the ids
        /// </summary>
        static DialogueController iDS;

        /// <summary>
        /// our thread locker
        /// </summary>
        static readonly object sLock = new object();

        /// <summary>
        /// get our instance
        /// </summary>
        public static DialogueController Instance
        {
            get 
            { 
                lock(sLock)
                {
                    if (iDS == null)
                    {
                        iDS = new DialogueController();
                    }
                    return iDS;
                }
            }
        }

        private string CommandChar;

        public void SetCommandChar(string commandChar)
        {
            CommandChar = commandChar;
        }

        /// <summary>
        /// private entry point
        /// </summary>
        DialogueController()
        {
            ActiveDialogues = new List<Dialogue.Dialogue>();
            CommandChar = string.Empty;
        }

        //////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////

        /// <summary>
        /// Currently ongoing dialogues to monitor
        /// </summary>
        readonly List<Dialogue.Dialogue> ActiveDialogues;

        /// <summary>
        /// attempts to start a new dialogue
        /// </summary>
        /// <param name="dId">our dialogue id</param>
        /// <param name="user">our requesting user</param>
        public void StartDialogue(Type dId, string user)
        {
            Dialogue.Dialogue d = null;
            if (!ActiveDialogues.Any(x => x.Id.Equals(dId.GetHashCode()) && x.Owner.Equals(user)))
            {
                d = (Dialogue.Dialogue)Activator.CreateInstance(dId, new object[] { user, CommandChar });

                if (d.TypeOfDialogue == DialogueType.Conversation)
                {
                    if (ActiveDialogues.Any(x => x.TypeOfDialogue == DialogueType.Conversation && x.Locale == d.Locale))
                    {
                        SystemController.Instance.Respond(null, $"Sorry, please finish your other conversation to initiate this one: {ActiveDialogues.First(x => x.TypeOfDialogue == DialogueType.Conversation && x.Locale == d.Locale).ChildType.Name}", user);
                        return;
                    }
                }

                ActiveDialogues.Add(d);
                d.Progress();

                if (d.Status == DialogueStatus.Complete)
                {
                    ActiveDialogues.Remove(d);
                }

                return;
            }
        }

        public int TryProgressDialogue(string user, string message, DialogueType dialogueType, DialogueLocale locale)
        {
            List<Dialogue.Dialogue> dProgressed = new List<Dialogue.Dialogue>();

            ActiveDialogues.Where(x => x.TypeOfDialogue == dialogueType && x.Locale == locale && x.Owner == user).ToList().ForEach(dialogue =>
           {
               if (dialogue.Progress(message))
               {
                   dProgressed.Add(dialogue);
               }
           });

            dProgressed.Where(dialogue => dialogue.Status == DialogueStatus.Complete).ToList().ForEach(dialogue => ActiveDialogues.Remove(dialogue));
            return dProgressed.Count;
        }
    }
}
