namespace Wizard;

public class DeckConventionInfoTask : BaseTask
{
	public class DeckConventionInfoTaskParam : BaseParam
	{
		public string tournament_id;

		public int deck_no;
	}

	private ConventionInfo _conventionInfo;

	public ConventionDeckList DeckList { get; private set; }

	public DeckConventionInfoTask()
	{
		base.type = ApiType.Type.DeckInfoConvention;
	}

	public void SetParameter(int deck_no, ConventionInfo conventionInfo, ConventionDeckList conventionDeckList = null)
	{
		DeckConventionInfoTaskParam deckConventionInfoTaskParam = new DeckConventionInfoTaskParam();
		_conventionInfo = conventionInfo;
		DeckList = conventionDeckList;
		if (conventionDeckList == null)
		{
			DeckList = new ConventionDeckList();
		}
		deckConventionInfoTaskParam.tournament_id = conventionInfo.Id;
		deckConventionInfoTaskParam.deck_no = deck_no;
		base.Params = deckConventionInfoTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		DeckList.Parse(base.ResponseData, _conventionInfo);
		return num;
	}
}
