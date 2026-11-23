
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
