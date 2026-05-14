using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public static class SaveSystem
{
	private const string SavePath = "user://saves.json";
	
	// Все пользователи
	public static Dictionary<string, UserData> Users = new();
	
	// Текущий залогиненный пользователь
	public static UserData CurrentUser = null;

	public static string HashPassword(string password)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToHexString(bytes);
	}

public class UserData
{
	public string Username;
	public string PasswordHash;
	public Dictionary<int, LevelResult> LevelResults = new();
}

public class LevelResult
{
	public bool Completed;
	public float BestTime;
	public int BestMoves;
}
public static void Save()
{
	var data = new Godot.Collections.Dictionary();
	foreach (var user in Users)
	{
		var levels = new Godot.Collections.Dictionary();
		foreach (var lvl in user.Value.LevelResults)
		{
			levels[lvl.Key.ToString()] = new Godot.Collections.Dictionary
			{
				["completed"] = lvl.Value.Completed,
				["bestTime"] = lvl.Value.BestTime,
				["bestMoves"] = lvl.Value.BestMoves
			};
		}
		data[user.Key] = new Godot.Collections.Dictionary
		{
			["passwordHash"] = user.Value.PasswordHash,
			["levels"] = levels
		};
	}

	using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
	file.StoreString(Json.Stringify(data));
}

public static void Load()
{
	if (!FileAccess.FileExists(SavePath)) return;

	using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
	var json = new Json();
	json.Parse(file.GetAsText());
	var data = json.Data.AsGodotDictionary();

	Users.Clear();
	foreach (var key in data.Keys)
	{
		var userData = data[key].AsGodotDictionary();
		var user = new UserData
		{
			Username = key.ToString(),
			PasswordHash = userData["passwordHash"].ToString()
		};

		var levels = userData["levels"].AsGodotDictionary();
		foreach (var lvlKey in levels.Keys)
		{
			var lvl = levels[lvlKey].AsGodotDictionary();
			user.LevelResults[int.Parse(lvlKey.ToString())] = new LevelResult
			{
				Completed = lvl["completed"].AsBool(),
				BestTime = lvl["bestTime"].AsSingle(),
				BestMoves = lvl["bestMoves"].AsInt32()
			};
		}
		Users[key.ToString()] = user;
	}
}
public static bool Register(string username, string password)
{
	if (Users.ContainsKey(username))
		return false; // пользователь уже существует

	var user = new UserData
	{
		Username = username,
		PasswordHash = HashPassword(password)
	};

	Users[username] = user;
	Save();
	return true;
}

public static bool Login(string username, string password)
{
	if (!Users.ContainsKey(username))
		return false; // пользователь не найден

	if (Users[username].PasswordHash != HashPassword(password))
		return false; // неверный пароль

	CurrentUser = Users[username];
	return true;
}

public static void Logout()
{
	CurrentUser = null;
}
public static void SaveLevelResult(int levelIndex, float time, int moves)
{
	if (CurrentUser == null) return;

	if (!CurrentUser.LevelResults.ContainsKey(levelIndex))
	{
		CurrentUser.LevelResults[levelIndex] = new LevelResult
		{
			Completed = true,
			BestTime = time,
			BestMoves = moves
		};
	}
	else
	{
		var result = CurrentUser.LevelResults[levelIndex];
		result.Completed = true;
		if (time < result.BestTime) result.BestTime = time;
		if (moves < result.BestMoves) result.BestMoves = moves;
	}

	Save();
}
}
