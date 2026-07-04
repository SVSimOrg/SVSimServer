using System.Collections.Generic;
using LitJson;
using Wizard;

public class DoMatchingBase : BaseTask
{
	public class DoMatchingTaskParam : BaseParam
	{
		public int deck_no;

		public int need_init;

		public int log;

		public List<int> excluded_field_id_list;

		public int use_stage_select;

		public int is_default_skin;

		public DoMatchingTaskParam(int deckNo, int needInit, int logdType)
		{
			deck_no = deckNo;
			need_init = needInit;
			log = logdType;
			excluded_field_id_list = PlayerPrefsWrapper.CreateServerSendStageOffList();
			use_stage_select = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.USE_OFF_STAGE);
			is_default_skin = (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_OPPONENT_DEFAULT_SKIN) ? 1 : 0);
		}
	}

	public class DoMatchingTaskIncludeCardMasterHashParam : DoMatchingTaskParam
	{
		public string card_master_hash;

		public DoMatchingTaskIncludeCardMasterHashParam(int deckNo, int needInit, int logdType)
			: base(deckNo, needInit, logdType)
		{
			card_master_hash = CardMasterLocalFileUtility.GetCardMasterHash();
		}
	}

	public DoMatchingBase()
	{
		SkipCuteTimeOutPopup();
		SkipCuteHttpStatusErrorPopup();
		SkipAllCuteResultCodeCheckErrorPopup();
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		if (base.ResponseData["data"].Keys.Contains("feature_maintenance_list"))
		{
			List<NetworkDefine.MAINTENANCE_TYPE> list = new List<NetworkDefine.MAINTENANCE_TYPE>();
			for (int i = 0; i < base.ResponseData["data"]["feature_maintenance_list"].Count; i++)
			{
				list.Add((NetworkDefine.MAINTENANCE_TYPE)base.ResponseData["data"]["feature_maintenance_list"][i].ToInt());
			}
			Data.UpdateMaintenance(new List<NetworkDefine.MAINTENANCE_TYPE>
			{
				NetworkDefine.MAINTENANCE_TYPE.REPLAY_ALL,
				NetworkDefine.MAINTENANCE_TYPE.NEWREPLAY_ALL,
				NetworkDefine.MAINTENANCE_TYPE.NEWREPLAY_EXCLUDE_ROTATION,
				NetworkDefine.MAINTENANCE_TYPE.NEWREPLAY_RECORD
			}, list);
		}
		SettingCardMasterId(base.ResponseData["data"]);
		return num;
	}

	private void SettingCardMasterId(JsonData jsonData)
	{
		int num = jsonData["matching_state"].ToInt();
		if (num == 3004 || num == 3007 || num == 3011)
		{
			CardMaster.SetBattleCardMasterId(jsonData["card_master_id"].ToInt());
		}
	}
}
