using System.Collections.Generic;

public class UIInputWizard : UIInput
{
	public List<EventDelegate> onSelect = new List<EventDelegate>();

	public List<EventDelegate> onDeselect = new List<EventDelegate>();

	protected override void Update()
	{
		base.Update();
	}

	protected override void OnSelectEvent()
	{
		base.OnSelectEvent();
		EventDelegate.Execute(onSelect);
	}

	protected override void OnDeselectEvent()
	{
		base.OnDeselectEvent();
		if (UIInput.current == null)
		{
			UIInput.current = this;
			EventDelegate.Execute(onDeselect);
			UIInput.current = null;
			UpdateLabel();
		}
	}
}
