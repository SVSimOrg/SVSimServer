namespace Wizard;

public class DeckOrderTask : BaseTask
{
	public class DeckOrderTaskParam : BaseParam
	{
		public int[] deck_order;

		public int deck_format;
	}

	private Format _updateDeckFormat;

	public DeckOrderTask()
	{
		base.type = ApiType.Type.DeckOrder;
	}

	public void SetParameter(int[] deck_order, Format format)
	{
		DeckOrderTaskParam deckOrderTaskParam = new DeckOrderTaskParam();
		deckOrderTaskParam.deck_order = deck_order;
		deckOrderTaskParam.deck_format = Data.FormatConvertApi(format);
		_updateDeckFormat = format;
		base.Params = deckOrderTaskParam;
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
