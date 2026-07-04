public class VoiceAndWaitTime
{
	public static VoiceAndWaitTime _nullVoice = new VoiceAndWaitTime("", 0f);

	public string Voice { get; private set; }

	public float WaitTime { get; private set; }

	public VoiceAndWaitTime(string voice, float waitTime)
	{
		Voice = voice;
		WaitTime = waitTime;
	}
}
