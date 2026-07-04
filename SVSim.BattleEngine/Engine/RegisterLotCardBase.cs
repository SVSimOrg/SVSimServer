using System.Collections.Generic;

public class RegisterLotCardBase : RegisterTargetBase
{
	private class TargetPlaceData
	{
		public int GroupIndex { get; private set; }

		public NetworkBattleDefine.NetworkCardPlaceState PlaceState { get; private set; }

		public TargetPlaceData(int index, NetworkBattleDefine.NetworkCardPlaceState state)
		{
			GroupIndex = index;
			PlaceState = state;
		}
	}

	private List<List<double>> _randomList;

	public static double RAND_MAX = 0.999;

	private List<TargetPlaceData> TargetPlaceStateList;

	protected BattleCardBase LotSkillCard;

	private int nowGroupIndex;

	private int _setGroupCount;

	public RegisterLotCardBase(RegisterActionManager registerActionManager, BattleManagerBase mgr, double rand, bool isplayer, NetworkBattleDefine.NetworkCardPlaceState from, int targetIndex, SkillBase skill)
		: base(skill, registerActionManager, isplayer, mgr)
	{
		FromPlaceState = from;
		_randomList = new List<List<double>>();
		List<double> item = new List<double> { rand };
		_randomList.Add(item);
		TargetPlaceStateList = new List<TargetPlaceData>();
		base.IndexList = new List<int>();
		NotNeedIndex = true;
		AddTargetIndexAndPlaceGroup(targetIndex);
		LotSkillCard = skill.SkillPrm.ownerCard;
		nowGroupIndex = 0;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		if (_randomList.Count >= 1)
		{
			List<object> list = new List<object>();
			foreach (List<double> random in _randomList)
			{
				list.Add(random);
			}
			dictionary.Add(ActionBaseParameter.rand.ToString(), list);
		}
		return dictionary;
	}

	public void AddRandomList(double randomVal)
	{
		if (nowGroupIndex >= _randomList.Count)
		{
			List<double> list = new List<double>();
			list.Add(randomVal);
			_randomList.Add(list);
		}
		else
		{
			_randomList[nowGroupIndex].Add(randomVal);
		}
	}

	public void AddTargetIndexAndPlaceGroup(int targetIndex)
	{
		base.IndexList.Add(targetIndex);
		NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(BattleMgr.GetBattlePlayer(IsSelf), targetIndex);
		if (TargetPlaceStateList.Count >= 1 && (TargetPlaceStateList[TargetPlaceStateList.Count - 1].PlaceState != cardPlaceState || base.Skill.ApplyAndFilter.Count > 0))
		{
			AddGroupIndex();
			nowGroupIndex++;
		}
		TargetPlaceStateList.Add(new TargetPlaceData(nowGroupIndex, cardPlaceState));
	}

	public override void SettingGroupIndexMsg(RegisterActionBase registerBase)
	{
		if (registerBase is RegisterStateChangeCard)
		{
			NetworkBattleDefine.NetworkCardPlaceState toPlaceState = (registerBase as RegisterStateChangeCard).ToPlaceState;
			for (int i = 0; i < TargetPlaceStateList.Count; i++)
			{
				if (toPlaceState == TargetPlaceStateList[i].PlaceState && (base.Skill.ApplyAndFilter.Count <= 0 || _setGroupCount <= i))
				{
					_setGroupCount++;
					registerBase.PrivateGroupIndexMsg = GroupMsgList[TargetPlaceStateList[i].GroupIndex];
					return;
				}
			}
		}
		if (!(registerBase is RegisterExtract))
		{
			base.SettingGroupIndexMsg(registerBase);
		}
	}

	public bool IsAlreadyAddedIndex(int index, NetworkBattleDefine.NetworkCardPlaceState fromPlace)
	{
		if (base.IndexList.Exists((int x) => x == index))
		{
			return FromPlaceState == fromPlace;
		}
		return false;
	}
}
