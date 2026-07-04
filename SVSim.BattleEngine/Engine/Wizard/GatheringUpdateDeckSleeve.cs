namespace Wizard;

public class GatheringUpdateDeckSleeve : BaseTask
{
	public class GatheringUpdateDeckSleeveParam : BaseParam
	{
		public int deck_no;

		public long sleeve_id;
	}

	public GatheringUpdateDeckSleeve()
	{
		base.type = ApiType.Type.GatheringUpdateDeckSleeve;
	}

	public void SetParameter(int deckNo, long sleeveId)
	{
		GatheringUpdateDeckSleeveParam gatheringUpdateDeckSleeveParam = new GatheringUpdateDeckSleeveParam();
		gatheringUpdateDeckSleeveParam.deck_no = deckNo;
		gatheringUpdateDeckSleeveParam.sleeve_id = sleeveId;
		base.Params = gatheringUpdateDeckSleeveParam;
	}
}
