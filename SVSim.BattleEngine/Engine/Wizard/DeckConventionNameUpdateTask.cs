using LitJson;

namespace Wizard;

public class DeckConventionNameUpdateTask : BaseTask
{
	public class DeckConventionNameUpdateTaskParam : BaseParam
	{
		public string tournament_id;

		public int deck_no;

		public string deck_name;
	}

	private ConventionDeckList _deckList;

	public DeckConventionNameUpdateTask()
	{
		base.type = ApiType.Type.DeckNameUpdateConvention;
	}

	public void SetParameter(int deck_no, string deck_name, ConventionDeckList deckList)
	{
		_deckList = deckList;
		DeckConventionNameUpdateTaskParam deckConventionNameUpdateTaskParam = new DeckConventionNameUpdateTaskParam();
		deckConventionNameUpdateTaskParam.tournament_id = deckList.Conventioninfo.Id;
		deckConventionNameUpdateTaskParam.deck_no = deck_no;
		deckConventionNameUpdateTaskParam.deck_name = deck_name;
		base.Params = deckConventionNameUpdateTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"]["user_deck"];
		int key = jsonData["deck_no"].ToInt();
		_deckList.DeckList[key].SetDeckName(jsonData["deck_name"].ToString());
		return num;
	}
}
