using Godot;
 
public partial class TutorialManager : CanvasLayer
{
	public enum TutorialStep
	{
		ShowTimerMoves, // 0 — таймер и ходы
		ShowMotor,      // 1 — мотор
		ShowTarget,     // 2 — таргет
		SelectAxis,     // 3 — ось (триггер: выбрал ось)
		SelectGear,     // 4 — панель шестерёнок (триггер: поставил шестерёнку)
		Done
	}
 
	public static TutorialStep CurrentStep = TutorialStep.ShowTimerMoves;
 
	[Export] public Godot.Collections.Array<Texture2D> StepTextures = new();
 
	private Label _hintLabel;
	private TextureRect _overlay;
	private TextureButton _okButton;
 
	public override void _Ready()
	{
		Layer = 100;
 
		if (SaveSystem.HasAchievement("tutorial_done"))
		{
			Visible = false;
			return;
		}
 
		_hintLabel = GetNode<Label>("TextureRect/Label");
		_overlay   = GetNode<TextureRect>("TextureRect2");
		_okButton  = GetNodeOrNull<TextureButton>("OkButton");
 
		if (_okButton != null)
			_okButton.Pressed += OnOkPressed;
 
		CurrentStep = TutorialStep.ShowTimerMoves;
		UpdateHint();
	}
 
	private void OnOkPressed()
	{
		if (CurrentStep == TutorialStep.ShowTimerMoves ||
			CurrentStep == TutorialStep.ShowMotor      ||
			CurrentStep == TutorialStep.ShowTarget)
		{
			NextStep();
		}
	}
 
	public override void _Process(double delta)
	{
		if (CurrentStep == TutorialStep.Done) return;
 
		var gm = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
		if (gm == null) return;
 
		switch (CurrentStep)
		{
			case TutorialStep.SelectAxis:
				if (GameManager.SelectedAxis != null)
					NextStep();
				break;
 
			case TutorialStep.SelectGear:
				if (gm.GetAllGears().Count > 0)
					NextStep();
				break;
		}
	}
 
	private void NextStep()
	{
		CurrentStep++;
		if (CurrentStep == TutorialStep.Done)
		{
			SaveSystem.UnlockAchievement("tutorial_done");
			Visible = false;
			return;
		}
 
		if (_okButton != null)
			_okButton.Visible =
				CurrentStep == TutorialStep.ShowTimerMoves ||
				CurrentStep == TutorialStep.ShowMotor      ||
				CurrentStep == TutorialStep.ShowTarget;
 
		UpdateHint();
	}
 
	private void UpdateHint()
	{
		_hintLabel.Text = CurrentStep switch
		{
			TutorialStep.ShowTimerMoves => Tr("TUT_TIMER_MOVES"),
			TutorialStep.ShowMotor      => Tr("TUT_MOTOR"),
			TutorialStep.ShowTarget     => Tr("TUT_TARGET"),
			TutorialStep.SelectAxis     => Tr("TUT_SELECT_AXIS"),
			TutorialStep.SelectGear     => Tr("TUT_SELECT_GEAR"),
			_ => ""
		};
 
		int stepIndex = (int)CurrentStep;
		if (StepTextures != null && stepIndex < StepTextures.Count)
			_overlay.Texture = StepTextures[stepIndex];
		else
			_overlay.Texture = null;
	}
}
