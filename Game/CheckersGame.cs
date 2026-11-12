using System;
public enum PieceSelectionResult
{
	Selected,
	EmptySquare,
	WrongPlayer,
	MustSelectCapturingPiece,
	OutsideBoard
}
public enum BoardClickResult
{
	IgnoredGameOver,
	PieceSelected,
	SelectionCleared,
	MoveCompleted,
	EmptySquare,
	WrongPlayer,
	MustSelectCapturingPiece,
	MustContinueCapture,
	CaptureRequired,
	InvalidMove,
	OutsideBoard
}
public class CheckersGame
{
	public const int BoardSize = 8;

	public CheckerPiece?[,] Board { get; } =
		new CheckerPiece?[BoardSize, BoardSize];

public PieceColour CurrentTurn { get; private set; } =
	PieceColour.Red;

public bool MustContinueCapture { get; private set; }

public bool GameOver { get; private set; }

public PieceColour? Winner { get; private set; }

public int? SelectedRow { get; private set; }

public int? SelectedColumn { get; private set; }
public int? LastMoveSourceRow { get; private set; }

public int? LastMoveSourceColumn { get; private set; }

public int? LastMoveTargetRow { get; private set; }

public int? LastMoveTargetColumn { get; private set; }
public int MoveCount { get; private set; }

public int RedCaptures { get; private set; }

public int BlackCaptures { get; private set; }

	public void StartGame()
	{
		Array.Clear(
			Board,
			0,
			Board.Length
		);

		SelectedRow = null;
		SelectedColumn = null;

		CurrentTurn = PieceColour.Red;

		MustContinueCapture = false;
		GameOver = false;
		Winner = null;

LastMoveSourceRow = null;
LastMoveSourceColumn = null;
LastMoveTargetRow = null;
LastMoveTargetColumn = null;

MoveCount = 0;

RedCaptures = 0;
BlackCaptures = 0;

PlaceStartingPieces();

		
	}

