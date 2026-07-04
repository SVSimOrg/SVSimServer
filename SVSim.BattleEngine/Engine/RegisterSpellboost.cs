using System.Collections.Generic;
using System.Linq;

public class RegisterSpellboost : RegisterAlter
{
	private int _addValue;

	private int _deffSet;

	private readonly string SpellboostParameter = "spellboost";

	public List<int> SpellBoostIndexList { get; private set; }

	public List<string> SpellBoostGroupIndexList { get; private set; }

	public RegisterSpellboost(List<BattleCardBase> cardList, SkillBase skill, int addCount, int deffSet)
		: base(cardList, skill)
	{
		base.IndexList = new List<int>();
		if (cardList != null && cardList.Count > 0)
		{
			cardList.ForEach(delegate(BattleCardBase x)
			{
				if (!base.IndexList.Any((int z) => z == x.Index))
				{
					base.IndexList.Add(x.Index);
				}
			});
		}
		_addValue = addCount;
		_deffSet = deffSet;
		IsSelf = skill.SkillPrm.ownerCard.IsPlayer;
		SpellBoostGroupIndexList = new List<string>();
		SpellBoostIndexList = new List<int>();
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (SpellBoostGroupIndexList.Count > 0)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idx], SpellBoostGroupIndexList[0]);
		}
		else if (base.IndexList.Count > 0)
		{
			List<object> list = new List<object>();
			foreach (int index in base.IndexList)
			{
				list.Add(index);
			}
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.idx], list);
		}
		dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isSelf], IsSelf ? 1 : 0);
		dictionary.Add(ActionBaseParameter.type.ToString(), ActionBaseParameter.add.ToString());
		if (_deffSet == 0)
		{
			dictionary.Add(SpellboostParameter, "a" + _addValue);
		}
		else
		{
			dictionary.Add(SpellboostParameter, "s" + _deffSet);
		}
		return MakeAttachTarget(dictionary);
	}

	public void SettingSpellBoostGrope(string message)
	{
		SpellBoostGroupIndexList.Add(message);
	}
}
