using LitJson;

namespace Wizard;

public class QuestTweetTask : BaseTask
{
	public string Message { get; private set; }

	public string ImagePath { get; private set; }

	public QuestTweetTask()
	{
		base.type = ApiType.Type.QuestTweet;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		string format = jsonData["tweet_message"].ToString().Replace("\\n", "\n");
		ImagePath = jsonData["tweet_image"].ToString();
		string arg = jsonData["tweet_url"].ToString();
		Message = string.Format(format, arg);
		return num;
	}
}
