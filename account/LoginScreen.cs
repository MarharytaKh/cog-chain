using Godot;

public partial class LoginScreen : CanvasLayer
{
	private LineEdit _usernameInput;
	private LineEdit _passwordInput;
	private Label _errorLabel;

	public override void _Ready()
	{
		SaveSystem.Load();

		_usernameInput = GetNode<LineEdit>("UsernameInput");
		_passwordInput = GetNode<LineEdit>("PasswordInput");
		_errorLabel = GetNode<Label>("ErrorLabel");

		_errorLabel.Text = "";

		GetNode<TextureButton>("LoginButton").Pressed += OnLogin;
		GetNode<TextureButton>("RegisterButton").Pressed += OnRegister;
	}

	private void OnLogin()
	{
		string username = _usernameInput.Text.Trim();
		string password = _passwordInput.Text;

		if (username == "" || password == "")
		{
			_errorLabel.Text = "Заполни все поля!";
			return;
		}

		if (SaveSystem.Login(username, password)){
			var gm = GetNode<GameManager>("/root/GameManager");
	gm?.RestoreUnlocks();
			GetTree().ChangeSceneToFile("res://main.tscn");
			}
		else
			_errorLabel.Text = "Неверный логин или пароль!";
	}

	private void OnRegister()
	{
		string username = _usernameInput.Text.Trim();
		string password = _passwordInput.Text;

		if (username == "" || password == "")
		{
			_errorLabel.Text = "Заполни все поля!";
			return;
		}

		if (SaveSystem.Register(username, password))
		{
			SaveSystem.Login(username, password);
			GetTree().ChangeSceneToFile("res://main.tscn");
		}
		else
			_errorLabel.Text = "Такой пользователь уже существует!";
	}
}
