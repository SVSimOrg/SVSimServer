using System;
using System.Collections;
using Cute;
using LitJson;
using UnityEngine;

namespace Wizard;

public class MasterResetMonthTask : BaseTask
{

	public MasterResetMonthTask()
	{
		base.type = ApiType.Type.MasterResetMonth;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		for (int i = 0; i < 2; i++)
		{
			JsonData jsonData = base.ResponseData["data"][Data.FormatConvertApi((Format)i).ToString()];
			UserRank userRank = Data.Load.data._userRank[i];
			userRank.rank = jsonData["rank"].ToInt();
			userRank.master_point = jsonData["master_point"].ToInt();
			userRank.grandMasterData.targetMasterPoint = jsonData["target_grand_master_point"].ToInt();
			userRank.grandMasterData.currentMasterPoint = jsonData["current_grand_master_point"].ToInt();
			UserRank.IsGrandMasterAvailability = true;
			userRank.user_promotion_match.is_promotion = jsonData["is_promotion"].ToInt() != 0;
			if (userRank.rank == 25)
			{
				userRank.is_grand_master_rank = false;
			}
		}
		Data.User.ConnectTimeForMasterReset = ConvertTime.UnixTimeToDateTime(base.ResponseData["data_headers"]["servertime"].ToInt());
		Data.User.ConnectSinceStartUp = Time.realtimeSinceStartup;
		return num;
	}

	public static IEnumerator MasterReset()
	{
		if (PlayerStaticData.IsMasterRank(Format.Rotation) || PlayerStaticData.IsMasterRank(Format.Unlimited))
		{
			DateTime dateTime = Data.User.ConnectTimeForMasterReset.AddSeconds(Time.realtimeSinceStartup - Data.User.ConnectSinceStartUp);
			if (Data.User.ConnectTimeForMasterReset < Data.Load.data._masterResetNextTime && dateTime >= Data.Load.data._masterResetNextTime)
			{
				MasterResetMonthTask task = new MasterResetMonthTask();
				yield return UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(task));
			}
		}
	}
}
