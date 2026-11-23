using System;
using System.Collections.Generic;

// Searches simulated positions; never modifies the live CheckersGame board.
public static class HardComputerOpponent
{
    private const int SearchDepth = 4; // Complete turns, not individual jumps.
    private const int WinScore = 100000;
    private const int Infinity = 1000000;

    // Positive values represent Red; negative values represent Black.
    // Magnitude 1 = regular piece, magnitude 2 = king.
    private sealed class Position
    {
        public int[,] Board = new int[CheckersGame.BoardSize, CheckersGame.BoardSize];
        public int Turn; // +1 Red, -1 Black
        public int? ForcedRow;
        public int? ForcedColumn;

        public Position Copy()
        {
            Position copy = new()
            {
                Turn = Turn,
                ForcedRow = ForcedRow,
                ForcedColumn = ForcedColumn
            };

            Array.Copy(Board, copy.Board, Board.Length);
            return copy;
        }
    }

    public static ComputerMove? ChooseHardMove(CheckersGame game)
    {
        // Use the live game's proven legality checks for the actual move.
        List<ComputerMove> legalMoves = ComputerOpponent.GetLegalMoves(game);
        if (legalMoves.Count == 0)
        {
            return null;
        }

        Position position = Snapshot(game);
        int computerSide = position.Turn;
        int bestScore = -Infinity;
        List<ComputerMove> bestMoves = new();
        int alpha = -Infinity;

        foreach (ComputerMove move in legalMoves)
        {
            Position next = Apply(position, move);
            int remaining = SearchDepth - (next.Turn != position.Turn ? 1 : 0);
            int score = Minimax(next, remaining, alpha, Infinity, computerSide);

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

            alpha = Math.Max(alpha, bestScore);
        }

        // Break ties so repeated matches do not always play identically.
        return bestMoves[Random.Shared.Next(bestMoves.Count)];
    }

    private static Position Snapshot(CheckersGame game)
    {
        Position result = new()
        {
            Turn = game.CurrentTurn == PieceColour.Red ? 1 : -1,
            ForcedRow = game.MustContinueCapture ? game.SelectedRow : null,
            ForcedColumn = game.MustContinueCapture ? game.SelectedColumn : null
        };

        for (int row = 0; row < CheckersGame.BoardSize; row++)
        {
            for (int column = 0; column < CheckersGame.BoardSize; column++)
            {
                CheckerPiece? piece = game.Board[row, column];
                if (piece != null)
                {
                    int side = piece.Colour == PieceColour.Red ? 1 : -1;
                    result.Board[row, column] = side * (piece.IsKing ? 2 : 1);
                }
            }
        }

        return result;
    }

    private static int Minimax(
        Position position,
        int remainingTurns,
        int alpha,
        int beta,
        int computerSide)
    {
        List<ComputerMove> legalMoves = GetMoves(position);

        // The side that cannot move loses, including when it has no pieces.
        if (legalMoves.Count == 0)
        {
            return position.Turn == computerSide
                ? -WinScore - remainingTurns
                : WinScore + remainingTurns;
        }

        // A capture sequence must finish before evaluating a position.
        if (remainingTurns <= 0 && !position.ForcedRow.HasValue)
        {
            return Evaluate(position, computerSide);
        }

        bool maximizing = position.Turn == computerSide;
        int best = maximizing ? -Infinity : Infinity;

        foreach (ComputerMove move in legalMoves)
        {
            Position next = Apply(position, move);
            int nextDepth = remainingTurns - (next.Turn != position.Turn ? 1 : 0);
            int score = Minimax(next, nextDepth, alpha, beta, computerSide);

            if (maximizing)
            {
                best = Math.Max(best, score);
                alpha = Math.Max(alpha, best);
            }
            else
            {
                best = Math.Min(best, score);
                beta = Math.Min(beta, best);
            }

            if (beta <= alpha)
            {
                break;
            }
        }

        return best;
    }

    private static int Evaluate(Position position, int computerSide)
    {
        int score = 0;
        for (int row = 0; row < CheckersGame.BoardSize; row++)
        {
            for (int column = 0; column < CheckersGame.BoardSize; column++)
            {
                int piece = position.Board[row, column];
                if (piece == 0)
                {
                    continue;
                }

                int side = Math.Sign(piece);
                int value = Math.Abs(piece) == 2 ? 185 : 100;

                // Regular pieces benefit from advancing towards promotion.
                if (Math.Abs(piece) == 1)
                {
                    value += side == 1 ? (7 - row) * 3 : row * 3;
                }

                // Slight preference for the central four columns.
                if (column >= 2 && column <= 5)
                {
                    value += 5;
                }

                score += side == computerSide ? value : -value;
            }
        }
        return score;
    }

