using LitJson;

namespace Wizard;

public class LeaderSkinUpdateTask : BaseTask
{
	public class LeaderSkinUpdateTaskParam : BaseParam
	{
		public int class_id;

		public int leader_skin_id;

		public bool is_random_leader_skin;

		public int[] leader_skin_id_list;
	}

	private LeaderSkinUpdateTaskParam _param;

	public LeaderSkinUpdateTask()
	{
		base.type = ApiType.Type.LeaderSkinUpdate;
		_param = null;
	}

	public void SetParameter(int class_id, int leader_skin_id)
	{
		_param = new LeaderSkinUpdateTaskParam();
		_param.class_id = class_id;
		_param.leader_skin_id = leader_skin_id;
		_param.is_random_leader_skin = false;
		base.Params = _param;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		ClassCharaPrm classPrm = dataMgr.GetClassPrm(_param.class_id);
		JsonData jsonData = base.ResponseData["data"];
		bool flag = jsonData["is_random_leader_skin"].ToBoolean();
		int num2 = 0;
		num2 = ((!flag) ? _param.leader_skin_id : jsonData["leader_skin_id"].ToInt());
		classPrm.SetLeaderRandomSkinIdList(jsonData["leader_skin_id_list"]);
		classPrm.IsRandomLeaderSkin = flag;
		ClassCharacterMasterData charaPrmBySkinId = dataMgr.GetCharaPrmBySkinId(num2);
		dataMgr.GetClassPrm(_param.class_id).SetCurrentCharaId(charaPrmBySkinId.chara_id);
		return num;
	}
}
