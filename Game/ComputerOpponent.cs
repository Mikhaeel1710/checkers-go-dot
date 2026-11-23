
using System;
using System.Collections.Generic;

public readonly struct ComputerMove
{
	public int SourceRow { get; }
	public int SourceColumn { get; }
	public int TargetRow { get; }
	public int TargetColumn { get; }
	public bool IsCapture { get; }

	public ComputerMove(
		int sourceRow,
		int sourceColumn,
		int targetRow,
		int targetColumn,
		bool isCapture)
	{
		SourceRow = sourceRow;
		SourceColumn = sourceColumn;
		TargetRow = targetRow;
		TargetColumn = targetColumn;
		IsCapture = isCapture;
	}
}

public static class ComputerOpponent
{
	// Generate only legal moves for the current player.
	public static List<ComputerMove> GetLegalMoves(
		CheckersGame game)
	{
		List<ComputerMove> captures = new();

		if (game.GameOver)
		{
			return captures;
		}

		// During multiple jumps, the same piece must move.
		if (game.MustContinueCapture)
		{
			if (game.SelectedRow.HasValue &&
				game.SelectedColumn.HasValue)
			{
				AddMoves(
					game,
					game.SelectedRow.Value,
					game.SelectedColumn.Value,
					true,
					captures
				);
			}

			return captures;
		}

		// First search for mandatory captures.
		for (int row = 0; row < CheckersGame.BoardSize; row++)
		{
			for (int column = 0;
				 column < CheckersGame.BoardSize;
				 column++)
			{
				CheckerPiece? piece =
					game.Board[row, column];

				if (piece == null ||
					piece.Colour != game.CurrentTurn)
				{
					continue;
				}

				AddMoves(
					game, row, column, true, captures
				);
			}
		}

		// Captures always take priority.
		if (captures.Count > 0)
		{
			return captures;
		}

		// No captures available: allow regular moves.
		List<ComputerMove> normalMoves = new();

		for (int row = 0; row < CheckersGame.BoardSize; row++)
		{
			for (int column = 0;
				 column < CheckersGame.BoardSize;
				 column++)
			{
				CheckerPiece? piece =
					game.Board[row, column];

				if (piece == null ||
					piece.Colour != game.CurrentTurn)
				{
					continue;
				}

				AddMoves(
					game, row, column, false, normalMoves
				);
			}
		}

		return normalMoves;
	}

