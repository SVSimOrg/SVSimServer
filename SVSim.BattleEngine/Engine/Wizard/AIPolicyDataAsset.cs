namespace Wizard;

public class AIPolicyDataAsset
{
	public int ID { get; set; }

	public string Category { get; set; }

	public int Priority { get; set; }

	public string Type { get; set; }

	public string Arg { get; set; }

	public string Cond { get; set; }

	public AIPolicyDataAsset()
	{
		ID = -1;
		Category = "";
		Priority = 0;
		Type = "";
		Arg = "";
		Cond = "";
	}

	public AIPolicyDataAsset(string[] columns)
	{
		int num = 0;
		ID = AIScriptParser.ParseInt(columns[num++]);
		Category = columns[num++];
		Priority = AIScriptParser.ParseInt(columns[num++]);
		Type = columns[num++];
		Arg = columns[num++];
		Cond = columns[num++];
	}
}
