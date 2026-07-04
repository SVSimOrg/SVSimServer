using System.Collections.Generic;

public class RegisterCopyToken : RegisterToken
{
	public int CopyCardIndex { get; private set; }

	public bool IsPremium { get; private set; }

	public RegisterCopyToken(BattleCardBase card, bool isChoice, NetworkBattleDefine.NetworkCardPlaceState toPlaceState, int copyCardIndex, SkillBase skill)
		: base(card, isChoice, toPlaceState, skill)
	{
		CopyCardIndex = copyCardIndex;
		IsPremium = card.BaseParameter.IsFoil;
	}

	protected override Dictionary<string, object> MakeCardData()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (PrivateGroupIndexMsg != "")
		{
			dictionary.Add(ActionBaseParameter.baseIdx.ToString(), PrivateGroupIndexMsg);
		}
		else
		{
			dictionary.Add(ActionBaseParameter.baseIdx.ToString(), CopyCardIndex);
		}
		dictionary.Add(ActionBaseParameter.isPremium.ToString(), IsPremium ? 1 : 0);
		return dictionary;
	}

	public override bool IsMatchData(RegisterToken baseToken, RegisterToken checkToken)
	{
		if (base.IsMatchData(baseToken, checkToken) && baseToken is RegisterCopyToken && checkToken is RegisterCopyToken)
		{
			RegisterCopyToken registerCopyToken = baseToken as RegisterCopyToken;
			RegisterCopyToken registerCopyToken2 = checkToken as RegisterCopyToken;
			if (registerCopyToken.CopyCardIndex == registerCopyToken2.CopyCardIndex && registerCopyToken.IsPremium == registerCopyToken2.IsPremium)
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		if (lot.IndexList.Contains(CopyCardIndex) && IsSelf == lot.IsSelf)
		{
			return base.Skill == lot.Skill;
		}
		return false;
	}
}
