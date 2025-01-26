using DontDropTheSoap.Helpers;
using DontDropTheSoap.Quests;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class TheBathroom : Node3D
{
    private Level Level = null;
    private Quest ActiveQuest = null;
    private bool ControlsShown = true;
    private bool QuestsShown = true;
    private bool DefaultDelayedActionsExecuted = false;
    private Timer Timer => GetNode<Timer>("Timer");
    private bool BlockEnding = false;
    private RigidBody3D Player => GetNode<RigidBody3D>("Player");
    private Node3D QuestLogic => GetNode<Node3D>("QuestLogic");
    private List<NinePatchRect> ContainersMarkedForHiding = new();
    private bool EndScreenIsDisplayed = false;
    private bool IsEndGood = false;
    private bool IsEndBad = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Create the quests for the level
        var quests = new List<Quest>()
        {
            new("q1_escape", "Escape", "Escape the prison bathroom", 1,
                new List<Objective>()
                {
                    new GotoObjective("_1_goto", "Get to the escape point", Player, QuestLogic.GetNode<Area3D>("Q1_Escape_Goto"))
                })
            {
                IsAcquired = true,
            },
            new("q2_the_wrench", "The Wrench", "One of the prisoners lost his wrench somewhere, help them find it!", 0.25,
                new List<Objective>()
                {
                    new GotoObjective("_1_goto", "Find the wrench", Player, QuestLogic.GetNode<Area3D>("Q2_Wrench_O1_Pickup")),
                    new GotoObjective("_2_goto", "Return the wrench", Player, QuestLogic.GetNode<Area3D>("Q2_Wrench_O2_DropOff"))
                })
            {
                IsAcquired = true,
            },
            new("q3_the_test", "The Test", "One of the prisoners is struggling to pass test, help him out!", 0.25,
                new List<Objective>()
                {
                    new("_1_goto", "Get a better sample from the toilet"),
                    new("_2_goto", "Return to the prisoner")
                })
            {
                IsAcquired = false,
            }
        };

        Level = new Level("The Prison Bathroom", "Soapy's in for a slippery surprise in the Prison bathrooms...", quests);
        ActiveQuest = Level.Quests.First();

        // Update UI
        ChangeActiveQuestText();
        ChangeControlMenuVisability();
        ReplaceTextsWithKeybinds();
        HideQuestNotifiationContainer();
        DefaultQuestContainerVisibility();
        HandleQuestInformationDisplay();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (EndScreenIsDisplayed)
        {
            if (EndScreenButtonPressed())
            {
                // TODO: Transition back to main menu
                GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
            }
        }
        else
        {
            HandleTimer();
            HandleUIMenu();
            HandleQuestLogic();
            HandleMarkedContainers();
        }
    }

    private void HandleUIMenu()
    {
        if (Level != null)
        {
            var indexOfCurrentQuest = Level.Quests.IndexOf(ActiveQuest);

            if (Input.IsActionJustPressed(UIHelperConstants.QuestPrevious))
            {
                var index = AdjustIndexAndCheck(indexOfCurrentQuest, indexOfCurrentQuest, "Decrement");

                if (index != -1)
                {
                    ActiveQuest = Level.Quests[index];
                    ChangeActiveQuestText();
                }
            }

            if (Input.IsActionJustPressed(UIHelperConstants.QuestNext))
            {
                IncrementActiveQuest();
            }

            if (Input.IsActionJustPressed(UIHelperConstants.ToggleControls))
            {
                ControlsShown = !ControlsShown;
                ChangeControlMenuVisability();
            }

            if (Input.IsActionJustPressed(UIHelperConstants.ToggleQuests))
            {
                QuestsShown = !QuestsShown;
                ChangeQuestMenuVisability();
                HandleQuestInformationDisplay();
            }

            if (QuestsShown)
            {
                if (Input.IsActionJustPressed(UIHelperConstants.MenuTabLeft))
                {
                    HandleQuestInformationTabbing("left");
                }

                if (Input.IsActionJustPressed(UIHelperConstants.MenuTabRight))
                {
                    HandleQuestInformationTabbing("right");
                }
            }

            if ((Timer.TimeLeft <= (Timer.WaitTime - 5)) && !DefaultDelayedActionsExecuted)
            {
                DefaultDelayedActionsExecuted = true;

                if (ControlsShown)
                {
                    ControlsShown = !ControlsShown;
                    ChangeControlMenuVisability();
                }

                if (QuestsShown)
                {
                    QuestsShown = !QuestsShown;
                    ChangeQuestMenuVisability();
                    HandleQuestInformationDisplay();
                }
            }
        }
    }

    private void IncrementActiveQuest()
    {
        var indexOfCurrentQuest = Level.Quests.IndexOf(ActiveQuest);
        var index = AdjustIndexAndCheck(indexOfCurrentQuest, indexOfCurrentQuest, "Increment");

        if (index != -1)
        {
            ActiveQuest = Level.Quests[index];
            ChangeActiveQuestText();
        }
    }

	private void ChangeActiveQuestText()
	{
        var labelItem = GetNode<Control>("Interface")
			.GetNode<NinePatchRect>("QuestContainer")
			.GetNode<Label>("QuestDisplay");

		labelItem.Text = ActiveQuest.Name;
        HandleQuestInformationDisplay();
    }

	private void ChangeControlMenuVisability()
	{
		var controlMenuExpanded = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("ControlsContainer")
            .GetNode<NinePatchRect>("ControlsExpanded");

		var controlMenuCollapsed = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("ControlsContainer")
            .GetNode<NinePatchRect>("ControlsCollapsed");

		if (ControlsShown)
		{
			controlMenuCollapsed.Visible = false;
			controlMenuExpanded.Visible = true;
		}
		else
		{
            controlMenuCollapsed.Visible = true;
            controlMenuExpanded.Visible = false;
        }
    }

    private void ChangeQuestMenuVisability()
    {
        var questMenuExpanded = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestInformationGroup")
            .GetNode<NinePatchRect>("QuestInformationContainer");

        var questMenuCollapsed = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestInformationGroup")
            .GetNode<NinePatchRect>("QuestInformationCollapsed");

        if (QuestsShown)
        {
            questMenuCollapsed.Visible = false;
            questMenuExpanded.Visible = true;
        }
        else
        {
            questMenuCollapsed.Visible = true;
            questMenuExpanded.Visible = false;
        }
    }

    private int AdjustIndexAndCheck(int indexOfCurrentQuest, int startingIndex, string direction)
	{
		if (direction == "Increment")
			indexOfCurrentQuest++;
		else
            indexOfCurrentQuest--;

        if (indexOfCurrentQuest < 0)
        {
            indexOfCurrentQuest = Level.Quests.Count - 1;
        }

        if (indexOfCurrentQuest >= Level.Quests.Count)
        {
            indexOfCurrentQuest = 0;
        }

        // If we're back at our starting index then there's no quest to change to
        if (indexOfCurrentQuest == startingIndex)
        {
			return -1;
        }

		var questToCheck = Level.Quests[indexOfCurrentQuest];

		if (questToCheck.IsCompleted || !questToCheck.IsAcquired)
		{
			return AdjustIndexAndCheck(indexOfCurrentQuest, startingIndex, direction);
		}

		return indexOfCurrentQuest;
    }

	private void ReplaceTextsWithKeybinds()
	{
		var keybinds = GetAllMappedKeybinds();

        var controlMenuExpanded = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("ControlsContainer")
            .GetNode<NinePatchRect>("ControlsExpanded");

        var controlMenuExpandedText = controlMenuExpanded
            .GetNode<RichTextLabel>("ControlsInformation");

		ReplaceKeybindsInText(keybinds, controlMenuExpandedText);

        var controlMenuExpandedLabel = controlMenuExpanded
            .GetNode<Label>("ControlsLabelToggle");

        ReplaceKeybindsInText(keybinds, controlMenuExpandedLabel);

        var controlMenuCollapsedLabel = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("ControlsContainer")
            .GetNode<NinePatchRect>("ControlsCollapsed")
            .GetNode<Label>("ControlsLabelToggle");

        ReplaceKeybindsInText(keybinds, controlMenuCollapsedLabel);

        var questMenuExpanded = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestInformationGroup")
            .GetNode<NinePatchRect>("QuestInformationContainer");
        var questMenuExpandedToggle = questMenuExpanded
            .GetNode<Label>("QuestMenuToggle");

        ReplaceKeybindsInText(keybinds, questMenuExpandedToggle);

        var questMenuExpandedTabCycle = questMenuExpanded
            .GetNode<Label>("QuestMenuTabCycle");

        ReplaceKeybindsInText(keybinds, questMenuExpandedTabCycle);

        var questMenuCollapsedToggle = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestInformationGroup")
            .GetNode<NinePatchRect>("QuestInformationCollapsed")
            .GetNode<Label>("QuestMenuToggle");

        ReplaceKeybindsInText(keybinds, questMenuCollapsedToggle);
    }

	private void ReplaceKeybindsInText(Dictionary<string, string> keybinds, RichTextLabel textLabel)
	{
		var text = textLabel.Text;

		foreach (var keybind in keybinds)
		{
            text = text.Replace($"<{keybind.Key}>", keybind.Value);
		}

		textLabel.Text = text;
    }

    private void ReplaceKeybindsInText(Dictionary<string, string> keybinds, Label textLabel)
    {
        var text = textLabel.Text;

        foreach (var keybind in keybinds)
        {
            text = text.Replace($"<{keybind.Key}>", keybind.Value);
        }

        textLabel.Text = text;
    }

    private Dictionary<string, string> GetAllMappedKeybinds()
	{
        var dictionary = new Dictionary<string, string>();
		var uiMappings = new List<string>()
		{
            UIHelperConstants.PlayerForward,
            UIHelperConstants.PlayerBack,
            UIHelperConstants.PlayerLeft,
            UIHelperConstants.PlayerRight,
            UIHelperConstants.QuestNext,
            UIHelperConstants.QuestPrevious,
            UIHelperConstants.ToggleControls,
            UIHelperConstants.ToggleQuests,
            UIHelperConstants.MenuTabLeft,
            UIHelperConstants.MenuTabRight,
        };

		foreach (var mapping in uiMappings)
		{
            var keys = GetKeyAction(mapping);
            dictionary.Add(keys.Key, keys.Value);
        }

        return dictionary;
    }

	private KeyValuePair<string, string> GetKeyAction(string keyIdentifier)
	{
        var action = (InputEventKey)InputMap.ActionGetEvents(keyIdentifier).FirstOrDefault();
        var keyThatWillBePressed = action.PhysicalKeycode.ToString();
        return new KeyValuePair<string, string>(keyIdentifier, keyThatWillBePressed);
	}

    private void HandleTimer()
    {
        var timeLeft = Timer.TimeLeft;

        if (timeLeft <= 0 && !BlockEnding)
        {
            IsEndBad = true;
            DisplayEndScreen();
        }

        var minutes = (int)Math.Floor(timeLeft / 60);
        var seconds = (int)Math.Floor(timeLeft - (minutes * 60));
        var timeLeftString = $"{minutes}:{(seconds.ToString().PadLeft(2, '0'))}";

        var timerLabel = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("TimerContainer")
            .GetNode<Label>("TimerLabel");

        timerLabel.Text = timeLeftString;
    }

    private void HandleQuestLogic()
    {
        if (Level.Quests.First().IsCompleted)
        {
            if (!EndScreenIsDisplayed)
            {
                EndScreenIsDisplayed = true;
                IsEndGood = true;
                DisplayEndScreen();
            }
        }
        else
        {
            var currentObjective = ActiveQuest.Objectives.FirstOrDefault(x => !x.IsCompleted && !x.IsOptional);

            if (currentObjective == null)
            {
                if (ActiveQuest.IsCompletedCondition.Invoke())
                {
                    ActiveQuest.IsCompleted = true;
                    HandleCompletedNotification("Quest Completed");
                    IncrementActiveQuest();
                }
            }
            else
            {
                if (currentObjective.IsCompletedCondition.Invoke())
                {
                    currentObjective.IsCompleted = true;
                    HandleCompletedNotification("Objective Completed");
                }
            }
        }
    }

    private void HideQuestNotifiationContainer()
    {
        var questNotificationContainer = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestNotificationContainer");
        questNotificationContainer.Visible = false;
    }

    private void HandleCompletedNotification(string notificationText)
    {
        var questNotificationContainer = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestNotificationContainer");

        var questNotificationLabel = questNotificationContainer
            .GetNode<Label>("QuestNotificationLabel");

        questNotificationLabel.Text = "Objective Completed";
        questNotificationContainer.Visible = true;

        DelayedHide(questNotificationContainer);
        HandleQuestInformationDisplay();
    }

    private void DelayedHide(NinePatchRect node)
    {
        Task.Delay(1000).ContinueWith(t =>
        {
            ContainersMarkedForHiding.Add(node);
        });
    }

    private void HandleMarkedContainers()
    {
        foreach(var container in ContainersMarkedForHiding)
        {
            container.Visible = false;
        }

        ContainersMarkedForHiding.Clear();
    }

    private void HandleQuestInformationDisplay()
    {
        var questInfoContainer = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestInformationGroup")
            .GetNode<NinePatchRect>("QuestInformationContainer");

        var questListText = string.Empty;
        foreach (var quest in Level.Quests)
        {
            var questStatus = quest.IsCompleted ? "Done" : "In Progress...";
            questListText += $"> {quest.Name}: [b]{questStatus}[/b]";
            questListText += @"
";
            questListText += quest.Description;
            questListText += @"

";
            // Adds all quests to list
        }

        questInfoContainer.GetNode<RichTextLabel>("QuestList").Text = questListText;

        var objectiveListText = string.Empty;
        foreach (var objective in ActiveQuest.Objectives)
        {
            var objectiveStatus = objective.IsCompleted ? "Done" : "In Progress...";
            objectiveListText += $"> {objective.Description}: [b]{objectiveStatus}[/b]";
            objectiveListText += @"

";
            // Adds all objectives to list
        }

        questInfoContainer.GetNode<RichTextLabel>("ObjectiveList").Text = objectiveListText;
    }

    private void HandleQuestInformationTabbing(string direction)
    {
        // TODO : Adjust implementation to support more than two tabs
        var questInfoContainer = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestInformationGroup")
            .GetNode<NinePatchRect>("QuestInformationContainer");

        var questList = questInfoContainer.GetNode<RichTextLabel>("QuestList");
        var objectiveList = questInfoContainer.GetNode<RichTextLabel>("ObjectiveList");

        if (questList.Visible)
        {
            questList.Visible = false;
            objectiveList.Visible = true;
        }
        else if (objectiveList.Visible)
        {
            objectiveList.Visible = false;
            questList.Visible = true;
        }
    }

    private void DefaultQuestContainerVisibility()
    {
        var questInfoGroup = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("QuestInformationGroup");

        var questInfoContainer = questInfoGroup
            .GetNode<NinePatchRect>("QuestInformationContainer");

        questInfoContainer.GetNode<RichTextLabel>("ObjectiveList").Visible = false;

        questInfoGroup.GetNode<NinePatchRect>("QuestInformationCollapsed").Visible = false;
    }

    private void DisplayEndScreen()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        if (IsEndGood)
        {
            GetNode<Control>("Interface").GetNode<NinePatchRect>("EndScreenGood").Visible = true;
        }
        else
        {
            GetNode<Control>("Interface").GetNode<NinePatchRect>("EndScreenBad").Visible = true;
        }
    }

    private bool EndScreenButtonPressed()
    {
        var endScreenButtonGood = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("EndScreenGood")
            .GetNode<Button>("Button");

        var endScreenButtonBad = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("EndScreenBad")
            .GetNode<Button>("Button");

        if (endScreenButtonGood.ButtonPressed || endScreenButtonBad.ButtonPressed)
        {
            return true;
        }

        return false;
    }
}
