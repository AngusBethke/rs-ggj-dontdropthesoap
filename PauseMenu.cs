using Godot;
using System;

public partial class PauseMenu : Control
{
	public bool ResumePressed = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnButtonResumePressed()
	{
        Input.MouseMode = Input.MouseModeEnum.Captured;
        Visible = false;
    }

    public void OnButtonMainMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }
}
