using Godot;

public partial class CheckerSquare : Button
{
	public static Color RedPieceColour { get; set; } =
		new Color(0.92f, 0.12f, 0.15f);

public static Color BlackPieceColour { get; set; } =
	new Color(0.55f, 0.25f, 0.90f);

	public int Row { get; }
	public int Column { get; }

	private CheckerPiece? piece;

	private bool isSelected;
	private bool isLegalDestination;
	private bool isCaptureDestination;
	private bool isLastMoveSource;
private bool isLastMoveTarget;

	private Texture2D? crownTexture;

	public CheckerSquare(
		int row,
		int column)
	{
		Row = row;
		Column = column;
	}

	public override void _Ready()
	{
		crownTexture =
			GD.Load<Texture2D>(
                "res://Assets/crown.svg"
			);

		QueueRedraw();
	}

	public void UpdateAppearance(
	CheckerPiece? newPiece,
	bool selected,
	bool legalDestination,
	bool captureDestination,
	bool lastMoveSource,
	bool lastMoveTarget)
{
	piece = newPiece;

	isSelected = selected;
	isLegalDestination = legalDestination;
	isCaptureDestination = captureDestination;

	isLastMoveSource = lastMoveSource;
	isLastMoveTarget = lastMoveTarget;

	QueueRedraw();
}

	public override void _Draw()
	{
		Vector2 centre = Size / 2.0f;
		// Previous square in the most recent move.
if (isLastMoveSource)
{
	DrawRect(
		new Rect2(
			Vector2.Zero,
			Size
		),
		new Color(
			0.20f,
			0.55f,
			1.0f,
			0.30f
		),
		true
	);

	DrawRect(
		new Rect2(
			Vector2.One * 3.0f,
			Size - Vector2.One * 6.0f
		),
		new Color(
			0.30f,
			0.65f,
			1.0f,
			0.95f
		),
		false,
		3.0f,
		true
	);
}

// Destination square in the most recent move.
if (isLastMoveTarget)
{
	DrawRect(
		new Rect2(
			Vector2.Zero,
			Size
		),
		new Color(
			0.20f,
			1.0f,
			0.55f,
			0.30f
		),
		true
	);

	DrawRect(
		new Rect2(
			Vector2.One * 3.0f,
			Size - Vector2.One * 6.0f
		),
		new Color(
			0.25f,
			1.0f,
			0.55f,
			0.95f
		),
		false,
		3.0f,
		true
	);
}

		// Border around the selected square.
		if (isSelected)
		{
			DrawRect(
				new Rect2(
					Vector2.Zero,
					Size
				),
				new Color(
					1.0f,
					0.78f,
					0.15f
				),
				false,
				5.0f,
				true
			);
		}

		// Small green dot for a normal legal move.
		if (isLegalDestination)
		{
			DrawCircle(
				centre,
				9.0f,
				new Color(
					0.20f,
					0.95f,
					0.42f,
					0.90f
				),
				true,
				-1.0f,
				true
			);
		}

		// Gold ring for a legal capture.
		if (isCaptureDestination)
		{
			DrawCircle(
				centre,
				18.0f,
				new Color(
					1.0f,
					0.70f,
					0.10f,
					0.95f
				),
				false,
				5.0f,
				true
			);
		}

		if (piece == null)
		{
			return;
		}

		float radius =
			Mathf.Min(
				Size.X,
				Size.Y
			) * 0.30f;

		Color pieceColour =
			piece.Colour == PieceColour.Red
				? RedPieceColour
				: BlackPieceColour;

		// Shadow beneath the piece.
		DrawCircle(
			centre + new Vector2(2, 3),
			radius,
			new Color(
				0.0f,
				0.0f,
				0.0f,
				0.38f
			),
			true,
			-1.0f,
			true
		);

		// Main checker piece.
		DrawCircle(
			centre,
			radius,
			pieceColour,
			true,
			-1.0f,
			true
		);

		// Outer rim.
		DrawCircle(
			centre,
			radius,
			pieceColour.Lightened(0.22f),
			false,
			3.0f,
			true
		);

		// Inner decorative ring.
		DrawCircle(
			centre,
			radius * 0.72f,
			pieceColour.Darkened(0.16f),
			false,
			2.0f,
			true
		);

		// Crown for promoted pieces.
		if (piece.IsKing &&
			crownTexture != null)
		{
			Vector2 crownSize =
				new Vector2(
					radius * 1.25f,
					radius * 0.95f
				);

			Rect2 crownArea =
				new Rect2(
					centre - crownSize / 2.0f,
					crownSize
				);

			DrawTextureRect(
				crownTexture,
				crownArea,
				false,
				new Color(
					1.0f,
					0.80f,
					0.12f
				)
			);
		}
	}
}
