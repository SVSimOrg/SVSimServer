namespace Wizard;

public class GatheringUpdateDeckLeaderSkin : BaseTask
{
	public class GatheringUpdateDeckLeaderSkinParam : BaseParam
	{
		public int deck_no;

		public int leader_skin_id;
	}

	public GatheringUpdateDeckLeaderSkin()
	{
		base.type = ApiType.Type.GatheringUpdateDeckLeaderSkin;
	}

	public void SetParameter(int deckNo, int leaderSkinId)
	{
		GatheringUpdateDeckLeaderSkinParam gatheringUpdateDeckLeaderSkinParam = new GatheringUpdateDeckLeaderSkinParam();
		gatheringUpdateDeckLeaderSkinParam.deck_no = deckNo;
		gatheringUpdateDeckLeaderSkinParam.leader_skin_id = leaderSkinId;
		base.Params = gatheringUpdateDeckLeaderSkinParam;
	}
}
