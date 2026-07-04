namespace Wizard;

public class ConventionDeckOrderTask : BaseTask
{
	public class DeckOrderTaskParam : BaseParam
	{
		public string tournament_id;

		public int[] deck_order;

		public int deck_format;
	}

	public ConventionDeckOrderTask()
	{
		base.type = ApiType.Type.ConventionDeckOrder;
	}

	public void SetParameter(string tournament_id, int[] deck_order, Format format)
	{
		DeckOrderTaskParam deckOrderTaskParam = new DeckOrderTaskParam();
		deckOrderTaskParam.tournament_id = tournament_id;
		deckOrderTaskParam.deck_order = deck_order;
		deckOrderTaskParam.deck_format = Data.FormatConvertApi(format);
		base.Params = deckOrderTaskParam;
	}
}
