using ChatApi;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Enums;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Dialogue
{

    public class Dialogue
    {
        public int Id { get; private set; }

        public string Owner { get; private set; }

        protected int CurrentStep { get; set; }

        public void GetTotalSteps() { }

        public void GetCurrentStep() { }

        public DialogueStatus Status { get; set; }

        public int MaxSteps { get; protected set; }

        public Type ChildType { get; protected set; }

        public DialogueLocale Locale { get; set; }

        public DialogueType TypeOfDialogue { get; set; }

        protected bool BackingUp { get; set; }

        protected Dictionary<int, string> StoredArgs { get; set; }

        protected string CommandChar = string.Empty;
        
        public string Channel { get; set; }



        protected Dialogue(int id, string owner, string commandChar, string channel) 
        {
            CurrentStep = 0;
            Id = id;
            Owner = owner;
            Status = DialogueStatus.Inactive;
            MaxSteps = 0;
            Locale = DialogueLocale.Private;
            TypeOfDialogue = DialogueType.Command;
            BackingUp = false;
            StoredArgs = new Dictionary<int, string>();
            CommandChar = commandChar;
            Channel = channel;
        }

        public virtual void GoToStep(int step)
        {

        }

        public virtual bool Progress(string args = null)
        {
            if (Status == DialogueStatus.Complete)
            {
                return false;
            }
            else if (Status == DialogueStatus.Inactive)
            {
                Status = DialogueStatus.Active;
                SystemController.Instance.Respond(null, $"Starting Dialogue: {ChildType.Name}.", Owner);
            }

            if (!string.IsNullOrWhiteSpace(args) && args.StripPunctuation().ToLowerInvariant().Equals("cancel"))
            {
                Status = DialogueStatus.Complete;
                SystemController.Instance.Respond(null, $"Dialogue {ChildType.Name} cancelled.", Owner);
                return true;
            }
            else if (!string.IsNullOrWhiteSpace(args) && args.StripPunctuation().ToLowerInvariant().Equals("repeat"))
            {
                if (CurrentStep <= 1)
                {
                    args = null;
                    CurrentStep = 0;
                }
                else
                {
                    args = StoredArgs[CurrentStep];
                    CurrentStep -= 1;
                }
            }

            CurrentStep += 1;
            if (StoredArgs.Count < CurrentStep) StoredArgs.Add(CurrentStep, args);
            else StoredArgs[CurrentStep] = args;

            MethodInfo method = ChildType.GetMethod(CurrentStep.AmountInWords());
            method.Invoke(this, new object[] { args });

            if (CurrentStep >= MaxSteps)
            {
                Status = DialogueStatus.Complete;
                SystemController.Instance.Respond(null, $"Dialogue {ChildType.Name} complete!", Owner);
            }
            else if (BackingUp)
            {
                CurrentStep -= 1;
                if (CurrentStep < 0) CurrentStep = 0;

                BackingUp = false;
                return Progress(StoredArgs[CurrentStep + 1]);
            }

            return true;
        }

        /// <summary>
        /// base reply colorization
        /// </summary>
        public string BASE_COLOR;
    }
}
