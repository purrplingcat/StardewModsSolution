﻿using Newtonsoft.Json;
using QuestEssentials.Messages;
using System.Collections.Generic;

namespace QuestEssentials.Tasks
{
    public class SlayTask : QuestTask<SlayTask.SlayData>
    {
        public struct SlayData
        {
            public string TargetName { get; set; }
        }

        protected List<string> _targetNames = new List<string>();

        public override void Load()
        {
            base.Load();

            if (this.Data.TargetName != null)
            {
                this._targetNames.Clear();

                foreach (string t in this.Data.TargetName.Split(','))
                    this._targetNames.Add(t.Trim());
            }
        }

        public override bool OnCheckProgress(StoryMessage message)
        {
            if (this.IsCompleted())
                return false;

            if (message is VanillaCompletionMessage args && args.CompletionType == 4 && this.IsWhenMatched())
            {
                if (this._targetNames.Count == 0 && !string.IsNullOrEmpty(args.String))
                {
                    this.IncrementCount(1);

                    return true;
                }
                    
                foreach (string targetName in this._targetNames)
                {
                    if (args.String != null && args.String.Contains(targetName))
                    {
                        this.IncrementCount(1);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
