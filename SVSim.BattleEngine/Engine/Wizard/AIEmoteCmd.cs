namespace Wizard;

public class AIEmoteCmd
{
	public ClassCharaPrm.EmotionType emoteType { get; set; }

	public int ID { get; set; }

	public int CategoryKey { get; set; }

	public int faceID { get; set; }

	public int motionID { get; set; }

	public string voiceID { get; set; }

	public string textID { get; set; }

	public bool isAI { get; set; }

	public AIEmoteCmd()
	{
		emoteType = ClassCharaPrm.EmotionType.GREET;
		ID = 0;
		CategoryKey = 0;
		faceID = 0;
		motionID = 0;
		voiceID = "";
		textID = "";
		isAI = true;
	}

	public AIEmoteCmd(AIEmoteDataAsset asset)
	{
		ID = asset.ID;
		CategoryKey = asset.Category;
		faceID = asset.FaceID;
		motionID = asset.MotionID;
		voiceID = asset.VoiceID;
		textID = asset.TextID;
		isAI = true;
	}

	public AIEmoteCmd(ClassCharaPrm.EmotionType _emoteType)
	{
		emoteType = _emoteType;
		ID = 0;
		CategoryKey = 0;
		faceID = 0;
		motionID = 0;
		voiceID = "";
		textID = "";
		isAI = true;
	}
}
