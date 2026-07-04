using LitJson;

namespace Wizard;

public class DeckAutoCreateTask : BaseTask
{
	public class DeckAutoCreateTaskParam : BaseParam
	{
		public int deck_format;

		public int class_id;

		public int[] chosen_card_ids;

		public int tournament_id;

		public string rotation_id;
	}

	public class DeckAutoCreateUseSubClassTaskParam : BaseParam
	{
		public int deck_format;

		public int class_id;

		public int sub_class_id;

		public int[] chosen_card_ids;

		public int tournament_id;
	}

	public int[] _autoDeckCreateCardList;

	public DeckAutoCreateTask()
	{
		base.type = ApiType.Type.DeckAutoCreate;
	}

	public void SetParameter(Format format, int classId, int tournamentId, int[] deckCards, MyRotationInfo myRotationInfo)
	{
		DeckAutoCreateTaskParam deckAutoCreateTaskParam = new DeckAutoCreateTaskParam();
		deckAutoCreateTaskParam.deck_format = Data.FormatConvertApi(format);
		deckAutoCreateTaskParam.class_id = classId;
		deckAutoCreateTaskParam.chosen_card_ids = deckCards;
		deckAutoCreateTaskParam.tournament_id = tournamentId;
		deckAutoCreateTaskParam.rotation_id = ((myRotationInfo != null) ? myRotationInfo.Id : "");
		base.Params = deckAutoCreateTaskParam;
	}

	public void SetParameter(Format format, int classId, int subClassId, int tournamentId, int[] deckCards)
	{
		DeckAutoCreateUseSubClassTaskParam deckAutoCreateUseSubClassTaskParam = new DeckAutoCreateUseSubClassTaskParam();
		deckAutoCreateUseSubClassTaskParam.deck_format = Data.FormatConvertApi(format);
		deckAutoCreateUseSubClassTaskParam.class_id = classId;
		deckAutoCreateUseSubClassTaskParam.sub_class_id = subClassId;
		deckAutoCreateUseSubClassTaskParam.chosen_card_ids = deckCards;
		deckAutoCreateUseSubClassTaskParam.tournament_id = tournamentId;
		base.Params = deckAutoCreateUseSubClassTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		_autoDeckCreateCardList = new int[jsonData.Count];
		for (int i = 0; i < jsonData.Count; i++)
		{
			_autoDeckCreateCardList[i] = jsonData[i].ToInt();
		}
		return num;
	}
}
