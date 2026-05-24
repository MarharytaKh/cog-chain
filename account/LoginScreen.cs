using System;
using System.Threading.Tasks;
using Godot;

public partial class LoginScreen : CanvasLayer
{
	private LineEdit _usernameInput;
	private LineEdit _passwordInput;
	private LineEdit _emailInput;
	private TextureRect _emailLabel;
	private Label _errorLabel;

	public override void _Ready()
	{
		SaveSystem.Load();

		_usernameInput = GetNode<LineEdit>("UsernameInput");
		_passwordInput = GetNode<LineEdit>("PasswordInput");
		_emailInput    = GetNodeOrNull<LineEdit>("EmailInput");
		_emailLabel    = GetNodeOrNull<TextureRect>("EmailLabel");
		_errorLabel    = GetNode<Label>("ErrorLabel");
		_errorLabel.Text = "";

		if (_emailInput != null) _emailInput.Visible = false;
		if (_emailLabel != null) _emailLabel.Visible = false;

		GetNode<TextureButton>("LoginButton").Pressed    += OnLogin;
		GetNode<TextureButton>("RegisterButton").Pressed += OnRegister;
	}

	private async void OnLogin()
	{
		string username = _usernameInput.Text.Trim();
		string password = _passwordInput.Text;

		if (username == "" || password == "")
		{
			_errorLabel.Text = Tr("FILL_FIELDS");
			return;
		}

		if (!SaveSystem.Login(username, password))
		{
			_errorLabel.Text = Tr("SEARCHING_ONLINE");
			var email = await FirebaseManager.FindEmailByNickname(username);
			if (email == null)
			{
				_errorLabel.Text = Tr("WRONG_PASSWORD");
				return;
			}

			var (ok, _) = await FirebaseManager.Login(email, password);
			if (!ok)
			{
				_errorLabel.Text = Tr("WRONG_PASSWORD");
				return;
			}

			var cloudData = await FirebaseManager.LoadUserData();
			if (cloudData != null)
			{
				SaveSystem.Register(username, password);
				SaveSystem.Login(username, password);
				SaveSystem.MergeWithCloud(cloudData);
			}
		}
		else
		{
			_ = SyncFirebaseLogin(username, password);
		}

		var gm = GetNode<GameManager>("/root/GameManager");
		gm?.RestoreUnlocks();
		_errorLabel.Text = "";
		GetTree().ChangeSceneToFile("res://main.tscn");
	}

	private async void OnRegister()
	{
		string username = _usernameInput.Text.Trim();
		string password = _passwordInput.Text;
		string email    = _emailInput?.Text.Trim() ?? "";

		// Показываем поле email если ещё скрыто
		if (_emailInput != null && !_emailInput.Visible)
		{
			_emailInput.Visible = true;
			if (_emailLabel != null) _emailLabel.Visible = true;
			_errorLabel.Text = Tr("ENTER_EMAIL");
			return;
		}

		if (username == "" || password == "" || email == "")
		{
			_errorLabel.Text = Tr("FILL_FIELDS");
			return;
		}

		if (!email.Contains("@"))
		{
			_errorLabel.Text = Tr("INVALID_EMAIL");
			return;
		}

		if (password.Length < 6)
		{
			_errorLabel.Text = Tr("WEAK_PASSWORD");
			return;
		}

		if (!SaveSystem.Register(username, password))
		{
			_errorLabel.Text = Tr("USER_EXISTS");
			return;
		}

		SaveSystem.Login(username, password);

		_errorLabel.Text = Tr("CREATING_ACCOUNT");
		var (ok, err) = await FirebaseManager.Register(email, password);
		if (ok)
		{
			await FirebaseManager.SaveNickname(username, email);
			await FirebaseManager.SaveUserData(SaveSystem.CurrentUser);
			_errorLabel.Text = "";
			GetTree().ChangeSceneToFile("res://main.tscn");
		}
		else
		{
			// Откатываем локальную регистрацию
			SaveSystem.Users.Remove(username);
			SaveSystem.CurrentUser = null;
			SaveSystem.Save();

			// Очищаем только email поле — форма остаётся открытой
			if (_emailInput != null) _emailInput.Text = "";

			if (err.Contains("EMAIL_EXISTS"))
				_errorLabel.Text = Tr("EMAIL_IN_USE");
			else if (err.Contains("WEAK_PASSWORD"))
				_errorLabel.Text = Tr("WEAK_PASSWORD");
			else if (err.Contains("INVALID_EMAIL"))
				_errorLabel.Text = Tr("INVALID_EMAIL");
			else
				_errorLabel.Text = err;
		}
	}

	private async Task SyncFirebaseLogin(string username, string password)
	{
		try
		{
			var email = await FirebaseManager.FindEmailByNickname(username);
			if (email == null) return;

			var (ok, _) = await FirebaseManager.Login(email, password);
			if (!ok) return;

			var cloudData = await FirebaseManager.LoadUserData();
			if (cloudData != null)
				SaveSystem.MergeWithCloud(cloudData);

			if (SaveSystem.CurrentUser != null)
				await FirebaseManager.SaveUserData(SaveSystem.CurrentUser);
		}
		catch (Exception e)
		{
			GD.PrintErr("Firebase sync error: " + e.Message);
		}
	}
}
