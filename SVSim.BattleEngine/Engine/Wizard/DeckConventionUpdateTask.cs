namespace Wizard;

public class DeckConventionUpdateTask : BaseTask
{
	public class DeckConventionUpdateTaskParam : BaseParam
	{
		public string tournament_id;

		public int deck_no;

		public int class_id;

		public int leader_skin_id;

		public bool is_random_leader_skin;

		public int[] leader_skin_id_list;

		public long sleeve_id;

		public string deck_name;

		public int is_delete;

		public string rotation_id;

		public int[] card_id_array;
	}

	public class DeckConventionUpdateTaskParamWithSubClass : BaseParam
	{
		public string tournament_id;

		public int deck_no;

		public int class_id;

		public int sub_class_id;

		public int leader_skin_id;

		public bool is_random_leader_skin;

		public int[] leader_skin_id_list;

		public long sleeve_id;

		public string deck_name;

		public int is_delete;

		public int[] card_id_array;
	}

	private ConventionDeckList _deckList;

	public DeckConventionUpdateTask()
	{
		base.type = ApiType.Type.DeckUpdateConvention;
	}

	public void SetParameter(int deck_no, int class_id, int leader_skin_id, bool isRandomLeaderSkin, int[] leaderSkinIdList, long sleeve_id, string deck_name, bool is_delete, int[] card_id_array, string rotationId, ConventionDeckList deckList)
	{
		_deckList = deckList;
		DeckConventionUpdateTaskParam deckConventionUpdateTaskParam = new DeckConventionUpdateTaskParam();
		deckConventionUpdateTaskParam.tournament_id = _deckList.Conventioninfo.Id;
		deckConventionUpdateTaskParam.deck_no = deck_no;
		deckConventionUpdateTaskParam.class_id = class_id;
		deckConventionUpdateTaskParam.leader_skin_id = leader_skin_id;
		deckConventionUpdateTaskParam.is_random_leader_skin = isRandomLeaderSkin;
		deckConventionUpdateTaskParam.leader_skin_id_list = leaderSkinIdList;
		deckConventionUpdateTaskParam.sleeve_id = sleeve_id;
		deckConventionUpdateTaskParam.deck_name = deck_name;
		deckConventionUpdateTaskParam.is_delete = (is_delete ? 1 : 0);
		deckConventionUpdateTaskParam.card_id_array = card_id_array;
		deckConventionUpdateTaskParam.rotation_id = (string.IsNullOrEmpty(rotationId) ? string.Empty : rotationId);
		base.Params = deckConventionUpdateTaskParam;
	}

	public void SetParameterWithSubClass(int deck_no, int class_id, int subClassId, int leader_skin_id, bool isRandomLeaderSkin, int[] leaderSkinIdList, long sleeve_id, string deck_name, bool is_delete, int[] card_id_array, ConventionDeckList deckList)
	{
		_deckList = deckList;
		DeckConventionUpdateTaskParamWithSubClass deckConventionUpdateTaskParamWithSubClass = new DeckConventionUpdateTaskParamWithSubClass();
		deckConventionUpdateTaskParamWithSubClass.tournament_id = _deckList.Conventioninfo.Id;
		deckConventionUpdateTaskParamWithSubClass.deck_no = deck_no;
		deckConventionUpdateTaskParamWithSubClass.class_id = class_id;
		deckConventionUpdateTaskParamWithSubClass.sub_class_id = subClassId;
		deckConventionUpdateTaskParamWithSubClass.leader_skin_id = leader_skin_id;
		deckConventionUpdateTaskParamWithSubClass.is_random_leader_skin = isRandomLeaderSkin;
		deckConventionUpdateTaskParamWithSubClass.leader_skin_id_list = leaderSkinIdList;
		deckConventionUpdateTaskParamWithSubClass.sleeve_id = sleeve_id;
		deckConventionUpdateTaskParamWithSubClass.deck_name = deck_name;
		deckConventionUpdateTaskParamWithSubClass.is_delete = (is_delete ? 1 : 0);
		deckConventionUpdateTaskParamWithSubClass.card_id_array = card_id_array;
		base.Params = deckConventionUpdateTaskParamWithSubClass;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		_deckList.ParseDeckListJson(base.ResponseData["data"]["user_deck_list"]);
		return num;
	}
}
