using System;
using System.Collections.Generic;
using System.Text;

// Leaderboard values are calculated from saved match history.
// No second statistics save file is needed.
public sealed class PlayerStats
{
    public string Name { get; }
    public int Wins { get; internal set; }
    public int Losses { get; internal set; }
    public int Captures { get; internal set; }

    public int GamesPlayed => Wins + Losses;
    public double WinRate => GamesPlayed == 0
        ? 0
        : 100.0 * Wins / GamesPlayed;

    public PlayerStats(string name)
    {
        Name = name;
    }
}

public static class PlayerLeaderboard
{
    public static string BuildDisplayText(IReadOnlyList<MatchRecord> matches)
    {
        if (matches.Count == 0)
        {
            return "No completed matches yet.\n\nFinish a game to create player statistics.";
        }

        var byName = new Dictionary<string, PlayerStats>(
            StringComparer.OrdinalIgnoreCase);
        int unrankedMatches = 0;

        foreach (MatchRecord match in matches)
        {
            string nameOne = (match.PlayerOneName ?? "").Trim();
            string nameTwo = (match.PlayerTwoName ?? "").Trim();
            string winner = (match.WinnerName ?? "").Trim();

            // Existing history only identifies the winner by name.
            // Identical names, missing names, or an unknown winner can't
            // be ranked reliably, so leave those records in history only.
            if (string.IsNullOrWhiteSpace(nameOne) ||
                string.IsNullOrWhiteSpace(nameTwo) ||
                string.Equals(nameOne, nameTwo,
                    StringComparison.OrdinalIgnoreCase))
            {
                unrankedMatches++;
                continue;
            }

            bool oneWon = string.Equals(winner, nameOne,
                StringComparison.OrdinalIgnoreCase);
            bool twoWon = string.Equals(winner, nameTwo,
                StringComparison.OrdinalIgnoreCase);

            if (!oneWon && !twoWon)
            {
                unrankedMatches++;
                continue;
            }

            PlayerStats playerOne = FindOrCreate(byName, nameOne);
            PlayerStats playerTwo = FindOrCreate(byName, nameTwo);

            playerOne.Captures += Math.Max(0, match.PlayerOneCaptures);
            playerTwo.Captures += Math.Max(0, match.PlayerTwoCaptures);

            if (oneWon)
            {
                playerOne.Wins++;
                playerTwo.Losses++;
            }
            else
            {
                playerTwo.Wins++;
                playerOne.Losses++;
            }
        }

        var players = new List<PlayerStats>(byName.Values);
        players.Sort((a, b) =>
        {
            int comparison = b.Wins.CompareTo(a.Wins);
            if (comparison != 0) return comparison;

            comparison = b.WinRate.CompareTo(a.WinRate);
            if (comparison != 0) return comparison;

            comparison = b.Captures.CompareTo(a.Captures);
            if (comparison != 0) return comparison;

            return StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
        });

        var text = new StringBuilder();
        text.AppendLine("LOCAL PLAYER LEADERBOARD");
        text.AppendLine($"Saved matches: {matches.Count}");
        text.AppendLine();

        if (players.Count == 0)
        {
            text.AppendLine("No rankable matches yet.");
        }
        else
        {
            int shown = Math.Min(players.Count, 10);

            for (int i = 0; i < shown; i++)
            {
                PlayerStats player = players[i];
                text.AppendLine($"{i + 1}. {player.Name}");
                text.AppendLine(
                    $"   Wins: {player.Wins}  |  Losses: {player.Losses}  |  Games: {player.GamesPlayed}");
                text.AppendLine(
                    $"   Win rate: {player.WinRate:0.#}%  |  Captures: {player.Captures}");
                text.AppendLine();
            }
        }

        if (unrankedMatches > 0)
        {
            text.AppendLine(
                $"{unrankedMatches} match(es) could not be ranked because player names or winner data were ambiguous.");
        }

        return text.ToString();
    }

    private static PlayerStats FindOrCreate(
        Dictionary<string, PlayerStats> players, string name)
    {
        if (!players.TryGetValue(name, out PlayerStats? stats))
        {
            stats = new PlayerStats(name);
            players.Add(name, stats);
        }

        return stats;
    }
}
