namespace Wizard;

public class AIEmoteDataAsset
{
	public int ID;

	public int Category;

	public int FaceID;

	public int MotionID;

	public string VoiceID;

	public string TextID;

	public AIEmoteDataAsset(string[] columns)
	{
		int num = 0;
		ID = AIScriptParser.ParseInt(columns[num++]);
		Category = AIScriptParser.ParseInt(columns[num++]);
		FaceID = AIScriptParser.ParseInt(columns[num++]);
		MotionID = AIScriptParser.ParseInt(columns[num++]);
		VoiceID = columns[num++];
		TextID = ConvEmoteMasterText(columns[num++]);
	}

	private string ConvEmoteMasterText(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return "";
		}
		return Data.Master.GetEmoteWordText(id);
	}
}
