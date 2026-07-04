using System;
using LitJson;

namespace Wizard;

public class QuestOpenInfo
{

	public bool IsDisplayBadge { get; private set; }

	public void SetIsDisplayBadge(JsonData jsonData)
	{
		IsDisplayBadge = jsonData["is_display_badge"].ToBoolean();
	}
}
