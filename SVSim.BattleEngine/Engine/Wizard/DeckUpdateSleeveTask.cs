namespace Wizard;

public class DeckUpdateSleeveTask : BaseTask
{
	public class SleeveSetTaskParam : BaseParam
	{
		public int deck_no;

		public long sleeve_id;

		public int deck_format;
	}

	private Format _updateDeckFormat;

	public DeckUpdateSleeveTask()
	{
		base.type = ApiType.Type.DeckUpdateSleeve;
	}

	public void SetParameter(int deck_no, long sleeve_id, Format format)
	{
		SleeveSetTaskParam sleeveSetTaskParam = new SleeveSetTaskParam();
		sleeveSetTaskParam.deck_no = deck_no;
		sleeveSetTaskParam.sleeve_id = sleeve_id;
		sleeveSetTaskParam.deck_format = Data.FormatConvertApi(format);
		base.Params = sleeveSetTaskParam;
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
