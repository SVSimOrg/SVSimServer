using System.Collections.Generic;
using System.Linq;

public class RegisterStateChangeCard : RegisterActionBase
{
	public List<string> SkillTargetList = new List<string>();

	public BattleCardBase StateCard { get; private set; }

	public NetworkBattleDefine.NetworkCardPlaceState FromPlaceState { get; private set; }

	public NetworkBattleDefine.NetworkCardPlaceState ToPlaceState { get; private set; }

	public List<int> OpenCardIndexList { get; private set; }

	public bool IsOpen { get; private set; }

	public bool IsWhenDraw { get; private set; }

	public bool IsFlood { get; private set; }

	public SkillBase Skill { get; private set; }

	public List<int> HasGuardList { get; private set; }

	public RegisterStateChangeCard(BattleCardBase Card, NetworkBattleDefine.NetworkCardPlaceState from, NetworkBattleDefine.NetworkCardPlaceState to, SkillBase skill, bool isOpen = false, bool isFlood = false, bool isWhenDraw = false, bool hasGuard = false)
	{
		StateCard = Card;
		FromPlaceState = from;
		ToPlaceState = to;
		Skill = skill;
		IsOpen = isOpen;
		IsFlood = isFlood;
		base.IndexList = new List<int>();
		OpenCardIndexList = new List<int>();
		HasGuardList = new List<int>();
		IsWhenDraw = isWhenDraw;
		if (StateCard != null)
		{
			base.IndexList.Add(StateCard.Index);
			IsSelf = StateCard.IsPlayer;
			if (hasGuard && !IsFlood && FromPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Field && ToPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Cemetery)
			{
				HasGuardList.Add(StateCard.Index);
			}
		}
		if (IsOpen)
		{
			OpenCardIndexList.Add(0);
		}
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.from], (int)FromPlaceState);
		dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.to], (int)ToPlaceState);
		if (OpenCardIndexList.Count > 0)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.is_open], OpenCardIndexList);
		}
		if (SkillTargetList.Count > 0)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.skillTarget], SkillTargetList);
		}
		if (IsFlood)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.isFlood], 1);
		}
		if (HasGuardList.Count > 0)
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.hasGuard], HasGuardList);
		}
		if (FromPlaceState == NetworkBattleDefine.NetworkCardPlaceState.None && ToPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Deck && Skill is Skill_update_deck && Skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.player_side, SkillFilterCreator.ContentKeyword.me.ToStringCustom()) == "op")
		{
			dictionary.Add(NetworkBattleDefine.NetworkParameterNames[NetworkBattleDefine.NetworkParameter.byOppo], 1);
		}
		return dictionary;
	}

	public void SetOpenIfWhenDraw()
	{
		if (!IsOpen && IsWhenDraw && ((StateCard.FinalMetamorphoseCard == null) ? StateCard : StateCard.FinalMetamorphoseCard).Skills.Any((SkillBase s) => s.OnWhenDraw != 0 && s.IsScanConditionOk))
		{
			IsOpen = true;
			OpenCardIndexList.Add(0);
		}
	}

	public void AddOpenCardIndex(int index)
	{
		if (!OpenCardIndexList.Contains(index))
		{
			OpenCardIndexList.Add(index);
		}
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.move.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		if (base.IndexList.Count == 0)
		{
			return false;
		}
		if (base.IndexList.All((int x) => lot.IndexList.Contains(x)) && IsSelf == lot.IsSelf)
		{
			return Skill == lot.Skill;
		}
		return false;
	}
}
