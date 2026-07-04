using LitJson;

namespace Wizard;

public class CheckSpecialTitleTask : BaseTask
{
	public CheckSpecialTitleTask()
	{
		base.type = ApiType.Type.CheckSpecialTitle;
	}

	public int ParseTitleCheckData()
	{
		int num = Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		string text = "0";
		dataMgr.TitleId = "0";
		if (jsonData.Keys.Contains("title_image_id"))
		{
			text = jsonData["title_image_id"].ToString();
		}
		if (text != "0" && text != "1")
		{
			if (SpecialTitleAssetBundle.IsAvailableTitleAssetBundle(text))
			{
				dataMgr.TitleId = text;
			}
		}
		else
		{
			dataMgr.TitleId = text;
		}
		return num;
	}
}
