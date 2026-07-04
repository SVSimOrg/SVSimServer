using Wizard;

public class ConventionDeckDeleteTask : BaseTask
{
	public class ConventionDeckDeleteTaskParam : BaseParam
	{
		public string tournament_id;

		public int[] deck_no_list;

		public int deck_format;
	}

	public ConventionDeckDeleteTask()
	{
		base.type = ApiType.Type.ConventionDeckDelete;
	}

	public void SetParameter(string tournament_id, int[] deck_no, Format format)
	{
		ConventionDeckDeleteTaskParam conventionDeckDeleteTaskParam = new ConventionDeckDeleteTaskParam();
		conventionDeckDeleteTaskParam.tournament_id = tournament_id;
		conventionDeckDeleteTaskParam.deck_no_list = deck_no;
		conventionDeckDeleteTaskParam.deck_format = Data.FormatConvertApi(format);
		base.Params = conventionDeckDeleteTaskParam;
	}
}
