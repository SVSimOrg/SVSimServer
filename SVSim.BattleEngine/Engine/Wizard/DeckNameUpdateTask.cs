namespace Wizard;

public class DeckNameUpdateTask : BaseTask
{
	public class DeckNameUpdateTaskParam : BaseParam
	{
		public int deck_no;

		public string deck_name;

		public int deck_format;
	}

	private Format _updateDeckFormat;

	public DeckNameUpdateTask()
	{
		base.type = ApiType.Type.DeckNameUpdate;
	}

	public void SetParameter(int deck_no, string deck_name, Format format)
	{
		DeckNameUpdateTaskParam deckNameUpdateTaskParam = new DeckNameUpdateTaskParam();
		deckNameUpdateTaskParam.deck_no = deck_no;
		deckNameUpdateTaskParam.deck_name = deck_name;
		deckNameUpdateTaskParam.deck_format = Data.FormatConvertApi(format);
		base.Params = deckNameUpdateTaskParam;
		_updateDeckFormat = format;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		DeckListUtility.DeckUpdate(base.ResponseData["data"]["user_deck"], _updateDeckFormat, DeckAttributeType.CustomDeck);
		return num;
	}
}