    private static Position Apply(Position position, ComputerMove move)
    {
        Position next = position.Copy();
        int piece = next.Board[move.SourceRow, move.SourceColumn];
        next.Board[move.SourceRow, move.SourceColumn] = 0;
        next.Board[move.TargetRow, move.TargetColumn] = piece;

        if (move.IsCapture)
        {
            next.Board[
                (move.SourceRow + move.TargetRow) / 2,
                (move.SourceColumn + move.TargetColumn) / 2] = 0;
        }

        // The live game promotes immediately, including during a jump chain.
        if (piece == 1 && move.TargetRow == 0)
        {
            next.Board[move.TargetRow, move.TargetColumn] = 2;
        }
        else if (piece == -1 && move.TargetRow == CheckersGame.BoardSize - 1)
        {
            next.Board[move.TargetRow, move.TargetColumn] = -2;
        }

        if (move.IsCapture && HasCapture(next, move.TargetRow, move.TargetColumn))
        {
            next.ForcedRow = move.TargetRow;
            next.ForcedColumn = move.TargetColumn;
        }
        else
        {
            next.ForcedRow = null;
            next.ForcedColumn = null;
            next.Turn = -next.Turn;
        }

        return next;
    }

    private static List<ComputerMove> GetMoves(Position position)
    {
        List<ComputerMove> captures = new();

        if (position.ForcedRow.HasValue && position.ForcedColumn.HasValue)
        {
            AddMoves(position, position.ForcedRow.Value,
                position.ForcedColumn.Value, true, captures);
            return captures;
        }

        for (int row = 0; row < CheckersGame.BoardSize; row++)
        {
            for (int column = 0; column < CheckersGame.BoardSize; column++)
            {
                if (position.Board[row, column] * position.Turn > 0)
                {
                    AddMoves(position, row, column, true, captures);
                }
            }
        }

        // Match the game's mandatory-capture rule.
        if (captures.Count > 0)
        {
            return captures;
        }

        List<ComputerMove> normalMoves = new();
        for (int row = 0; row < CheckersGame.BoardSize; row++)
        {
            for (int column = 0; column < CheckersGame.BoardSize; column++)
            {
                if (position.Board[row, column] * position.Turn > 0)
                {
                    AddMoves(position, row, column, false, normalMoves);
                }
            }
        }

        return normalMoves;
    }

    private static bool HasCapture(Position position, int row, int column)
    {
        List<ComputerMove> captures = new();
        AddMoves(position, row, column, true, captures);
        return captures.Count != 0;
    }

    private static void AddMoves(
        Position position,
        int sourceRow,
        int sourceColumn,
        bool capture,
        List<ComputerMove> output)
    {
        int piece = position.Board[sourceRow, sourceColumn];
        if (piece == 0 || Math.Sign(piece) != position.Turn)
        {
            return;
        }

        int distance = capture ? 2 : 1;
        int[] rowDirections = Math.Abs(piece) == 2
            ? new[] { -distance, distance }
            : new[] { -Math.Sign(piece) * distance };

        foreach (int dr in rowDirections)
        {
            foreach (int dc in new[] { -distance, distance })
            {
                int row = sourceRow + dr;
                int column = sourceColumn + dc;
                if (!Inside(row, column) || position.Board[row, column] != 0)
                {
                    continue;
                }

                if (capture)
                {
                    int middleRow = (sourceRow + row) / 2;
                    int middleColumn = (sourceColumn + column) / 2;
                    int jumped = position.Board[middleRow, middleColumn];
                    if (jumped == 0 || Math.Sign(jumped) == Math.Sign(piece))
                    {
                        continue;
                    }
                }

                output.Add(new ComputerMove(
                    sourceRow, sourceColumn, row, column, capture));
            }
        }
    }

    private static bool Inside(int row, int column) =>
        row >= 0 && row < CheckersGame.BoardSize &&
        column >= 0 && column < CheckersGame.BoardSize;
}
