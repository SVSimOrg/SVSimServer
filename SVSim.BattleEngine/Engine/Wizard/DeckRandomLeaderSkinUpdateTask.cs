using LitJson;

namespace Wizard;

public class DeckRandomLeaderSkinUpdateTask : BaseTask
{
	private class DeckRandomLeaderSkinUpdateParam : BaseParam
	{
		public int deck_format;

		public int deck_no;

		public int[] leader_skin_id_list;
	}

	private Format _updateDeckFormat;

	public int SelectedSkinId { get; private set; }

	public DeckRandomLeaderSkinUpdateTask()
	{
		base.type = ApiType.Type.DeckRandomLeaderSkinUpdate;
	}

	public void SetParameter(Format format, int deckNo, int[] leaderSkinIdList)
	{
		DeckRandomLeaderSkinUpdateParam deckRandomLeaderSkinUpdateParam = new DeckRandomLeaderSkinUpdateParam();
		deckRandomLeaderSkinUpdateParam.deck_format = Data.FormatConvertApi(format);
		deckRandomLeaderSkinUpdateParam.deck_no = deckNo;
		deckRandomLeaderSkinUpdateParam.leader_skin_id_list = leaderSkinIdList;
		base.Params = deckRandomLeaderSkinUpdateParam;
		_updateDeckFormat = format;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"]["user_deck"];
		DeckListUtility.DeckUpdate(jsonData, _updateDeckFormat, DeckAttributeType.CustomDeck);
		SelectedSkinId = jsonData["leader_skin_id"].ToInt();
		return num;
	}
}
