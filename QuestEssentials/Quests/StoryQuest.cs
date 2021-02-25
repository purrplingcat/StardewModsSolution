﻿using Newtonsoft.Json;
using QuestFramework.Quests;
using QuestFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using System.Text;
using QuestEssentials.Messages;
using QuestEssentials.Tasks;

namespace QuestEssentials.Quests
{
    public class StoryQuest : CustomQuest<StoryQuest.StoryQuestState>
    {
        public class StoryQuestState
        {
            public bool complete = false;
            public Dictionary<string, int> progress;

            public StoryQuestState()
            {
                this.progress = new Dictionary<string, int>();
            }
        }

        public List<StoryQuestTask> Tasks { get; set; }

        protected override void OnInitialize()
        {
            this.Accepted += this.OnAccepted;
            QuestEssentialsMod.ModHelper.Events.GameLoop.UpdateTicked += this.OnGameUpdateTicked;
            base.OnInitialize();
        }

        private void OnAccepted(object sender, IQuestInfo e)
        {
            this.Tasks.ForEach(t => t.Register(this));
        }

        protected override void Dispose(bool disposing)
        {
            this.Accepted -= this.OnAccepted;
            QuestEssentialsMod.ModHelper.Events.GameLoop.UpdateTicked -= this.OnGameUpdateTicked;
            base.Dispose(disposing);
        }

        private void OnGameUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.CanPlayerMove)
                return;

            if (this.State.complete && this.IsInQuestLogAndActive())
            {
                this.Complete();
            }
        }

        public override void OnRegister()
        {
            this.Tasks.ForEach(t => t.Register(this));
            base.OnRegister();
        }

        public override bool OnCompletionCheck(object completionMessage)
        {
            if (completionMessage is StoryMessage storyMessage)
            {
                return this.CheckTasks(storyMessage);
            } 
            else if (completionMessage is ICompletionArgs completionArgs)
            {
                return this.CheckTasks(new VanillaCompletionMessage(completionArgs));
            }

            return false;
        }

        public override void OnAdjust(object toAdjust)
        {
            if (this.Tasks == null)
                return;

            foreach (var task in this.Tasks)
            {
                if (!task.IsRegistered() && !task.IsActive())
                    continue;

                task.DoAdjust(toAdjust);
            }

        }

        protected override void UpdateCurrentObjectives(List<CustomQuestObjective> currentObjectives)
        {
            currentObjectives.Clear();

            if (this.Tasks != null) {
                foreach (var task in this.Tasks)
                {
                    if (!task.IsActive())
                        continue;

                    StringBuilder text = new StringBuilder(task.Description);

                    if (task.ShouldShowProgress() && task.Goal > 1)
                    {
                        text.Append($" ({task.CurrentCount}/{task.Goal})");
                    }

                    currentObjectives.Add(new CustomQuestObjective(task.Name, text.ToString())
                    {
                        IsCompleted = task.IsCompleted()
                    });
                }
            }
        }

        public bool HasCompletedTask(string requiredTaskName)
        {
            return this.Tasks.Any(t => t.Name == requiredTaskName && t.IsCompleted());
        }

        public bool IsInQuestLogAndActive()
        {
            return this.IsInQuestLog() && !this.GetInQuestLog().completed.Value;
        }

        public void CheckQuestCompletion()
        {
            this.State.complete = this.HasAllTasksCompleted();
        }

        public bool HasAllTasksCompleted()
        {
            return !this.Tasks.Any(t => t.IsRegistered() && !t.IsCompleted());
        }

        private bool CheckTasks(StoryMessage storyMessage)
        {
            if (this.Tasks == null || !this.IsInQuestLogAndActive())
                return false;

            bool worked = false;
            foreach (var task in this.Tasks)
            {
                if (!task.IsRegistered() || !task.IsActive())
                    continue;

                worked |= task.OnCheckProgress(storyMessage);
            }

            return worked;
        }
    }
}
