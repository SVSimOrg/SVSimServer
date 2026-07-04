using LitJson;

namespace Wizard;

public class QuestOpenPuzzleDialogTask : BaseTask
{
	public bool IsDisplayBadge { get; private set; }

	public PuzzleQuestInfo PuzzleQuestInfo { get; private set; }

	public QuestOpenPuzzleDialogTask()
	{
		base.type = ApiType.Type.QuestOpenPuzzleDialog;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		IsDisplayBadge = jsonData["is_display_badge"].ToBoolean();
		PuzzleQuestInfo = new PuzzleQuestInfo(jsonData);
		return num;
	}
}
