using System.Collections.Generic;
using System.Linq;

public class RegisterActionBase
{
	public enum CARD_TYPE
	{
		NON,
		FOLLOWER,
		NOT_COUNT_AMULET,
		COUNT_AMULET,
		SPELL
	}

	public enum SPECIAL_LIBRARY
	{
		NONE = 0,
		EARTH_RITUAL = 1001,
		EARTH_MARK = 1002,
		SPELL_CHARGE_ALL = 1003,
		WHEN_DESTROY = 1004,
		NOT_WHEN_DESTROY = 1104,
		UNION_BURST = 1005,
		WHEN_PLAY = 1006,
		WHEN_ACCELERATE = 1007,
		WHEN_CRYSTALLIZE = 1008,
		NOT_WHEN_PLAY = 1106,
		NOT_WHEN_ACCELERATE = 1107,
		NOT_WHEN_CRYSTALLIZE = 1108,
		ENHANCE = 1009,
		SUPER_SKYBOUND_ART = 1010,
		REANIMATE = 1011,
		WHEN_EVOLVE = 1012,
		NECROMANCE = 1013,
		FUSION = 1014,
		SPELL_CHARGE_SPELL = 2001,
		SPELL_NOT_SPELL_CHARGE = 2002,
		NOT_WHEN_ACCELERATE_AND_WHEN_CRYSTALLIZE = 2003
	}

	public enum ActionBaseParameter
	{
		idx,
		isSelf,
		skillIdx,
		type,
		target,
		clan,
		tribe,
		baseCardId,
		state,
		condition,
		excludeList,
		rand,
		libraryType,
		charType,
		duplication,
		baseCost,
		nowCost,
		inHandCost,
		atk,
		life,
		baseAtk,
		hasBuffLife,
		group,
		conditions,
		includeList,
		baseCardIdx,
		handIdxList,
		card,
		cardId,
		baseIdx,
		isPremium,
		candidates,
		cost,
		add,
		del,
		attachTarget,
		repeat,
		isBase,
		var,
		param,
		skillCount,
		other,
		damage,
		heal,
		addPP,
		set,
		isBoard,
		isChoice,
		expect,
		ingredients,
		isInvoke,
		cemetery,
		buffUnit,
		maxPP,
		stack,
		isForce,
		isPreprocess,
		isIncludeSelf,
		isExcludePlayIdx,
		discarded,
		forHandResident
	}

	public string PrivateGroupIndexMsg = "";

	public bool NotNeedIndex;

	public bool IsSelf;

	public int EffectSkillPublicCount;

	public int EffectSkillMovement;

	public List<int> IndexList { get; protected set; }

	public RegisterActionBase()
	{
		IndexList = new List<int>();
	}

	public virtual Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (!NotNeedIndex)
		{
			if (PrivateGroupIndexMsg != "" && !(this is RegisterCopyToken))
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idx], PrivateGroupIndexMsg);
			}
			else if (IndexList != null && IndexList.Count >= 1)
			{
				dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idx], IndexList);
			}
		}
		dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isSelf], IsSelf ? 1 : 0);
		return dictionary;
	}

	public virtual string GetUriMsg()
	{
		return "";
	}

	public void SetIndexList(List<int> indexList)
	{
		IndexList = indexList;
	}

	public virtual bool IsUseLotCard(RegisterLotCardBase lot)
	{
		if (IndexList.Count == 0)
		{
			return false;
		}
		if (IndexList.All((int x) => lot.IndexList.Contains(x)))
		{
			return IsSelf == lot.IsSelf;
		}
		return false;
	}
}
