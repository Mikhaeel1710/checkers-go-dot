using Godot;
using System;

public partial class Main : Control
{
	private const int BoardSize =
	CheckersGame.BoardSize;
	private const int SquareSize = 76;

	private readonly CheckerSquare[,] boardButtons =
		new CheckerSquare[BoardSize, BoardSize];

	private readonly CheckersGame game = new();
private readonly PlayerProfile playerOne =
	new("Player 1");

private readonly PlayerProfile playerTwo =
	new("Player 2");

private bool winnerScoreRecorded;
 

	private Label turnLabel = null!;
	private Label playerOneCountLabel = null!;
	private Label playerTwoCountLabel = null!;

	private GridContainer boardGrid = null!;

	private Button restartButton = null!;
	private Button mainMenuButton = null!;

	private ColorPickerButton playerOneColourPicker = null!;
	private ColorPickerButton playerTwoColourPicker = null!;
	private ConfirmationDialog restartDialog = null!;
	private ConfirmationDialog gameOverDialog = null!;
	private ColorRect mainMenu = null!;
private AcceptDialog rulesDialog = null!;
private ConfirmationDialog mainMenuDialog = null!;
private ColorRect playerSetup = null!;

private LineEdit playerOneNameInput = null!;
private LineEdit playerTwoNameInput = null!;

private Label matchScoreLabel = null!;

private OptionButton boardThemeSelector = null!;

private Color lightBoardColour =
	new Color(0.94f, 0.80f, 0.63f);

private Color darkBoardColour =
	new Color(0.47f, 0.25f, 0.10f);

private AudioStreamPlayer moveSound = null!;
private AudioStreamPlayer captureSound = null!;
private AudioStreamPlayer winSound = null!;

private bool soundEnabled = true;
private int boardThemeIndex = 0;

private PauseMenu pauseMenu = null!;
private Label matchStatsLabel = null!;

	public override void _Ready()
{
	SetAnchorsAndOffsetsPreset(
		Control.LayoutPreset.FullRect
	);

	LoadSettings();

	CreateInterface();
	StartGame();
}

	public override void _UnhandledInput(
		InputEvent inputEvent)
	{
		if (inputEvent is not InputEventKey keyEvent)
		{
			return;
		}

		if (!keyEvent.Pressed ||
			keyEvent.Echo ||
			keyEvent.Keycode != Key.Escape)
		{
			return;
		}

		if (mainMenu.Visible ||
			playerSetup.Visible)
		{
			return;
		}

		if (pauseMenu.Visible)
		{
			ClosePauseMenu();
		}
		else
		{
			OpenPauseMenu();
		}

		GetViewport().SetInputAsHandled();
	}

	private string GetPlayerName(
	PieceColour colour)
{
	return colour == PieceColour.Red
		? playerOne.Name
		: playerTwo.Name;
}

