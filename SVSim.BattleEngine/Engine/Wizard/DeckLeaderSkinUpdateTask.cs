namespace Wizard;

public class DeckLeaderSkinUpdateTask : BaseTask
{
	private class Param : BaseParam
	{
		public int deck_no;

		public int leader_skin_id;

		public int deck_format;
	}

	private Format _updateDeckFormat;

	public DeckLeaderSkinUpdateTask()
	{
		base.type = ApiType.Type.DeckLeaderSkinUpdate;
	}

	public void SetParameter(int deckNo, int leaderSkinId, Format format)
	{
		Param param = new Param();
		param.deck_no = deckNo;
		param.leader_skin_id = leaderSkinId;
		param.deck_format = Data.FormatConvertApi(format);
		base.Params = param;
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
