namespace Wizard;

public class Emotion
{
	public readonly int emotion_id;

	public readonly ClassCharaPrm.FaceType face_id;

	public readonly ClassCharaPrm.MotionType motion_id;

	private readonly string voice_id;

	private readonly string evolved_voice_id;

	private readonly string text;

	private readonly string evolved_text;

	public Emotion(string[] columns)
	{
		emotion_id = int.Parse(columns[0]);
		face_id = (string.IsNullOrEmpty(columns[1]) ? ClassCharaPrm.FaceType.skin_01 : ((ClassCharaPrm.FaceType)int.Parse(columns[1])));
		motion_id = (string.IsNullOrEmpty(columns[2]) ? ClassCharaPrm.MotionType.idle : ((ClassCharaPrm.MotionType)int.Parse(columns[2])));
		string[] array = columns[3].Split(':');
		voice_id = array[0];
		evolved_voice_id = ((array.Length > 1) ? array[1] : array[0]);
		string[] array2 = columns[4].Split(':');
		text = ConvEmoteMasterText(array2[0]);
		evolved_text = ((array2.Length > 1) ? ConvEmoteMasterText(array2[1]) : ConvEmoteMasterText(array2[0]));
	}

	private string ConvEmoteMasterText(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return "";
		}
		return Data.Master.GetEmoteWordText(id);
	}

	public string GetText(bool isEvolved = false)
	{
		if (!isEvolved)
		{
			return text;
		}
		return evolved_text;
	}
}
