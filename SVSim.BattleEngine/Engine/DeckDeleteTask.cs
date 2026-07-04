using Wizard;

public class DeckDeleteTask : BaseTask
{
	public class DeckDeleteTaskParam : BaseParam
	{
		public int[] deck_no_list;

		public int deck_format;
	}

	private Format _updateDeckFormat;

	public DeckDeleteTask()
	{
		base.type = ApiType.Type.DeckDelete;
	}

	public void SetParameter(int[] deck_no, Format format)
	{
		DeckDeleteTaskParam deckDeleteTaskParam = new DeckDeleteTaskParam();
		deckDeleteTaskParam.deck_no_list = deck_no;
		deckDeleteTaskParam.deck_format = Data.FormatConvertApi(format);
		base.Params = deckDeleteTaskParam;
		_updateDeckFormat = format;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		DeckListUtility.ParseDeckInfoResponceData(base.ResponseData["data"], _updateDeckFormat);
		return num;
	}
}
