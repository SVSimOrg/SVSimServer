using LitJson;

namespace Wizard;

public class DeckConventionLeaderSkinUpdateTask : BaseTask
{
	private class Param : BaseParam
	{
		public string tournament_id;

		public int deck_no;

		public int leader_skin_id;
	}

	private ConventionDeckList _deckList;

	public DeckConventionLeaderSkinUpdateTask()
	{
		base.type = ApiType.Type.DeckLeaderSkinUpdateConvention;
	}

	public void SetParameter(int deckNo, int leaderSkinId, ConventionDeckList deckList)
	{
		_deckList = deckList;
		Param param = new Param();
		param.tournament_id = _deckList.Conventioninfo.Id;
		param.deck_no = deckNo;
		param.leader_skin_id = leaderSkinId;
		base.Params = param;
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
		int skinId = jsonData["leader_skin_id"].ToInt();
		_deckList.DeckList[key].SetSkinId(skinId);
		_deckList.DeckList[key].IsSkinRandom = false;
		return num;
	}
}
