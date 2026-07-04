using Cute;

namespace Wizard;

public class GenerateDeckCodeTask : BaseTask
{
	public enum SubmitDeckType
	{
		NORMAL = 1,
		SEALED = 4,
		Crossover = 6,
		MY_ROTATION = 7
	}

	public class GenerateDeckCodeTaskParam : BaseParam
	{
		public int clan;

		public int deck_format;

		public int[] cardID;

		public int[] phantomCardID;
	}

	public class GenerateDeckCodeTaskUseSubClassParam : BaseParam
	{
		public int clan;

		public int sub_clan;

		public int deck_format;

		public int[] cardID;

		public int[] phantomCardID;
	}

	public class GenerateDeckCodeTaskMyRotation : BaseParam
	{
		public int clan;

		public int deck_format;

		public int[] cardID;

		public string rotation_id;
	}

	public override string Url => $"{CustomPreference.GetDeckBuilderServerURL()}{ApiType.ApiList[base.type]}";

	public GenerateDeckCodeTask()
	{
		base.type = ApiType.Type.GenerateDeckCode;
	}

	public void SetParameter(int clan_id, SubmitDeckType type, int[] card_id_array, int[] phantomCardIdList = null)
	{
		GenerateDeckCodeTaskParam generateDeckCodeTaskParam = new GenerateDeckCodeTaskParam();
		generateDeckCodeTaskParam.clan = clan_id;
		generateDeckCodeTaskParam.deck_format = (int)type;
		generateDeckCodeTaskParam.cardID = card_id_array;
		generateDeckCodeTaskParam.phantomCardID = phantomCardIdList;
		base.Params = generateDeckCodeTaskParam;
	}

	public void SetParameterMyRotation(DeckData deck, SubmitDeckType type)
	{
		GenerateDeckCodeTaskMyRotation generateDeckCodeTaskMyRotation = new GenerateDeckCodeTaskMyRotation();
		generateDeckCodeTaskMyRotation.clan = deck.GetDeckClassID();
		generateDeckCodeTaskMyRotation.deck_format = (int)type;
		generateDeckCodeTaskMyRotation.cardID = deck.GetCardIdList().ToArray();
		generateDeckCodeTaskMyRotation.rotation_id = deck.MyRotationId;
		base.Params = generateDeckCodeTaskMyRotation;
	}

	public void SetParameter(int clan_id, int sub_clan, SubmitDeckType type, int[] card_id_array, int[] phantomCardIdList = null)
	{
		GenerateDeckCodeTaskUseSubClassParam generateDeckCodeTaskUseSubClassParam = new GenerateDeckCodeTaskUseSubClassParam();
		generateDeckCodeTaskUseSubClassParam.clan = clan_id;
		generateDeckCodeTaskUseSubClassParam.sub_clan = sub_clan;
		generateDeckCodeTaskUseSubClassParam.deck_format = (int)type;
		generateDeckCodeTaskUseSubClassParam.cardID = card_id_array;
		generateDeckCodeTaskUseSubClassParam.phantomCardID = phantomCardIdList;
		base.Params = generateDeckCodeTaskUseSubClassParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		string deck_code = (string)base.ResponseData["data"]["deck_code"];
		Data.GenerateDeckCode.deck_code = deck_code;
		return num;
	}
}
