using System.Collections.Generic;

namespace Wizard;

public class WizardUIButton : UIButton
{
	public List<EventDelegate> onPress = new List<EventDelegate>();

	public List<EventDelegate> onRelease = new List<EventDelegate>();

	public bool IsNewReplayForwardButton { get; set; }

	protected override void OnPress(bool isPressed)
	{
		base.OnPress(isPressed);
		if (UIButton.current == null && isEnabled && isPressed)
		{
			SetAsPressed();
		}
		else if (UIButton.current == null && isEnabled && !isPressed)
		{
			SetAsReleased();
		}
	}

	private void SetAsPressed()
	{
		UIButton.current = this;
		EventDelegate.Execute(onPress);
		UIButton.current = null;
		SetState(State.Pressed, immediate: true);
	}

	private void SetAsReleased()
	{
		UIButton.current = this;
		EventDelegate.Execute(onRelease);
		UIButton.current = null;
		SetState(State.Normal, immediate: true);
	}

	protected override void OnDragOut()
	{
		if ((IsNewReplayForwardButton && isEnabled) || !IsNewReplayForwardButton)
		{
			SetAsReleased();
		}
		base.OnDragOut();
	}
}
