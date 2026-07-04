using System.Collections.Generic;

public class RegisterEnhanceTrigger : RegisterActionBase
{
	public enum OpponentEnhanceTriggerParameter
	{
		targetIdx,
		revenge,
		resonance,
		avarice,
		returnCard,
		option,
		turnEndRestore,
		turnStartRestore,
		turnDamageFromUnit,
		useEp,
		maxAtk,
		canEvolve,
		isUnlimited
	}

	private List<BattleCardBase> TurnEndStartHandList;

	private int _revenge;

	private bool _revengeFlag;

	private int _resonance;

	private bool _resonanceFlag;

	private int _avarice;

	private bool _avariceFlag;

	private bool _isTurnEndRestore;

	private bool _isTurnStartRestore;

	private bool _isTurnDamageFromUnit;

	private int _turnDamageFromUnit;

	private bool _isUseEp;

	private int _returnCard;

	private bool _returnCardFlag;

	private int _maxAtk;

	private bool _maxAtkFlag;

	private bool _canEvolve;

	private bool _isUnlimited;

	public RegisterEnhanceTrigger(BattlePlayerBase player)
	{
		IsSelf = player.IsPlayer;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		if (_revengeFlag)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.revenge.ToString(), _revenge);
		}
		if (_avariceFlag)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.avarice.ToString(), _avarice);
		}
		if (_resonanceFlag)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.resonance.ToString(), _resonance);
		}
		if (_returnCardFlag)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.returnCard.ToString(), _returnCard);
		}
		if (_isTurnEndRestore)
		{
			List<int> list = new List<int>();
			foreach (BattleCardBase turnEndStartHand in TurnEndStartHandList)
			{
				list.Add(turnEndStartHand.Index);
			}
			dictionary.Add(OpponentEnhanceTriggerParameter.turnEndRestore.ToString(), list);
		}
		if (_isTurnStartRestore)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.turnStartRestore.ToString(), 1);
		}
		if (_isTurnDamageFromUnit)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.turnDamageFromUnit.ToString(), _turnDamageFromUnit);
		}
		if (_isUseEp)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.useEp.ToString(), 1);
		}
		if (_maxAtkFlag)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.maxAtk.ToString(), _maxAtk);
		}
		if (_canEvolve)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.canEvolve.ToString(), 1);
		}
		if (_isUnlimited)
		{
			dictionary.Add(OpponentEnhanceTriggerParameter.isUnlimited.ToString(), 1);
		}
		return dictionary;
	}

	public void SettingRevenge(int revengeNum)
	{
		_revenge = revengeNum;
		_revengeFlag = true;
	}

	public void SettingResonance(int resonanceNum)
	{
		_resonance = resonanceNum;
		_resonanceFlag = true;
	}

	public void SettingAvarice(int avariceNum)
	{
		_avarice = avariceNum;
		_avariceFlag = true;
	}

	public void SettingReturnCard(int returnCardNum)
	{
		_returnCard = returnCardNum;
		_returnCardFlag = true;
	}

	public void SettingTurnEndRestore(List<BattleCardBase> cards)
	{
		_isTurnEndRestore = true;
		TurnEndStartHandList = cards;
	}

	public void SettingTurnStartRestore()
	{
		_isTurnStartRestore = true;
	}

	public void SettingBeforeTurnDamageFromUnit(int turnCausedDamage)
	{
		_isTurnDamageFromUnit = true;
		_turnDamageFromUnit = turnCausedDamage;
	}

	public void SettingUseEp()
	{
		_isUseEp = true;
	}

	public void SettingMaxAtk(int maxAtk)
	{
		_maxAtk = maxAtk;
		_maxAtkFlag = true;
	}

	public void SettingCanEvolve()
	{
		_canEvolve = true;
	}

	public void SettingIsUnlimited()
	{
		_isUnlimited = true;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.trigger.ToString();
	}
}
