using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public static class FirebaseManager
{
	private const string ApiKey  = "AIzaSyCtsKGmjxYLrk9dlsvUxbfxyK1N3Lly4t0";
	private const string DbUrl   = "https://cogchain-8834c-default-rtdb.europe-west1.firebasedatabase.app";
	private const string AuthUrl = "https://identitytoolkit.googleapis.com/v1/accounts";

	public static string IdToken = "";
	public static string LocalId = "";

	// ─── AUTH ─────────────────────────────────────────────────────────────────

	public static async System.Threading.Tasks.Task<(bool ok, string error)> Register(string email, string password)
	{
		var body = JsonSerializer.Serialize(new { email, password, returnSecureToken = true });
		var (ok, json) = await Post($"{AuthUrl}:signUp?key={ApiKey}", body);
		if (!ok) return (false, ParseError(json));
		ParseAuth(json);
		return (true, "");
	}

	public static async System.Threading.Tasks.Task<(bool ok, string error)> Login(string email, string password)
	{
		var body = JsonSerializer.Serialize(new { email, password, returnSecureToken = true });
		var (ok, json) = await Post($"{AuthUrl}:signInWithPassword?key={ApiKey}", body);
		if (!ok) return (false, ParseError(json));
		ParseAuth(json);
		return (true, "");
	}

	// ─── NICKNAMES ────────────────────────────────────────────────────────────

	public static async System.Threading.Tasks.Task<bool> SaveNickname(string username, string email)
	{
		var body = JsonSerializer.Serialize(email);
		var (ok, _) = await Put($"{DbUrl}/nicknames/{username}.json", body);
		return ok;
	}

	public static async System.Threading.Tasks.Task<string> FindEmailByNickname(string username)
	{
		var (ok, json) = await Get($"{DbUrl}/nicknames/{username}.json");
		if (!ok || json == "null" || string.IsNullOrEmpty(json)) return null;
		return json.Trim('"');
	}

	// ─── DATABASE ─────────────────────────────────────────────────────────────

	public static async System.Threading.Tasks.Task<bool> SaveUserData(UserData data)
	{
		if (string.IsNullOrEmpty(IdToken) || string.IsNullOrEmpty(LocalId)) return false;

		var levels = new Dictionary<string, object>();
		foreach (var lvl in data.LevelResults)
		{
			levels[lvl.Key.ToString()] = new {
				completed = lvl.Value.Completed,
				bestTime  = lvl.Value.BestTime,
				bestMoves = lvl.Value.BestMoves,
				bestStars = lvl.Value.BestStars
			};
		}

		// Если нет уровней — добавляем placeholder чтобы Firebase не создал массив []
		if (levels.Count == 0)
			levels["_"] = new { placeholder = true };

		var payload = new {
			username     = data.Username,
			levels,
			achievements = string.Join(",", data.Achievements)
		};

		var body = JsonSerializer.Serialize(payload);
		var (ok, _) = await Put($"{DbUrl}/users/{LocalId}.json?auth={IdToken}", body);
		return ok;
	}

	public static async System.Threading.Tasks.Task<UserData> LoadUserData()
	{
		if (string.IsNullOrEmpty(IdToken) || string.IsNullOrEmpty(LocalId)) return null;

		var (ok, json) = await Get($"{DbUrl}/users/{LocalId}.json?auth={IdToken}");
		if (!ok || json == "null" || string.IsNullOrEmpty(json)) return new UserData();

		var data = new UserData();
		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			if (root.TryGetProperty("username", out var unEl))
				data.Username = unEl.GetString() ?? "";

			if (root.TryGetProperty("achievements", out var achEl))
			{
				var achStr = achEl.GetString() ?? "";
				if (!string.IsNullOrEmpty(achStr))
					foreach (var a in achStr.Split(','))
						data.Achievements.Add(a);
			}

			if (root.TryGetProperty("levels", out var levelsEl))
			{
				foreach (var lvl in levelsEl.EnumerateObject())
				{
					if (lvl.Name == "_") continue; // пропускаем placeholder
					if (!int.TryParse(lvl.Name, out int idx)) continue;
					data.LevelResults[idx] = new LevelResult
					{
						Completed = lvl.Value.GetProperty("completed").GetBoolean(),
						BestTime  = lvl.Value.GetProperty("bestTime").GetSingle(),
						BestMoves = lvl.Value.GetProperty("bestMoves").GetInt32(),
						BestStars = lvl.Value.TryGetProperty("bestStars", out var bs) ? bs.GetInt32() : 0
					};
				}
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("FirebaseManager.LoadUserData: " + e.Message);
		}

		return data;
	}

	// ─── HTTP helpers ─────────────────────────────────────────────────────────

	private static async System.Threading.Tasks.Task<(bool, string)> Post(string url, string body)
	{
		using var client = new System.Net.Http.HttpClient();
		var content = new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");
		var resp = await client.PostAsync(url, content);
		var json = await resp.Content.ReadAsStringAsync();
		return (resp.IsSuccessStatusCode, json);
	}

	private static async System.Threading.Tasks.Task<(bool, string)> Put(string url, string body)
	{
		using var client = new System.Net.Http.HttpClient();
		var content = new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json");
		var resp = await client.PutAsync(url, content);
		var json = await resp.Content.ReadAsStringAsync();
		return (resp.IsSuccessStatusCode, json);
	}

	private static async System.Threading.Tasks.Task<(bool, string)> Get(string url)
	{
		using var client = new System.Net.Http.HttpClient();
		var resp = await client.GetAsync(url);
		var json = await resp.Content.ReadAsStringAsync();
		return (resp.IsSuccessStatusCode, json);
	}

	private static void ParseAuth(string json)
	{
		using var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;
		IdToken = root.TryGetProperty("idToken", out var t) ? t.GetString() : "";
		LocalId = root.TryGetProperty("localId", out var l) ? l.GetString() : "";
	}

	private static string ParseError(string json)
	{
		try
		{
			using var doc = JsonDocument.Parse(json);
			if (doc.RootElement.TryGetProperty("error", out var err))
				if (err.TryGetProperty("message", out var msg))
					return msg.GetString() ?? "Unknown error";
		}
		catch { }
		return "Unknown error";
	}

	public static async System.Threading.Tasks.Task<bool> UpdateRanking(string username, int totalStars)
	{
		if (string.IsNullOrEmpty(IdToken)) return false;
		var payload = new { username, totalStars };
		var body = JsonSerializer.Serialize(payload);
		var (ok, _) = await Put($"{DbUrl}/rankings/{LocalId}.json?auth={IdToken}", body);
		return ok;
	}

	public static async System.Threading.Tasks.Task<string> GetRankings()
	{
		var (ok, json) = await Get($"{DbUrl}/rankings.json");
		if (!ok || json == "null") return null;
		return json;
	}
}
