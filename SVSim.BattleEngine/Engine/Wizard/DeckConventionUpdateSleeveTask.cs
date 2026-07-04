using LitJson;

namespace Wizard;

public class DeckConventionUpdateSleeveTask : BaseTask
{
	public class SleeveConventionSetTaskParam : BaseParam
	{
		public string tournament_id;

		public int deck_no;

		public long sleeve_id;
	}

	private ConventionDeckList _deckList;

	public DeckConventionUpdateSleeveTask()
	{
		base.type = ApiType.Type.DeckUpdateSleeveConvention;
	}

	public void SetParameter(int deck_no, long sleeve_id, ConventionDeckList deckList)
	{
		_deckList = deckList;
		SleeveConventionSetTaskParam sleeveConventionSetTaskParam = new SleeveConventionSetTaskParam();
		sleeveConventionSetTaskParam.tournament_id = deckList.Conventioninfo.Id;
		sleeveConventionSetTaskParam.deck_no = deck_no;
		sleeveConventionSetTaskParam.sleeve_id = sleeve_id;
		base.Params = sleeveConventionSetTaskParam;
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
		long deckSleeveID = jsonData["sleeve_id"].ToLong();
		_deckList.DeckList[key].SetDeckSleeveID(deckSleeveID);
		return num;
	}
}
