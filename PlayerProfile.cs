public sealed class PlayerProfile
{
	public string Name { get; set; }

	public int Score { get; private set; }

	public PlayerProfile(
		string name)
	{
		Name = name;
	}

	public void AddWin()
	{
		Score++;
	}

	public void ResetScore()
	{
		Score = 0;
	}
}
