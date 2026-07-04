namespace Wizard;

public class GatheringUpdateDeckName : BaseTask
{
	public class GatheringUpdateDeckNameParam : BaseParam
	{
		public int deck_no;

		public string deck_name;
	}

	public GatheringUpdateDeckName()
	{
		base.type = ApiType.Type.GatheringUpdateDeckName;
	}

	public void SetParameter(int deckNo, string deckName)
	{
		GatheringUpdateDeckNameParam gatheringUpdateDeckNameParam = new GatheringUpdateDeckNameParam();
		gatheringUpdateDeckNameParam.deck_no = deckNo;
		gatheringUpdateDeckNameParam.deck_name = deckName;
		base.Params = gatheringUpdateDeckNameParam;
	}
}
