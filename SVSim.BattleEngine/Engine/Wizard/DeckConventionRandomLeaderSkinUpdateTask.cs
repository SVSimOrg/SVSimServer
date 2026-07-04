using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class DeckConventionRandomLeaderSkinUpdateTask : BaseTask
{
	private class DeckConventionRandomLeaderSkinUpdateParam : BaseParam
	{
		public string tournament_id;

		public int deck_no;

		public int[] leader_skin_id_list;
	}

	private ConventionDeckList _deckList;

	public int SelectedSkinId { get; private set; }

	public DeckConventionRandomLeaderSkinUpdateTask()
	{
		base.type = ApiType.Type.DeckRandomLeaderSkinUpdateConvention;
	}

	public void SetParameter(int deckNo, int[] leaderSkinIdList, ConventionDeckList deckList)
	{
		_deckList = deckList;
		DeckConventionRandomLeaderSkinUpdateParam deckConventionRandomLeaderSkinUpdateParam = new DeckConventionRandomLeaderSkinUpdateParam();
		deckConventionRandomLeaderSkinUpdateParam.tournament_id = _deckList.Conventioninfo.Id;
		deckConventionRandomLeaderSkinUpdateParam.deck_no = deckNo;
		deckConventionRandomLeaderSkinUpdateParam.leader_skin_id_list = leaderSkinIdList;
		base.Params = deckConventionRandomLeaderSkinUpdateParam;
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
		_deckList.DeckList[key].IsSkinRandom = true;
		JsonData jsonData2 = jsonData["leader_skin_id_list"];
		_deckList.DeckList[key].SelectRandomSkinIdList = new List<int>();
		for (int i = 0; i < jsonData2.Count; i++)
		{
			_deckList.DeckList[key].SelectRandomSkinIdList.Add(jsonData2[i].ToInt());
		}
		SelectedSkinId = jsonData["leader_skin_id"].ToInt();
		return num;
	}
}
