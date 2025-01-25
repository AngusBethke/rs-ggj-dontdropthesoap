using DontDropTheSoap.Helpers;
using DontDropTheSoap.Quests;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class TheBathroom : Node3D
{
	private Level Level = null;
	private Quest ActiveQuest = null;
	private bool ControlsShown = true;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Create the quests for the level
		var quests = new List<Quest>()
		{
			new("q1_escape", "Escape", "Escape the prison bathroom", 1,
				new List<Objective>()
				{
					new("_1_goto", "Get to the escape point")
				})
			{
				IsAcquired = true,
			},
			new("q2_the_wrench", "The Wrench", "One of the prisoners lost his wrench somewhere, help them find it!", 0.25,
				new List<Objective>()
				{
					new("_1_goto", "Find the wrench"),
					new("_2_goto", "Return the wrench")
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
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		HandleUIMenu();
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
                var index = AdjustIndexAndCheck(indexOfCurrentQuest, indexOfCurrentQuest, "Increment");

                if (index != -1)
                {
                    ActiveQuest = Level.Quests[index];
                    ChangeActiveQuestText();
                }
            }

            if (Input.IsActionJustPressed(UIHelperConstants.ToggleControls))
            {
				ControlsShown = !ControlsShown;
                ChangeControlMenuVisability();
            }
        }
    }

	private void ChangeActiveQuestText()
	{
        var labelItem = GetNode<Control>("Interface")
			.GetNode<NinePatchRect>("QuestContainer")
			.GetNode<Label>("QuestDisplay");

		labelItem.Text = ActiveQuest.Name;
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

        var controlMenuExpandedText = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("ControlsContainer")
            .GetNode<NinePatchRect>("ControlsExpanded")
			.GetNode<RichTextLabel>("ControlsInformation");

		ReplaceKeybindsInText(keybinds, controlMenuExpandedText);

        var controlMenuLabel = GetNode<Control>("Interface")
            .GetNode<NinePatchRect>("ControlsContainer")
            .GetNode<Label>("ControlsLabel");

        ReplaceKeybindsInText(keybinds, controlMenuLabel);
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
            UIHelperConstants.ToggleControls
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
}
