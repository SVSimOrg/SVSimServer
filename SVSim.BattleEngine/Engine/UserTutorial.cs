using LitJson;
using Wizard;

public class UserTutorial : HeaderData
{

	public int tutorial_step;

	public string create_time;

	public int TutorialStep
	{
		get
		{
			if ((tutorial_step == 31 || tutorial_step == 41) && Data.MaintenanceCodeList != null)
			{
				bool num = Data.MaintenanceCodeList.Contains(NetworkDefine.MAINTENANCE_TYPE.GIFT_MAINTENANCE);
				bool flag = Data.MaintenanceCodeList.Contains(NetworkDefine.MAINTENANCE_TYPE.SHOP_CARDPACK_MAINTENANCE);
				if (num || flag)
				{
					tutorial_step = 100;
				}
			}
			return tutorial_step;
		}
		set
		{
			tutorial_step = value;
		}
	}

	public void Update(JsonData data)
	{
		if (data.Keys.Contains("tutorial_step"))
		{
			tutorial_step = data["tutorial_step"].ToInt();
		}
	}
}
