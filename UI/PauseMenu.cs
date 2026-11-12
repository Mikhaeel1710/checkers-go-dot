using Godot;
using System;

public partial class PauseMenu : ColorRect
{
	public event Action ResumeRequested =
		delegate { };

	public event Action RestartRequested =
		delegate { };

	public event Action RulesRequested =
		delegate { };

	public event Action SoundToggleRequested =
		delegate { };

	public event Action MainMenuRequested =
		delegate { };

	private Button soundButton = null!;

	public override void _Ready()
	{
		Color = new Color(
			0.10f,
			0.13f,
			0.20f,
			0.96f
		);

		MouseFilter =
			Control.MouseFilterEnum.Stop;

		SetAnchorsAndOffsetsPreset(
			Control.LayoutPreset.FullRect
		);

		CreateInterface();
		Hide();
	}

	private void CreateInterface()
	{
		CenterContainer centre = new();

		centre.SetAnchorsAndOffsetsPreset(
			Control.LayoutPreset.FullRect
		);

		AddChild(centre);

		PanelContainer panel = new()
		{
			CustomMinimumSize =
				new Vector2(330, 0)
		};

		StyleBoxFlat panelStyle = new()
		{
			BgColor = new Color(
				0.30f,
				0.34f,
				0.44f
			),

			CornerRadiusTopLeft = 14,
			CornerRadiusTopRight = 14,
			CornerRadiusBottomLeft = 14,
			CornerRadiusBottomRight = 14,

			ContentMarginLeft = 32,
			ContentMarginRight = 32,
			ContentMarginTop = 28,
			ContentMarginBottom = 28
		};

		panel.AddThemeStyleboxOverride(
			"panel",
			panelStyle
		);

		centre.AddChild(panel);

		VBoxContainer content = new();

		content.AddThemeConstantOverride(
			"separation",
			14
		);

		panel.AddChild(content);

		Label title = new()
		{
			Text = "PAUSED",

			HorizontalAlignment =
				HorizontalAlignment.Center
		};

		title.AddThemeFontSizeOverride(
			"font_size",
			36
		);

		title.AddThemeColorOverride(
			"font_color",
			new Color(
				1.0f,
				0.80f,
				0.20f
			)
		);

		content.AddChild(title);

		Label instruction = new()
		{
			Text = "Press Esc to resume",

			HorizontalAlignment =
				HorizontalAlignment.Center
		};

		instruction.AddThemeFontSizeOverride(
			"font_size",
			15
		);

		content.AddChild(instruction);

		Control spacer = new()
		{
			CustomMinimumSize =
				new Vector2(0, 10)
		};

		content.AddChild(spacer);

		Button resumeButton =
			CreatePauseButton("Resume");

		resumeButton.Pressed += () =>
			ResumeRequested();

		content.AddChild(resumeButton);

		Button restartButton =
			CreatePauseButton(
				"Restart Match"
			);

		restartButton.Pressed += () =>
			RestartRequested();

		content.AddChild(restartButton);

		Button rulesButton =
			CreatePauseButton(
				"How to Play"
			);

		rulesButton.Pressed += () =>
			RulesRequested();

		content.AddChild(rulesButton);

		soundButton =
			CreatePauseButton(
				"Sound: On"
			);

		soundButton.Pressed += () =>
			SoundToggleRequested();

		content.AddChild(soundButton);

		Button mainMenuButton =
			CreatePauseButton(
				"Main Menu"
			);

		mainMenuButton.Pressed += () =>
			MainMenuRequested();

		content.AddChild(mainMenuButton);
	}

	public void SetSoundEnabled(
		bool enabled)
	{
		soundButton.Text =
			enabled
				? "Sound: On"
				: "Sound: Off";
	}

	private static Button CreatePauseButton(
		string text)
	{
		Button button = new()
		{
			Text = text,

			CustomMinimumSize =
				new Vector2(260, 48),

			FocusMode =
				Control.FocusModeEnum.None
		};

		button.AddThemeFontSizeOverride(
			"font_size",
			18
		);

		return button;
	}
}
