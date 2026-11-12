public enum PieceColour
{
	Red,
	Black
}

public class CheckerPiece
{
	public PieceColour Colour { get; }

	public bool IsKing { get; set; }

	public CheckerPiece(PieceColour colour)
	{
		Colour = colour;
		IsKing = false;
	}
}
