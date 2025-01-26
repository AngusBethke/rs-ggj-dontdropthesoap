using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DontDropTheSoap.Quests
{
    public class Level: LevelNodeBase
    {
        public Level(string name, string description, List<Quest> quests) : base(name, description) 
        {
            Quests = quests;
        }

        public List<Quest> Quests { get; set; }
    }

    public class Quest : LevelNodeBase
    {
        public Quest(string id, string name, string description, double multiplier, List<Objective> objectives, Quest subQuest = null) : base(name, description)
        {
            Id = id;
            Multiplier = multiplier;
            Objectives = objectives;
            SubQuest = subQuest;
            IsCompletedCondition = () => HandleIsCompleted();
        }

        public string Id { get; set; }
        public double Multiplier { get; set; }
        public double ModifiedMultiplier { get; set; }
        public bool IsAcquired { get; set; }
        public List<Objective> Objectives { get; set; }
        public Quest SubQuest { get; set; }

        public bool HandleIsCompleted()
        {
            var additionalMultiplier = Objectives.Where(x => x.IsCompleted).Select(x => x.Multiplier).Sum();
            var allObjectivesCompleted = Objectives.Where(x => !x.IsOptional).All(x => x.IsCompleted);
            var subQuestCompleted = true;

            if (SubQuest != null)
            {
                subQuestCompleted = SubQuest.IsCompleted;
            }

            var isCompleted = allObjectivesCompleted && subQuestCompleted;

            if (isCompleted)
            {
                ModifiedMultiplier = Multiplier + additionalMultiplier;
            }

            return isCompleted;
        }
    }

    public class GotoObjective: Objective
    {
        public GotoObjective(string id, string description, RigidBody3D player, Area3D area, double multiplier = 0, bool isOptional = false) : base(id, description, multiplier, isOptional)
        {
            Player = player;
            Area = area;
            IsCompletedCondition = () => HandleIsCompleted();
        }

        public RigidBody3D Player { get; set; }
        public Area3D Area { get; set; }

        public bool HandleIsCompleted()
        {
            var conditionMet = false;

            if (Area != null && Area.Visible)
            {
                var entitiesInArea = Area.GetOverlappingBodies();
                var playerInArea = entitiesInArea.Any(x => x == Player);

                if (playerInArea)
                {
                    Area.Visible = false;
                    conditionMet = true;
                }
            }

            return conditionMet;
        }
    }

    public class Objective: QuestNodeBase
    {
        public Objective(string id, string description, double multiplier = 0, bool isOptional = false): base(description)
        {
            Id = id;
            Multiplier = multiplier;
            IsOptional = isOptional;
        }

        public string Id { get; set; }
        public double Multiplier { get; set; }
        public bool IsOptional { get; set; }
    }

    public class LevelNodeBase: QuestNodeBase
    {
        public LevelNodeBase(string name, string description): base(description)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class QuestNodeBase
    {
        public QuestNodeBase(string description)
        {
            Description = description;
        }

        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public Func<bool> IsCompletedCondition { get; set; } = () => false;
    }
}