	private void PlaceStartingPieces()
	{
		// Player 2 begins at the top.
		for (int row = 0; row < 3; row++)
		{
			for (int column = 0;
				 column < BoardSize;
				 column++)
			{
				if ((row + column) % 2 != 0)
				{
					Board[row, column] =
						new CheckerPiece(
							PieceColour.Black
						);
				}
			}
		}

		// Player 1 begins at the bottom.
		for (int row = 5;
			 row < BoardSize;
			 row++)
		{
			for (int column = 0;
				 column < BoardSize;
				 column++)
			{
				if ((row + column) % 2 != 0)
				{
					Board[row, column] =
						new CheckerPiece(
							PieceColour.Red
						);
				}
			}
		}
	}
	public static bool IsInsideBoard(
	int row,
	int column)
{
	return row >= 0 &&
		   row < BoardSize &&
		   column >= 0 &&
		   column < BoardSize;
}

public bool IsValidSimpleMove(
	int sourceRow,
	int sourceColumn,
	int targetRow,
	int targetColumn)
{
	if (!IsInsideBoard(
		sourceRow,
		sourceColumn) ||
		!IsInsideBoard(
			targetRow,
			targetColumn))
	{
		return false;
	}

	CheckerPiece? piece =
		Board[sourceRow, sourceColumn];

	if (piece == null)
	{
		return false;
	}

	if (Board[targetRow, targetColumn] != null)
	{
		return false;
	}

	int rowDifference =
		targetRow - sourceRow;

	int columnDifference =
		Math.Abs(
			targetColumn - sourceColumn
		);

	if (columnDifference != 1)
	{
		return false;
	}

	if (piece.IsKing)
	{
		return Math.Abs(rowDifference) == 1;
	}

	if (piece.Colour == PieceColour.Red)
	{
		return rowDifference == -1;
	}

	return rowDifference == 1;
}

public bool IsValidCapture(
	int sourceRow,
	int sourceColumn,
	int targetRow,
	int targetColumn)
{
	if (!IsInsideBoard(
		sourceRow,
		sourceColumn) ||
		!IsInsideBoard(
			targetRow,
			targetColumn))
	{
		return false;
	}

	CheckerPiece? piece =
		Board[sourceRow, sourceColumn];

	if (piece == null)
	{
		return false;
	}

	if (Board[targetRow, targetColumn] != null)
	{
		return false;
	}

	int rowDifference =
		targetRow - sourceRow;

	int columnDifference =
		targetColumn - sourceColumn;

	if (Math.Abs(columnDifference) != 2)
	{
		return false;
	}

	if (piece.IsKing)
	{
		if (Math.Abs(rowDifference) != 2)
		{
			return false;
		}
	}
	else if (piece.Colour == PieceColour.Red)
	{
		if (rowDifference != -2)
		{
			return false;
		}
	}
	else
	{
		if (rowDifference != 2)
		{
			return false;
		}
	}

	int jumpedRow =
		sourceRow + rowDifference / 2;

	int jumpedColumn =
		sourceColumn + columnDifference / 2;

	CheckerPiece? jumpedPiece =
		Board[jumpedRow, jumpedColumn];

	return jumpedPiece != null &&
		   jumpedPiece.Colour != piece.Colour;
}
public bool HasAvailableCapture(
	int row,
	int column)
{
	if (!IsInsideBoard(row, column))
	{
		return false;
	}

	CheckerPiece? piece =
		Board[row, column];

	if (piece == null)
	{
		return false;
	}

	int[] rowDirections;

	if (piece.IsKing)
	{
		rowDirections = new[] { -2, 2 };
	}
	else if (piece.Colour == PieceColour.Red)
	{
		rowDirections = new[] { -2 };
	}
	else
	{
		rowDirections = new[] { 2 };
	}

	int[] columnDirections =
		new[] { -2, 2 };

	foreach (int rowDirection in rowDirections)
	{
		foreach (int columnDirection
				 in columnDirections)
		{
			int targetRow =
				row + rowDirection;

			int targetColumn =
				column + columnDirection;

			if (!IsInsideBoard(
				targetRow,
				targetColumn))
			{
				continue;
			}

			if (IsValidCapture(
				row,
				column,
				targetRow,
				targetColumn))
			{
				return true;
			}
		}
	}

	return false;
}

public bool PlayerHasAnyCapture(
	PieceColour colour)
{
	for (int row = 0;
		 row < BoardSize;
		 row++)
	{
		for (int column = 0;
			 column < BoardSize;
			 column++)
		{
			CheckerPiece? piece =
				Board[row, column];

			if (piece != null &&
				piece.Colour == colour &&
				HasAvailableCapture(
					row,
					column))
			{
				return true;
			}
		}
	}

	return false;
}

public bool HasAvailableSimpleMove(
	int row,
	int column)
{
	if (!IsInsideBoard(row, column))
	{
		return false;
	}

	CheckerPiece? piece =
		Board[row, column];

	if (piece == null)
	{
		return false;
	}

	int[] rowDirections;

	if (piece.IsKing)
	{
		rowDirections = new[] { -1, 1 };
	}
	else if (piece.Colour == PieceColour.Red)
	{
		rowDirections = new[] { -1 };
	}
	else
	{
		rowDirections = new[] { 1 };
	}

	int[] columnDirections =
		new[] { -1, 1 };

	foreach (int rowDirection in rowDirections)
	{
		foreach (int columnDirection
				 in columnDirections)
		{
			int targetRow =
				row + rowDirection;

			int targetColumn =
				column + columnDirection;

			if (!IsInsideBoard(
				targetRow,
				targetColumn))
			{
				continue;
			}

			if (IsValidSimpleMove(
				row,
				column,
				targetRow,
				targetColumn))
			{
				return true;
			}
		}
	}

	return false;
}

public bool PlayerHasAnyLegalMove(
	PieceColour colour)
{
	for (int row = 0;
		 row < BoardSize;
		 row++)
	{
		for (int column = 0;
			 column < BoardSize;
			 column++)
		{
			CheckerPiece? piece =
				Board[row, column];

			if (piece == null ||
				piece.Colour != colour)
			{
				continue;
			}

			if (HasAvailableCapture(
					row,
					column) ||
				HasAvailableSimpleMove(
					row,
					column))
			{
				return true;
			}
		}
	}

	return false;
}

public int CountPieces(
	PieceColour colour)
{
	int count = 0;

	for (int row = 0;
		 row < BoardSize;
		 row++)
	{
		for (int column = 0;
			 column < BoardSize;
			 column++)
		{
			CheckerPiece? piece =
				Board[row, column];

			if (piece != null &&
				piece.Colour == colour)
			{
				count++;
			}
		}
	}

	return count;
}
public void PromotePieceIfNeeded(
	CheckerPiece piece,
	int row)
{
	if (piece.Colour == PieceColour.Red &&
		row == 0)
	{
		piece.IsKing = true;
	}

	if (piece.Colour == PieceColour.Black &&
		row == BoardSize - 1)
	{
		piece.IsKing = true;
	}
}

public bool CheckForWinner()
{
	PieceColour? winningColour = null;

	if (CountPieces(PieceColour.Red) == 0)
	{
		winningColour =
			PieceColour.Black;
	}
	else if (CountPieces(PieceColour.Black) == 0)
	{
		winningColour =
			PieceColour.Red;
	}
	else if (!PlayerHasAnyLegalMove(CurrentTurn))
	{
		winningColour =
			CurrentTurn == PieceColour.Red
				? PieceColour.Black
				: PieceColour.Red;
	}

	if (winningColour == null)
	{
		return false;
	}

	GameOver = true;
	Winner = winningColour;

	SelectedRow = null;
	SelectedColumn = null;

	MustContinueCapture = false;

	return true;
}
public bool MovePiece(
	int sourceRow,
	int sourceColumn,
	int targetRow,
	int targetColumn,
	bool isCapture)
{
	if (!IsInsideBoard(
			sourceRow,
			sourceColumn) ||
		!IsInsideBoard(
			targetRow,
			targetColumn))
	{
		return false;
	}

	CheckerPiece? piece =
		Board[sourceRow, sourceColumn];

	if (piece == null)
	{
		return false;
	}
	LastMoveSourceRow = sourceRow;
LastMoveSourceColumn = sourceColumn;

LastMoveTargetRow = targetRow;
LastMoveTargetColumn = targetColumn;

	Board[targetRow, targetColumn] = piece;
	Board[sourceRow, sourceColumn] = null;

	if (isCapture)
{
	int jumpedRow =
		(sourceRow + targetRow) / 2;

	int jumpedColumn =
		(sourceColumn + targetColumn) / 2;

	Board[jumpedRow, jumpedColumn] = null;

	if (piece.Colour == PieceColour.Red)
	{
		RedCaptures++;
	}
	else
	{
		BlackCaptures++;
	}
}

	PromotePieceIfNeeded(
		piece,
		targetRow
	);

	if (isCapture &&
		HasAvailableCapture(
			targetRow,
			targetColumn))
	{
		SelectedRow = targetRow;
		SelectedColumn = targetColumn;

		MustContinueCapture = true;

		return true;
	}
MoveCount++;
	SelectedRow = null;
	SelectedColumn = null;

	MustContinueCapture = false;

	CurrentTurn =
		CurrentTurn == PieceColour.Red
			? PieceColour.Black
			: PieceColour.Red;

	CheckForWinner();

	return true;
}
public PieceSelectionResult SelectPiece(
	int row,
	int column)
{
	if (!IsInsideBoard(row, column))
	{
		return PieceSelectionResult.OutsideBoard;
	}

	CheckerPiece? piece =
		Board[row, column];

	if (piece == null)
	{
		return PieceSelectionResult.EmptySquare;
	}

	if (piece.Colour != CurrentTurn)
	{
		return PieceSelectionResult.WrongPlayer;
	}

	bool captureAvailable =
		PlayerHasAnyCapture(CurrentTurn);

	if (captureAvailable &&
		!HasAvailableCapture(row, column))
	{
		return
			PieceSelectionResult
				.MustSelectCapturingPiece;
	}

	SelectedRow = row;
	SelectedColumn = column;

	return PieceSelectionResult.Selected;
}

public void ClearSelection()
{
	SelectedRow = null;
	SelectedColumn = null;
}
public BoardClickResult HandleSquarePressed(
	int row,
	int column)
{
	if (GameOver)
	{
		return BoardClickResult.IgnoredGameOver;
	}

	if (!IsInsideBoard(row, column))
	{
		return BoardClickResult.OutsideBoard;
	}

	CheckerPiece? clickedPiece =
		Board[row, column];

	// Nothing has been selected yet.
	if (SelectedRow == null ||
		SelectedColumn == null)
	{
		return ConvertSelectionResult(
			SelectPiece(row, column)
		);
	}

	int sourceRow =
		SelectedRow.Value;

	int sourceColumn =
		SelectedColumn.Value;

	// The same piece must continue a chained capture.
	if (MustContinueCapture)
	{
		if (!IsValidCapture(
			sourceRow,
			sourceColumn,
			row,
			column))
		{
			return
				BoardClickResult
					.MustContinueCapture;
		}

		bool moveCompleted =
			MovePiece(
				sourceRow,
				sourceColumn,
				row,
				column,
				true
			);

		return moveCompleted
			? BoardClickResult.MoveCompleted
			: BoardClickResult.InvalidMove;
	}

	// Clicking the selected piece again deselects it.
	if (sourceRow == row &&
		sourceColumn == column)
	{
		ClearSelection();

		return BoardClickResult.SelectionCleared;
	}

	// Clicking another friendly piece changes selection.
	if (clickedPiece != null &&
		clickedPiece.Colour == CurrentTurn)
	{
		return ConvertSelectionResult(
			SelectPiece(row, column)
		);
	}

	bool captureRequired =
		PlayerHasAnyCapture(CurrentTurn);

	// Normal movement is forbidden when a capture exists.
	if (captureRequired)
	{
		if (!IsValidCapture(
			sourceRow,
			sourceColumn,
			row,
			column))
		{
			return BoardClickResult.CaptureRequired;
		}

		bool captureCompleted =
			MovePiece(
				sourceRow,
				sourceColumn,
				row,
				column,
				true
			);

		return captureCompleted
			? BoardClickResult.MoveCompleted
			: BoardClickResult.InvalidMove;
	}

	if (IsValidSimpleMove(
		sourceRow,
		sourceColumn,
		row,
		column))
	{
		bool moveCompleted =
			MovePiece(
				sourceRow,
				sourceColumn,
				row,
				column,
				false
			);

		return moveCompleted
			? BoardClickResult.MoveCompleted
			: BoardClickResult.InvalidMove;
	}

	if (IsValidCapture(
		sourceRow,
		sourceColumn,
		row,
		column))
	{
		bool captureCompleted =
			MovePiece(
				sourceRow,
				sourceColumn,
				row,
				column,
				true
			);

		return captureCompleted
			? BoardClickResult.MoveCompleted
			: BoardClickResult.InvalidMove;
	}

	return BoardClickResult.InvalidMove;
}
private static BoardClickResult ConvertSelectionResult(
	PieceSelectionResult result)
{
	switch (result)
	{
		case PieceSelectionResult.Selected:
			return BoardClickResult.PieceSelected;

		case PieceSelectionResult.EmptySquare:
			return BoardClickResult.EmptySquare;

		case PieceSelectionResult.WrongPlayer:
			return BoardClickResult.WrongPlayer;

		case PieceSelectionResult
			.MustSelectCapturingPiece:

			return BoardClickResult
				.MustSelectCapturingPiece;

		case PieceSelectionResult.OutsideBoard:
			return BoardClickResult.OutsideBoard;

		default:
			return BoardClickResult.InvalidMove;
	}
}
}
