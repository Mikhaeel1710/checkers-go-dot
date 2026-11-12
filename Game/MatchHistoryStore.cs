using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

// A snapshot of a finished match; we will also use these records for the history screen.
public sealed class MatchRecord
{
	public DateTimeOffset FinishedAt { get; set; }
	public string PlayerOneName { get; set; } = "";
	public string PlayerTwoName { get; set; } = "";
	public string WinnerName { get; set; } = "";
	public int MoveCount { get; set; }
	public int PlayerOneCaptures { get; set; }
	public int PlayerTwoCaptures { get; set; }
}

public static class MatchHistoryStore
{
	private const string SavePath = "user://checkers_match_history.json";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true
	};

	public static List<MatchRecord> Load()
	{
		string fullPath = ProjectSettings.GlobalizePath(SavePath);

		if (!File.Exists(fullPath))
		{
			return new List<MatchRecord>();
		}

		string json = File.ReadAllText(fullPath);
		return JsonSerializer.Deserialize<List<MatchRecord>>(json)
			?? new List<MatchRecord>();
	}

	public static bool TryAppend(MatchRecord record)
	{
		try
		{
			// Read existing records first. If the file is damaged, fail safely:
			// never overwrite previous match history with a blank list.
			List<MatchRecord> records = Load();
			records.Add(record);

			string fullPath = ProjectSettings.GlobalizePath(SavePath);
			string tempPath = fullPath + ".tmp";
			string json = JsonSerializer.Serialize(records, JsonOptions);

			// Write to a temporary file, then replace the history file.
			File.WriteAllText(tempPath, json);
			File.Move(tempPath, fullPath, true);

			GD.Print("Checkers: completed match saved to user://checkers_match_history.json");
			return true;
		}
		catch (Exception exception)
		{
			GD.PushError("Checkers: could not save match history: " + exception.Message);
			return false;
		}
	}
}