	private void CreateInterface()
	{
		ColorRect background = new()
		{
			Color = new Color(
				0.22f,
				0.24f,
				0.29f
			),

			MouseFilter =
				Control.MouseFilterEnum.Ignore
		};

		background.SetAnchorsAndOffsetsPreset(
			Control.LayoutPreset.FullRect
		);

		AddChild(background);

		CenterContainer centreContainer = new();

		centreContainer.SetAnchorsAndOffsetsPreset(
			Control.LayoutPreset.FullRect
		);

		AddChild(centreContainer);

		VBoxContainer content = new()
		{
			SizeFlagsHorizontal =
				Control.SizeFlags.ShrinkCenter,

			SizeFlagsVertical =
				Control.SizeFlags.ShrinkCenter
		};

		content.AddThemeConstantOverride(
			"separation",
			14
		);

		centreContainer.AddChild(content);

		Label titleLabel = new()
		{
			Text = "CHECKERS",

			HorizontalAlignment =
				HorizontalAlignment.Center
		};

		titleLabel.AddThemeFontSizeOverride(
			"font_size",
			40
		);

		titleLabel.AddThemeColorOverride(
			"font_color",
			Colors.White
		);

		content.AddChild(titleLabel);

		CreatePieceCountRow(content);
		CreateScoreRow(content);
		CreateMatchStatsRow(content);

		turnLabel = new Label
		{
			Text = "Player 1's turn",

			HorizontalAlignment =
				HorizontalAlignment.Center
		};

		turnLabel.AddThemeFontSizeOverride(
			"font_size",
			25
		);

		content.AddChild(turnLabel);

		boardGrid = new GridContainer
		{
			Columns = BoardSize,

			SizeFlagsHorizontal =
				Control.SizeFlags.ShrinkCenter
		};

		boardGrid.AddThemeConstantOverride(
			"h_separation",
			0
		);

		boardGrid.AddThemeConstantOverride(
			"v_separation",
			0
		);

		content.AddChild(boardGrid);

		CreateBoardSquares();

		
		restartDialog = new ConfirmationDialog
{
	Title = "Restart Game",
	DialogText =
        "Are you sure you want to restart the current game?"
};

restartDialog.Confirmed += StartGame;

AddChild(restartDialog);
mainMenuDialog = new ConfirmationDialog
{
	Title = "Return to Main Menu",

	DialogText =
		"Return to the main menu?\n\n" +
		"The current match will be restarted when you play again.",

	OkButtonText = "Main Menu",

	CancelButtonText = "Keep Playing"
};

mainMenuDialog.Confirmed += ReturnToMainMenu;

AddChild(mainMenuDialog);
gameOverDialog = new ConfirmationDialog
{
	Title = "Game Over",
	OkButtonText = "Play Again",
	CancelButtonText = "Close"
};

gameOverDialog.Confirmed += StartGame;

AddChild(gameOverDialog);
CreateMainMenu();
CreatePlayerSetup();
CreateSoundPlayers();
CreatePauseMenu();
	}
	private void CreateMainMenu()
{
	mainMenu = new ColorRect
	{
		Color = new Color(
			0.16f,
			0.19f,
			0.27f
		),

		MouseFilter =
			Control.MouseFilterEnum.Stop
	};

	mainMenu.SetAnchorsAndOffsetsPreset(
		Control.LayoutPreset.FullRect
	);

	AddChild(mainMenu);

	CenterContainer menuCentre = new();

	menuCentre.SetAnchorsAndOffsetsPreset(
		Control.LayoutPreset.FullRect
	);

	mainMenu.AddChild(menuCentre);

	VBoxContainer menuContent = new()
	{
		SizeFlagsHorizontal =
			Control.SizeFlags.ShrinkCenter,

		SizeFlagsVertical =
			Control.SizeFlags.ShrinkCenter
	};

	menuContent.AddThemeConstantOverride(
		"separation",
		16
	);

	menuCentre.AddChild(menuContent);

	Label titleLabel = new()
	{
		Text = "CHECKERS",

		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	titleLabel.AddThemeFontSizeOverride(
		"font_size",
		52
	);

	titleLabel.AddThemeColorOverride(
		"font_color",
		new Color(
			1.0f,
			0.78f,
			0.16f
		)
	);

	menuContent.AddChild(titleLabel);

	Label subtitleLabel = new()
	{
		Text = "A local two-player game",

		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	subtitleLabel.AddThemeFontSizeOverride(
		"font_size",
		18
	);

	subtitleLabel.AddThemeColorOverride(
		"font_color",
		new Color(
			0.88f,
			0.90f,
			0.96f
		)
	);

	menuContent.AddChild(subtitleLabel);

	Control spacer = new()
	{
		CustomMinimumSize =
			new Vector2(0, 18)
	};

	menuContent.AddChild(spacer);

	Button playButton =
		CreateMenuButton("Play");

	playButton.Pressed += StartFromMenu;

	menuContent.AddChild(playButton);

	Button rulesButton =
		CreateMenuButton("How to Play");

	rulesButton.Pressed += ShowRules;

	menuContent.AddChild(rulesButton);

	Button quitButton =
		CreateMenuButton("Quit");

	quitButton.Pressed += QuitGame;

	menuContent.AddChild(quitButton);

	Label footerLabel = new()
	{
		Text = "Player 1 moves first",

		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	footerLabel.AddThemeFontSizeOverride(
		"font_size",
		14
	);

	footerLabel.AddThemeColorOverride(
		"font_color",
		new Color(
			0.72f,
			0.76f,
			0.86f
		)
	);

	menuContent.AddChild(footerLabel);

	rulesDialog = new AcceptDialog
	{
		Title = "How to Play",

		DialogText =
			"CHECKERS RULES\n\n" +
			"• Player 1 moves first.\n\n" +
			"• Select one of your pieces, then select a highlighted square.\n\n" +
			"• Regular pieces move diagonally towards the opposite side.\n\n" +
			"• Jump over an opponent's piece to capture it.\n\n" +
			"• Captures are compulsory when available.\n\n" +
			"• Continue jumping when another capture is available.\n\n" +
			"• Reach the opposite end to become a king.\n\n" +
			"• Kings can move and capture forwards or backwards.\n\n" +
			"• Remove all opposing pieces, or block every legal move, to win."
	};

	AddChild(rulesDialog);
}
private void CreatePlayerSetup()
{
	playerSetup = new ColorRect
	{
		Color = new Color(
			0.20f,
			0.23f,
			0.31f
		),

		MouseFilter =
			Control.MouseFilterEnum.Stop
	};

	playerSetup.SetAnchorsAndOffsetsPreset(
		Control.LayoutPreset.FullRect
	);

	AddChild(playerSetup);

	CenterContainer setupCentre = new();

	setupCentre.SetAnchorsAndOffsetsPreset(
		Control.LayoutPreset.FullRect
	);

	playerSetup.AddChild(setupCentre);

	VBoxContainer setupContent = new()
	{
		CustomMinimumSize =
			new Vector2(350, 0),

		SizeFlagsHorizontal =
			Control.SizeFlags.ShrinkCenter,

		SizeFlagsVertical =
			Control.SizeFlags.ShrinkCenter
	};

	setupContent.AddThemeConstantOverride(
		"separation",
		12
	);

	setupCentre.AddChild(setupContent);

	Label titleLabel = new()
	{
		Text = "PLAYER SETUP",

		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	titleLabel.AddThemeFontSizeOverride(
		"font_size",
		38
	);

	titleLabel.AddThemeColorOverride(
		"font_color",
		new Color(
			1.0f,
			0.78f,
			0.16f
		)
	);

	setupContent.AddChild(titleLabel);

	Label instructionLabel = new()
	{
		Text = "Enter names and choose the match style",

		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	instructionLabel.AddThemeFontSizeOverride(
		"font_size",
		17
	);

	setupContent.AddChild(instructionLabel);

	Label playerOneLabel = new()
	{
		Text = "Player 1 Name"
	};

	playerOneLabel.AddThemeFontSizeOverride(
		"font_size",
		16
	);

	setupContent.AddChild(playerOneLabel);

	playerOneNameInput = new LineEdit
	{
		Text = playerOne.Name,

		PlaceholderText =
			"Player 1",

		MaxLength = 20,

		CustomMinimumSize =
			new Vector2(330, 44)
	};

	setupContent.AddChild(
		playerOneNameInput
	);

	Label playerTwoLabel = new()
	{
		Text = "Player 2 Name"
	};

	playerTwoLabel.AddThemeFontSizeOverride(
		"font_size",
		16
	);

	setupContent.AddChild(playerTwoLabel);

	playerTwoNameInput = new LineEdit
	{
		Text = playerTwo.Name,

		PlaceholderText =
			"Player 2",

		MaxLength = 20,

		CustomMinimumSize =
			new Vector2(330, 44)
	};

	setupContent.AddChild(
		playerTwoNameInput
	);

	CreateColourControls(setupContent);

	Label boardThemeLabel = new()
	{
		Text = "Board Theme",

		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	boardThemeLabel.AddThemeFontSizeOverride(
		"font_size",
		16
	);

	setupContent.AddChild(
		boardThemeLabel
	);

	boardThemeSelector = new OptionButton
	{
		CustomMinimumSize =
			new Vector2(330, 44),

		FocusMode =
			Control.FocusModeEnum.None
	};

	boardThemeSelector.AddItem("Classic");
	boardThemeSelector.AddItem("Ocean");
	boardThemeSelector.AddItem("Forest");
	boardThemeSelector.AddItem("Slate");
	boardThemeSelector.AddItem("Royal");

	boardThemeSelector.Selected =
	boardThemeIndex;

	boardThemeSelector.ItemSelected +=
		BoardThemeSelected;

	setupContent.AddChild(
		boardThemeSelector
	);

	Control buttonSpacer = new()
	{
		CustomMinimumSize =
			new Vector2(0, 6)
	};

	setupContent.AddChild(buttonSpacer);

	Button startMatchButton =
		CreateMenuButton("Start Match");

	startMatchButton.Pressed +=
		BeginMatchFromSetup;

	setupContent.AddChild(
		startMatchButton
	);

	Button backButton =
		CreateMenuButton("Back");

	backButton.Pressed +=
		ReturnFromPlayerSetup;

	setupContent.AddChild(backButton);

	playerSetup.Hide();
}
private static Button CreateMenuButton(
	string buttonText)
{
	Button button = new()
	{
		Text = buttonText,

		CustomMinimumSize =
			new Vector2(230, 52),

		SizeFlagsHorizontal =
			Control.SizeFlags.ShrinkCenter,

		FocusMode =
			Control.FocusModeEnum.None
	};

	button.AddThemeFontSizeOverride(
		"font_size",
		19
	);

	return button;
}

	private void CreateColourControls(
		VBoxContainer content)
	{
		HBoxContainer colourRow = new()
		{
			SizeFlagsHorizontal =
				Control.SizeFlags.ShrinkCenter
		};

		colourRow.AddThemeConstantOverride(
			"separation",
			22
		);

		VBoxContainer playerOneGroup = new();

		Label playerOneLabel = new()
		{
			Text = "Player 1 Colour",

			HorizontalAlignment =
				HorizontalAlignment.Center
		};

		playerOneLabel.AddThemeFontSizeOverride(
			"font_size",
			14
		);

		playerOneColourPicker =
			new ColorPickerButton
			{
				Color =
					CheckerSquare.RedPieceColour,

				EditAlpha = false,

				CustomMinimumSize =
					new Vector2(130, 36),

				FocusMode =
					Control.FocusModeEnum.None
			};

		playerOneColourPicker.ColorChanged +=
			PlayerOneColourChanged;

		playerOneGroup.AddChild(playerOneLabel);
		playerOneGroup.AddChild(playerOneColourPicker);

		VBoxContainer playerTwoGroup = new();

		Label playerTwoLabel = new()
		{
			Text = "Player 2 Colour",

			HorizontalAlignment =
				HorizontalAlignment.Center
		};

		playerTwoLabel.AddThemeFontSizeOverride(
			"font_size",
			14
		);

		playerTwoColourPicker =
			new ColorPickerButton
			{
				Color =
					CheckerSquare.BlackPieceColour,

				EditAlpha = false,

				CustomMinimumSize =
					new Vector2(130, 36),

				FocusMode =
					Control.FocusModeEnum.None
			};

		playerTwoColourPicker.ColorChanged +=
			PlayerTwoColourChanged;

		playerTwoGroup.AddChild(playerTwoLabel);
		playerTwoGroup.AddChild(playerTwoColourPicker);

		colourRow.AddChild(playerOneGroup);
		colourRow.AddChild(playerTwoGroup);

		content.AddChild(colourRow);
	}

	private void CreatePieceCountRow(
		VBoxContainer content)
	{
		HBoxContainer countRow = new()
		{
			SizeFlagsHorizontal =
				Control.SizeFlags.ShrinkCenter
		};

		countRow.AddThemeConstantOverride(
			"separation",
			40
		);

		playerOneCountLabel = new Label
		{
			Text = "Player 1: 12"
		};

		playerOneCountLabel.AddThemeFontSizeOverride(
			"font_size",
			20
		);

		playerTwoCountLabel = new Label
		{
			Text = "Player 2: 12"
		};

		playerTwoCountLabel.AddThemeFontSizeOverride(
			"font_size",
			20
		);

		countRow.AddChild(playerOneCountLabel);
		countRow.AddChild(playerTwoCountLabel);

		content.AddChild(countRow);
	}
	private void CreateScoreRow(
	VBoxContainer content)
{
	matchScoreLabel = new Label
	{
		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	matchScoreLabel.AddThemeFontSizeOverride(
		"font_size",
		21
	);

	matchScoreLabel.AddThemeColorOverride(
		"font_color",
		new Color(
			0.78f,
			0.80f,
			0.88f
		)
	);

	content.AddChild(matchScoreLabel);

	UpdateScoreLabel();
}	
private void CreateMatchStatsRow(
	VBoxContainer content)
{
	matchStatsLabel = new Label
	{
		HorizontalAlignment =
			HorizontalAlignment.Center
	};

	matchStatsLabel.AddThemeFontSizeOverride(
		"font_size",
		18
	);

	matchStatsLabel.AddThemeColorOverride(
		"font_color",
		new Color(
			0.86f,
			0.88f,
			0.93f
		)
	);

	content.AddChild(
		matchStatsLabel
	);

	UpdateMatchStats();
}
private void UpdateMatchStats()
{
	matchStatsLabel.Text =
		$"Move {game.MoveCount}    |    " +
		$"Captures: " +
		$"{playerOne.Name} {game.RedCaptures}" +
		$" — " +
		$"{game.BlackCaptures} {playerTwo.Name}";
}

	private void CreateBoardSquares()
	{
		for (int row = 0;
			 row < BoardSize;
			 row++)
		{
			for (int column = 0;
				 column < BoardSize;
				 column++)
			{
				Color squareColour =
					(row + column) % 2 == 0
						? lightBoardColour
						: darkBoardColour;

				CheckerSquare square =
					new CheckerSquare(
						row,
						column
					)
					{
						CustomMinimumSize =
							new Vector2(
								SquareSize,
								SquareSize
							),

						FocusMode =
							Control.FocusModeEnum.None
					};

				ApplySquareStyles(
					square,
					squareColour
				);

				int clickedRow = row;
				int clickedColumn = column;

				square.Pressed += () =>
					SquarePressed(
						clickedRow,
						clickedColumn
					);

				boardButtons[row, column] =
					square;

				boardGrid.AddChild(square);
			}
		}
	}

	private static void ApplySquareStyles(
		CheckerSquare square,
		Color squareColour)
	{
		square.AddThemeStyleboxOverride(
			"normal",
			CreateSquareStyle(
				squareColour
			)
		);

		square.AddThemeStyleboxOverride(
			"hover",
			CreateSquareStyle(
				squareColour.Lightened(
					0.08f
				)
			)
		);

		square.AddThemeStyleboxOverride(
			"pressed",
			CreateSquareStyle(
				squareColour.Darkened(
					0.08f
				)
			)
		);

		square.AddThemeStyleboxOverride(
			"focus",
			new StyleBoxEmpty()
		);
	}

	private static StyleBoxFlat CreateSquareStyle(
		Color colour)
	{
		return new StyleBoxFlat
		{
			BgColor = colour,

			CornerRadiusTopLeft = 0,
			CornerRadiusTopRight = 0,
			CornerRadiusBottomLeft = 0,
			CornerRadiusBottomRight = 0
		};
	}

private void RefreshBoard()
{
	bool captureRequired =
		game.PlayerHasAnyCapture(
			game.CurrentTurn
		);

	for (int row = 0; row < BoardSize; row++)
	{
		for (int column = 0;
			 column < BoardSize;
			 column++)
		{
			bool selected =
				game.SelectedRow == row &&
				game.SelectedColumn == column;

			bool legalDestination = false;
			bool captureDestination = false;

			if (game.SelectedRow != null &&
				game.SelectedColumn != null)
			{
				int sourceRow =
					game.SelectedRow.Value;

				int sourceColumn =
					game.SelectedColumn.Value;

				captureDestination =
					game.IsValidCapture(
						sourceRow,
						sourceColumn,
						row,
						column
					);

				if (!game.MustContinueCapture &&
					!captureRequired)
				{
					legalDestination =
						game.IsValidSimpleMove(
							sourceRow,
							sourceColumn,
							row,
							column
						);
				}
			}

			boardButtons[row, column]
	.UpdateAppearance(
		game.Board[row, column],
		selected,
		legalDestination,
		captureDestination,
		game.LastMoveSourceRow == row &&
			game.LastMoveSourceColumn == column,
		game.LastMoveTargetRow == row &&
			game.LastMoveTargetColumn == column
	);
		}
	}

	UpdatePieceCounts();
UpdateMatchStats();
}

private void SquarePressed(
	int row,
	int column)
{
	bool attemptedCapture = false;

	if (game.SelectedRow != null &&
		game.SelectedColumn != null)
	{
		attemptedCapture =
			game.IsValidCapture(
				game.SelectedRow.Value,
				game.SelectedColumn.Value,
				row,
				column
			);
	}

	BoardClickResult result =
		game.HandleSquarePressed(
			row,
			column
		);

	switch (result)
	{
		case BoardClickResult.IgnoredGameOver:
			return;

		case BoardClickResult.PieceSelected:
			turnLabel.Text =
				$"{GetPlayerName(game.CurrentTurn)} piece selected";

			RefreshBoard();
			return;

		case BoardClickResult.SelectionCleared:
			RefreshBoard();
			UpdateTurnLabel();
			return;

		case BoardClickResult.MoveCompleted:
			RefreshBoard();
			UpdateTurnLabel();

			if (game.GameOver)
			{
				if (soundEnabled)
				{
					winSound.Play();
				}

				ShowGameOverDialog();
			}
			else if (soundEnabled)
			{
				if (attemptedCapture)
				{
					captureSound.Play();
				}
				else
				{
					moveSound.Play();
				}
			}

			return;

		case BoardClickResult.EmptySquare:
			turnLabel.Text =
				"Select one of your pieces";
			return;

		case BoardClickResult.WrongPlayer:
			turnLabel.Text =
				$"It is {GetPlayerName(game.CurrentTurn)}'s turn";
			return;

		case BoardClickResult.MustSelectCapturingPiece:
			turnLabel.Text =
				"Select a piece that can capture";
			return;

		case BoardClickResult.MustContinueCapture:
			turnLabel.Text =
				"The selected piece must capture again";
			return;

		case BoardClickResult.CaptureRequired:
			turnLabel.Text =
				"A capture is available and must be taken";
			return;

		case BoardClickResult.InvalidMove:
			turnLabel.Text =
				"That move is not allowed";
			return;

		case BoardClickResult.OutsideBoard:
			turnLabel.Text =
				"That square is outside the board";
			return;
	}
}
	

	

	

		

	






		

	




private void ShowGameOverDialog()
{
	if (game.Winner == null)
	{
		return;
	}

	RecordWinnerScore();

	string winnerName =
		GetPlayerName(
			game.Winner.Value
		);

	int remainingPieces =
		game.CountPieces(
			game.Winner.Value
		);

	gameOverDialog.DialogText =
		$"{winnerName} wins!\n\n" +
		$"{winnerName} has {remainingPieces} " +
		"pieces remaining.\n\n" +
		$"Match Score\n" +
		$"{playerOne.Name}: {playerOne.Score}\n" +
		$"{playerTwo.Name}: {playerTwo.Score}\n\n" +
		"Would you like to play again?";

	gameOverDialog.PopupCentered();
}







	private void UpdateTurnLabel()
{
	if (game.GameOver &&
		game.Winner != null)
	{
		turnLabel.Text =
			$"{GetPlayerName(game.Winner.Value)} wins!";

		turnLabel.AddThemeColorOverride(
			"font_color",
			new Color(
				1.0f,
				0.82f,
				0.20f
			)
		);

		return;
	}

	string playerName =
		GetPlayerName(
			game.CurrentTurn
		);

	if (game.MustContinueCapture)
	{
		turnLabel.Text =
			$"{playerName} must capture again";
	}
	else
	{
		turnLabel.Text =
			$"{playerName}'s turn";
	}

	Color textColour =
		game.CurrentTurn == PieceColour.Red
			? CheckerSquare.RedPieceColour
			: CheckerSquare.BlackPieceColour;

	turnLabel.AddThemeColorOverride(
		"font_color",
		textColour
	);
}

	private void UpdatePieceCounts()
	{
		int playerOnePieces =
	game.CountPieces(
		PieceColour.Red
	);

int playerTwoPieces =
	game.CountPieces(
		PieceColour.Black
	);

		playerOneCountLabel.Text =
	$"{playerOne.Name}: {playerOnePieces}";

playerTwoCountLabel.Text =
	$"{playerTwo.Name}: {playerTwoPieces}";

		playerOneCountLabel.AddThemeColorOverride(
			"font_color",
			CheckerSquare.RedPieceColour
		);

		playerTwoCountLabel.AddThemeColorOverride(
			"font_color",
			CheckerSquare.BlackPieceColour
		);
	}
	private void UpdateScoreLabel()
{
	matchScoreLabel.Text =
		$"{playerOne.Name}  {playerOne.Score}" +
		$"  —  {playerTwo.Score}  {playerTwo.Name}";
}

private void RecordWinnerScore()
{
	if (winnerScoreRecorded ||
		game.Winner == null)
	{
		return;
	}

	if (game.Winner ==
		PieceColour.Red)
	{
		playerOne.AddWin();
	}
	else
	{
		playerTwo.AddWin();
	}

	winnerScoreRecorded = true;

	UpdateScoreLabel();
}

	private void PlayerOneColourChanged(
	Color colour)
{
	CheckerSquare.RedPieceColour =
		new Color(
			colour.R,
			colour.G,
			colour.B,
			1.0f
		);

	RedrawAllSquares();

	UpdatePieceCounts();
	UpdateTurnLabel();
	SaveSettings();
}

private void PlayerTwoColourChanged(
	Color colour)
{
	CheckerSquare.BlackPieceColour =
		new Color(
			colour.R,
			colour.G,
			colour.B,
			1.0f
		);

	RedrawAllSquares();

	UpdatePieceCounts();
	UpdateTurnLabel();
	SaveSettings();
}

private void RestartGame()
{
	restartDialog.PopupCentered();
}

private void StartGame()
{
	winnerScoreRecorded = false;

game.StartGame();

RefreshBoard();
UpdateTurnLabel();
UpdateScoreLabel();
UpdateMatchStats();
}
private void RequestMainMenu()
{
	mainMenuDialog.PopupCentered();
}

private void ReturnToMainMenu()
{
	pauseMenu.Hide();
	playerSetup.Hide();

	mainMenu.Show();
	mainMenu.MoveToFront();
}
	private void RedrawAllSquares()
{
	for (int row = 0; row < BoardSize; row++)
	{
		for (int column = 0;
			 column < BoardSize;
			 column++)
		{
			boardButtons[row, column].QueueRedraw();
		}
	}
}
private void StartFromMenu()
{
	mainMenu.Hide();

	playerSetup.Show();
	playerSetup.MoveToFront();

	playerOneNameInput.GrabFocus();
}
private void BeginMatchFromSetup()
{
	playerOne.Name =
		GetValidPlayerName(
			playerOneNameInput.Text,
            "Player 1"
		);

	playerTwo.Name =
		GetValidPlayerName(
			playerTwoNameInput.Text,
            "Player 2"
		);

	playerOneNameInput.Text =
		playerOne.Name;

	playerTwoNameInput.Text =
		playerTwo.Name;

	playerSetup.Hide();

	UpdateScoreLabel();
	StartGame();
}

private void ReturnFromPlayerSetup()
{
	playerSetup.Hide();

	mainMenu.Show();
	mainMenu.MoveToFront();
}

private static string GetValidPlayerName(
	string enteredName,
	string fallbackName)
{
	string trimmedName =
		enteredName.Trim();

	return string.IsNullOrWhiteSpace(
		trimmedName
	)
		? fallbackName
		: trimmedName;
}

private void ShowRules()
{
	rulesDialog.PopupCentered();
}

private void BoardThemeSelected(
	long selectedIndex)
{
	boardThemeIndex =
		(int)selectedIndex;

	LoadBoardThemeColours(
		boardThemeIndex
	);

	ApplyBoardTheme();

	SaveSettings();
}

private void LoadBoardThemeColours(
	int selectedIndex)
{
	switch (selectedIndex)
	{
		case 0:
			// Classic
			lightBoardColour =
				new Color(
					0.94f,
					0.80f,
					0.63f
				);

			darkBoardColour =
				new Color(
					0.47f,
					0.25f,
					0.10f
				);
			break;

		case 1:
			// Ocean
			lightBoardColour =
				new Color(
					0.68f,
					0.84f,
					0.94f
				);

			darkBoardColour =
				new Color(
					0.10f,
					0.32f,
					0.52f
				);
			break;

		case 2:
			// Forest
			lightBoardColour =
				new Color(
					0.72f,
					0.84f,
					0.64f
				);

			darkBoardColour =
				new Color(
					0.16f,
					0.38f,
					0.22f
				);
			break;

		case 3:
			// Slate
			lightBoardColour =
				new Color(
					0.72f,
					0.74f,
					0.80f
				);

			darkBoardColour =
				new Color(
					0.20f,
					0.22f,
					0.29f
				);
			break;

		case 4:
			// Royal
			lightBoardColour =
				new Color(
					0.82f,
					0.72f,
					0.94f
				);

			darkBoardColour =
				new Color(
					0.35f,
					0.16f,
					0.52f
				);
			break;

		default:
			boardThemeIndex = 0;

			lightBoardColour =
				new Color(
					0.94f,
					0.80f,
					0.63f
				);

			darkBoardColour =
				new Color(
					0.47f,
					0.25f,
					0.10f
				);
			break;
	}
}

private void ApplyBoardTheme()
{
	for (int row = 0;
		 row < BoardSize;
		 row++)
	{
		for (int column = 0;
			 column < BoardSize;
			 column++)
		{
			Color squareColour =
				(row + column) % 2 == 0
					? lightBoardColour
					: darkBoardColour;

			ApplySquareStyles(
				boardButtons[row, column],
				squareColour
			);

			boardButtons[row, column]
				.QueueRedraw();
		}
	}
}

private void CreateSoundPlayers()
{
	moveSound =
		CreateSoundPlayer(
			"res://Audio/move.wav"
		);

	captureSound =
		CreateSoundPlayer(
			"res://Audio/capture.wav"
		);

	winSound =
		CreateSoundPlayer(
			"res://Audio/win.wav"
		);
}

private AudioStreamPlayer CreateSoundPlayer(
	string soundPath)
{
	AudioStreamPlayer soundPlayer = new()
	{
		Stream =
			GD.Load<AudioStream>(
				soundPath
			),

		VolumeDb = -5.0f
	};

	AddChild(soundPlayer);

	return soundPlayer;
}

private void CreatePauseMenu()
{
	pauseMenu = new PauseMenu();

	pauseMenu.ResumeRequested +=
		ClosePauseMenu;

	pauseMenu.RestartRequested +=
		RestartFromPauseMenu;

	pauseMenu.RulesRequested +=
		ShowRules;

	pauseMenu.SoundToggleRequested +=
		ToggleSound;

	pauseMenu.MainMenuRequested +=
		ReturnToMainMenuFromPause;

	AddChild(pauseMenu);

	pauseMenu.SetSoundEnabled(
		soundEnabled
	);
}

private void OpenPauseMenu()
{
	pauseMenu.Show();
	pauseMenu.MoveToFront();
}

private void ClosePauseMenu()
{
	pauseMenu.Hide();
}

private void RestartFromPauseMenu()
{
	pauseMenu.Hide();
	StartGame();
}

private void ReturnToMainMenuFromPause()
{
	pauseMenu.Hide();

	mainMenu.Show();
	mainMenu.MoveToFront();
}

private void ToggleSound()
{
	soundEnabled =
		!soundEnabled;

	pauseMenu.SetSoundEnabled(
		soundEnabled
	);

	if (!soundEnabled)
	{
		moveSound.Stop();
		captureSound.Stop();
		winSound.Stop();
	}
	SaveSettings();
}

private void QuitGame()
{
	GetTree().Quit();
}
private void LoadSettings()
{
	ConfigFile config = new();

	Error loadResult =
		config.Load(
            "user://checkers_settings.cfg"
		);

	if (loadResult != Error.Ok)
	{
		return;
	}

	soundEnabled =
		(bool)config.GetValue(
			"audio",
			"enabled",
			true
		);

	CheckerSquare.RedPieceColour =
		(Color)config.GetValue(
			"appearance",
			"player_one_colour",
			CheckerSquare.RedPieceColour
		);

	CheckerSquare.BlackPieceColour =
		(Color)config.GetValue(
			"appearance",
			"player_two_colour",
			CheckerSquare.BlackPieceColour
		);

	boardThemeIndex =
		(int)config.GetValue(
			"appearance",
			"board_theme",
			0
		);

	LoadBoardThemeColours(
		boardThemeIndex
	);
}
private void SaveSettings()
{
	ConfigFile config = new();

	config.SetValue(
		"audio",
		"enabled",
		soundEnabled
	);

	config.SetValue(
		"appearance",
		"player_one_colour",
		CheckerSquare.RedPieceColour
	);

	config.SetValue(
		"appearance",
		"player_two_colour",
		CheckerSquare.BlackPieceColour
	);

	config.SetValue(
		"appearance",
		"board_theme",
		boardThemeIndex
	);

	config.Save(
        "user://checkers_settings.cfg"
	);
}
}
