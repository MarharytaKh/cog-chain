using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public static class SaveSystem
{
	private const string SavePath = "user://saves.json";

	public static Dictionary<string, UserData> Users = new();
	public static UserData CurrentUser = null;

	public static string HashPassword(string password)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToHexString(bytes);
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
					["bestTime"]  = lvl.Value.BestTime,
					["bestMoves"] = lvl.Value.BestMoves,
					["bestStars"] = lvl.Value.BestStars
				};
			}
			data[user.Key] = new Godot.Collections.Dictionary
			{
				["passwordHash"] = user.Value.PasswordHash,
				["levels"]       = levels,
				["achievements"] = string.Join(",", user.Value.Achievements)
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
				Username     = key.ToString(),
				PasswordHash = userData["passwordHash"].ToString()
			};

			if (userData.ContainsKey("achievements"))
			{
				string achStr = userData["achievements"].ToString();
				if (!string.IsNullOrEmpty(achStr))
					foreach (var a in achStr.Split(','))
						user.Achievements.Add(a);
			}

			var levels = userData["levels"].AsGodotDictionary();
			foreach (var lvlKey in levels.Keys)
			{
				var lvl = levels[lvlKey].AsGodotDictionary();
				user.LevelResults[int.Parse(lvlKey.ToString())] = new LevelResult
				{
					Completed  = lvl["completed"].AsBool(),
					BestTime   = lvl["bestTime"].AsSingle(),
					BestMoves  = lvl["bestMoves"].AsInt32(),
					BestStars  = lvl.ContainsKey("bestStars") ? lvl["bestStars"].AsInt32() : 0
				};
			}
			Users[key.ToString()] = user;
		}
	}

	public static bool Register(string username, string password)
	{
		if (Users.ContainsKey(username)) return false;
		var user = new UserData
		{
			Username     = username,
			PasswordHash = HashPassword(password)
		};
		Users[username] = user;
		Save();
		return true;
	}

	public static bool Login(string username, string password)
	{
		if (!Users.ContainsKey(username)) return false;
		if (Users[username].PasswordHash != HashPassword(password)) return false;
		CurrentUser = Users[username];
		return true;
	}

	public static void Logout()
	{
		CurrentUser = null;
	}

	public static void RemoveUser(string username)
	{
		if (Users.ContainsKey(username))
		{
			Users.Remove(username);
			Save();
		}
	}

	public static void SaveLevelResult(int levelIndex, float time, int moves, int stars)
	{
		if (CurrentUser == null) return;

		if (!CurrentUser.LevelResults.ContainsKey(levelIndex))
		{
			CurrentUser.LevelResults[levelIndex] = new LevelResult
			{
				Completed = true,
				BestTime  = time,
				BestMoves = moves,
				BestStars = stars
			};
		}
		else
		{
			var result = CurrentUser.LevelResults[levelIndex];
			result.Completed = true;
			if (time  < result.BestTime)  result.BestTime  = time;
			if (moves < result.BestMoves) result.BestMoves = moves;
			if (stars > result.BestStars) result.BestStars = stars;
		}
		_ = SyncToFirebase();
		Save();
	}

	private static async System.Threading.Tasks.Task SyncToFirebase()
	{
		try
		{
			if (!string.IsNullOrEmpty(FirebaseManager.IdToken) && CurrentUser != null)
				await FirebaseManager.SaveUserData(CurrentUser);
		}
		catch { }
	}

	public static int GetBestStars(int levelIndex)
	{
		if (CurrentUser == null) return 0;
		if (!CurrentUser.LevelResults.ContainsKey(levelIndex)) return 0;
		return CurrentUser.LevelResults[levelIndex].BestStars;
	}

	public static bool UnlockAchievement(string key)
	{
		if (CurrentUser == null) return false;
		if (CurrentUser.Achievements.Contains(key)) return false;
		CurrentUser.Achievements.Add(key);
		Save();
		return true;
	}

	public static bool HasAchievement(string key)
	{
		return CurrentUser?.Achievements.Contains(key) ?? false;
	}

	public static void MergeWithCloud(UserData cloudData)
	{
		if (CurrentUser == null) return;

		foreach (var ach in cloudData.Achievements)
			CurrentUser.Achievements.Add(ach);

		foreach (var lvl in cloudData.LevelResults)
		{
			int idx   = lvl.Key;
			var cloud = lvl.Value;

			if (!CurrentUser.LevelResults.ContainsKey(idx))
			{
				CurrentUser.LevelResults[idx] = cloud;
			}
			else
			{
				var local = CurrentUser.LevelResults[idx];
				if (cloud.BestStars > local.BestStars) local.BestStars = cloud.BestStars;
				if (cloud.BestTime  < local.BestTime)  local.BestTime  = cloud.BestTime;
				if (cloud.BestMoves < local.BestMoves) local.BestMoves = cloud.BestMoves;
				if (cloud.Completed) local.Completed = true;
			}
		}

		Save();
	}
}

public class UserData
{
	public string Username;
	public string PasswordHash;
	public Dictionary<int, LevelResult> LevelResults = new();
	public HashSet<string> Achievements = new();
}

public class LevelResult
{
	public bool Completed;
	public float BestTime;
	public int BestMoves;
	public int BestStars;
}
