namespace Wizard;

public class DeckMyListTask : BaseTask
{
	public class DeckMyListTaskParam : BaseParam
	{
		public int deck_format;
	}

	private Format _format;

	public DeckGroupListData DeckGroupListData { get; private set; }

	public DeckMyListTask()
	{
		base.type = ApiType.Type.DeckMyList;
	}

	public void SetParameter(Format format)
	{
		DeckMyListTaskParam deckMyListTaskParam = new DeckMyListTaskParam();
		deckMyListTaskParam.deck_format = Data.FormatConvertApi(format);
		base.Params = deckMyListTaskParam;
		_format = format;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		/* Pre-Phase-5b: DataMgr.SetMaintenanceCardIds dropped */
		DeckGroupListData = new DeckGroupListData(base.ResponseData["data"], _format);
		/* Pre-Phase-5b: DataMgr.CurrentDeckListParamData write dropped */
		return num;
	}
}
