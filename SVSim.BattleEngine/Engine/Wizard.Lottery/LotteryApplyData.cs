using LitJson;

namespace Wizard.Lottery;

public class LotteryApplyData
{

	public bool IsEnable { get; private set; }

	public string DialogTitle { get; private set; }

	public string DialogMessage { get; private set; }

	public string BannerFileName { get; private set; }

	private static string ParseString(string text)
	{
		return text.Replace("\\n", "\n");
	}

	public static LotteryApplyData EmptyData()
	{
		return new LotteryApplyData
		{
			IsEnable = false,
			DialogTitle = string.Empty,
			DialogMessage = string.Empty
		};
	}

	public static LotteryApplyData Parse(JsonData json)
	{
		if (!json.Keys.Contains("application_data") || json["application_data"] == null)
		{
			return EmptyData();
		}
		LotteryApplyData lotteryApplyData = new LotteryApplyData();
		JsonData jsonData = json["application_data"];
		lotteryApplyData.IsEnable = true;
		lotteryApplyData.DialogTitle = jsonData["dialog_message"].ToString();
		lotteryApplyData.DialogMessage = ParseString(jsonData["description_message"].ToString());
		lotteryApplyData.BannerFileName = jsonData["apply_banner_image"].ToString();
		if (jsonData.Keys.Contains("double_chance_message"))
		{
			string text = jsonData["double_chance_message"].ToString();
			if (!string.IsNullOrEmpty(text))
			{
				lotteryApplyData.DialogMessage = lotteryApplyData.DialogMessage + "\n\n" + ParseString(text);
			}
		}
		return lotteryApplyData;
	}
}
