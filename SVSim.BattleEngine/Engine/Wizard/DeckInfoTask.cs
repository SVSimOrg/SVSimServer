namespace Wizard;

public class DeckInfoTask : BaseTask
{
	public class DeckInfoTaskParam : BaseParam
	{
		public int deck_format;
	}

	public class DeckInfoTaskParamForCopySrcGet : BaseParam
	{
		public int deck_format;
	}

	private Format _format;

	public DeckGroupListData DeckGroupListData { get; private set; }

	public DeckInfoTask(bool isRoom = false)
	{
		if (isRoom)
		{
			base.type = ApiType.Type.OpenRoomDeckInfo;
		}
		else
		{
			base.type = ApiType.Type.DeckInfo;
		}
	}

	public void SetParameter(Format format)
	{
		DeckInfoTaskParam deckInfoTaskParam = new DeckInfoTaskParam();
		deckInfoTaskParam.deck_format = Data.FormatConvertApi(format);
		base.Params = deckInfoTaskParam;
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
