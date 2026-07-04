using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class MailReadTask : BaseTask
{
	public class MailReadTaskParam : BaseParam
	{
		public string[] present_id_array;

		public int state;
	}

	public MailReadTask(int state)
	{
		base.type = ApiType.Type.MailRead;
	}

	public void SetParameter(string[] present_id_array, int state, bool isTutorial)
	{
		base.type = (isTutorial ? ApiType.Type.MailReadTutorial : ApiType.Type.MailRead);
		MailReadTaskParam mailReadTaskParam = new MailReadTaskParam();
		mailReadTaskParam.present_id_array = present_id_array;
		mailReadTaskParam.state = state;
		base.Params = mailReadTaskParam;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		Data.ReadMail.data = new ReadMailDetail();
		Data.ReadMail.data.total_recieve_count_list = new List<ReceivedReward>();
		/* Pre-Phase-5b: MailTopTask.SetToFirstPage dropped */
		Data.MailTop.data = new MailDataDetail();
		Data.MailTop.data.mail_data_list = new List<MailData>();
		Data.MailTop.data.mail_history_list = new List<MailData>();
		JsonData jsonData = base.ResponseData["data"]["total_receive_count_list"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			ReceivedReward item = new ReceivedReward(jsonData[i]);
			Data.ReadMail.data.total_recieve_count_list.Add(item);
		}
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		Data.ReadMail.data.is_unreceived_present = base.ResponseData["data"]["is_unreceived_present"].ToBoolean();
		JsonData jsonData2 = base.ResponseData["data"]["present_list"];
		for (int j = 0; j < jsonData2.Count; j++)
		{
			MailData item2 = new MailData(jsonData2[j]);
			Data.MailTop.data.mail_data_list.Add(item2);
		}
		JsonData jsonData3 = base.ResponseData["data"]["present_history_list"];
		for (int k = 0; k < jsonData3.Count; k++)
		{
			MailData item3 = new MailData(jsonData3[k]);
			Data.MailTop.data.mail_history_list.Add(item3);
		}
		Data.Load.data._userTutorial.Update(base.ResponseData["data"]);
		return num;
	}
}
