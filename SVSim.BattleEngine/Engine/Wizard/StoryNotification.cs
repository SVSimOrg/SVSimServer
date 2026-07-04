using LitJson;

namespace Wizard;

public class StoryNotification
{

	public bool IsDisplayBadge { get; private set; }

	public void SetIsDisplayBadge(JsonData data)
	{
		IsDisplayBadge = data.GetValueOrDefault("is_display_badge", defaultValue: false);
	}
}
