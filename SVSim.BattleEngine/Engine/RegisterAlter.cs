using System.Collections.Generic;
using System.Linq;

public class RegisterAlter : RegisterActionBase
{
	public enum ChangeType
	{
		add,
		half
	}

	protected List<BattleCardBase> _cardList;

	protected SkillBase _skill;

	protected string _attachMsg = string.Empty;

	public RegisterAlter(List<BattleCardBase> cardList, SkillBase skill)
	{
		_cardList = cardList;
		_skill = skill;
		EffectSkillPublicCount = NetworkBattleGenericTool.GetPublishSkillCount(skill);
		if (_cardList != null && _skill != null)
		{
			_attachMsg = NetworkBattleGenericTool.MakeLogCode(_skill);
		}
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.alter.ToString();
	}

	protected virtual Dictionary<string, object> MakeAttachTarget(Dictionary<string, object> dataList)
	{
		if (EffectSkillPublicCount == -1)
		{
			dataList.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.attachTarget], _attachMsg);
		}
		else
		{
			dataList.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.attachTarget], EffectSkillPublicCount.ToString());
		}
		return dataList;
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		if (base.IndexList.Count == 0)
		{
			return false;
		}
		if (_skill != null && !_skill.IsOnceCallTiming)
		{
			return false;
		}
		if (base.IndexList.All((int x) => lot.IndexList.Contains(x)) && IsSelf == lot.IsSelf)
		{
			return _skill == lot.Skill;
		}
		return false;
	}
}
