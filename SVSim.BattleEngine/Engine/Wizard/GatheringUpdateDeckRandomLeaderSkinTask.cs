using LitJson;

namespace Wizard;

public class GatheringUpdateDeckRandomLeaderSkinTask : BaseTask
{
	public class GatheringUpdateDeckRandomLeaderSkinParam : BaseParam
	{
		public int deck_no;

		public int[] leader_skin_id_list;
	}

	public int SelectedSkinId { get; private set; }

	public GatheringUpdateDeckRandomLeaderSkinTask()
	{
		base.type = ApiType.Type.GatheringUpdateDeckRandomLeaderSkin;
	}

	public void SetParameter(int deckNo, int[] leaderSkinIdList)
	{
		GatheringUpdateDeckRandomLeaderSkinParam gatheringUpdateDeckRandomLeaderSkinParam = new GatheringUpdateDeckRandomLeaderSkinParam();
		gatheringUpdateDeckRandomLeaderSkinParam.deck_no = deckNo;
		gatheringUpdateDeckRandomLeaderSkinParam.leader_skin_id_list = leaderSkinIdList;
		base.Params = gatheringUpdateDeckRandomLeaderSkinParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"]["user_deck"];
		SelectedSkinId = jsonData["leader_skin_id"].ToInt();
		return num;
	}
}