	// Easy AI: choose a random legal move.
	public static ComputerMove? ChooseEasyMove(
		CheckersGame game)
	{
		List<ComputerMove> moves =
			GetLegalMoves(game);

		if (moves.Count == 0)
		{
			return null;
		}

		int index = Random.Shared.Next(moves.Count);

		return moves[index];
	}
	
	
public static ComputerMove? ChooseMediumMove(
	CheckersGame game)
{
	List<ComputerMove> moves =
		GetLegalMoves(game);

	if (moves.Count == 0)
	{
		return null;
	}

	int bestScore = int.MinValue;

	List<ComputerMove> bestMoves = new();

	foreach (ComputerMove move in moves)
	{
		CheckerPiece? piece =
			game.Board[
				move.SourceRow,
				move.SourceColumn
			];

		if (piece == null)
		{
			continue;
		}

		int score = 0;

		// Captures are mandatory under the existing rules.
		if (move.IsCapture)
		{
			score += 40;

			int jumpedRow =
				(move.SourceRow + move.TargetRow) / 2;

			int jumpedColumn =
				(move.SourceColumn + move.TargetColumn) / 2;

			CheckerPiece? capturedPiece =
				game.Board[jumpedRow, jumpedColumn];

			// Capturing an enemy king is more valuable.
			if (capturedPiece != null &&
				capturedPiece.IsKing)
			{
				score += 35;
			}
		}

		// Reward moves that create a new king.
		bool promotesToKing =
			!piece.IsKing &&
			(
				piece.Colour == PieceColour.Red &&
				move.TargetRow == 0
				||
				piece.Colour == PieceColour.Black &&
				move.TargetRow ==
					CheckersGame.BoardSize - 1
			);

		if (promotesToKing)
		{
			score += 60;
		}

		// Kings have greater movement flexibility.
		if (piece.IsKing)
		{
			score += 10;
		}

		// Slight preference for central positions.
		if (move.TargetColumn >= 2 &&
			move.TargetColumn <= 5)
		{
			score += 4;
		}
		
		// Avoid ordinary moves that expose this piece
		// to an immediate capture by the opponent.
		if (!move.IsCapture &&
			IsMoveExposedToCapture(game, move))
		{
			score -= piece.IsKing ? 90 : 60;
		}


		// Collect equally valuable moves.
		if (score > bestScore)
		{
			bestScore = score;

			bestMoves.Clear();
			bestMoves.Add(move);
		}
		else if (score == bestScore)
		{
			bestMoves.Add(move);
		}
	}

	if (bestMoves.Count == 0)
	{
		return null;
	}

	// Randomise between equally scored moves.
	int selectedIndex =
		Random.Shared.Next(bestMoves.Count);

	return bestMoves[selectedIndex];
}


private static bool IsMoveExposedToCapture(
	CheckersGame game,
	ComputerMove move)
{
	CheckerPiece? movingPiece =
		game.Board[move.SourceRow, move.SourceColumn];

	if (movingPiece == null)
	{
		return false;
	}

	int destinationRow = move.TargetRow;
	int destinationColumn = move.TargetColumn;

	for (int row = 0;
		 row < CheckersGame.BoardSize;
		 row++)
	{
		for (int column = 0;
			 column < CheckersGame.BoardSize;
			 column++)
		{
			CheckerPiece? opponent =
				game.Board[row, column];

			if (opponent == null ||
				opponent.Colour == movingPiece.Colour)
			{
				continue;
			}

			int rowDirection =
				destinationRow - row;

			int columnDirection =
				destinationColumn - column;

			// The opponent must be diagonally
			// adjacent to the proposed destination.
			if (Math.Abs(rowDirection) != 1 ||
				Math.Abs(columnDirection) != 1)
			{
				continue;
			}

			// Regular pieces capture forwards only.
			if (!opponent.IsKing)
			{
				int requiredDirection =
					opponent.Colour == PieceColour.Red
						? -1
						: 1;

				if (rowDirection != requiredDirection)
				{
					continue;
				}
			}

			int landingRow =
				destinationRow + rowDirection;

			int landingColumn =
				destinationColumn + columnDirection;

			if (!CheckersGame.IsInsideBoard(
					landingRow,
					landingColumn))
			{
				continue;
			}

			// The source square becomes empty
			// after the proposed move.
			bool landingEmpty =
				(landingRow == move.SourceRow &&
				 landingColumn == move.SourceColumn)
				||
				game.Board[
					landingRow,
					landingColumn
				] == null;

			if (landingEmpty)
			{
				return true;
			}
		}
	}

	return false;
}


	private static void AddMoves(
		CheckersGame game,
		int sourceRow,
		int sourceColumn,
		bool capture,
		List<ComputerMove> moves)
	{
		int distance = capture ? 2 : 1;

		int[] directions = { -distance, distance };

		foreach (int rowDirection in directions)
		{
			foreach (int columnDirection in directions)
			{
				int targetRow =
					sourceRow + rowDirection;

				int targetColumn =
					sourceColumn + columnDirection;

				bool valid = capture
					? game.IsValidCapture(
						sourceRow, sourceColumn,
						targetRow, targetColumn)
					: game.IsValidSimpleMove(
						sourceRow, sourceColumn,
						targetRow, targetColumn);

				if (!valid)
				{
					continue;
				}

				moves.Add(new ComputerMove(
					sourceRow,
					sourceColumn,
					targetRow,
					targetColumn,
					capture
				));
			}
		}
	}
}
