using UnityEngine;

public class NetworkTouchControl : TouchControl
{

	public bool notAttackFlag { private get; set; }

	public bool notEmoteFlag { private get; set; }

	public bool notDragPlayCardFlag { private get; set; }

	public bool notEvolCardFlag { private get; set; }

	public NetworkTouchControl(BattleManagerBase battleMgr, BattleCamera battleCamera, BackGroundBase backGround)
		: base(battleMgr, battleCamera, backGround)
	{
		notAttackFlag = false;
		notEmoteFlag = false;
		notDragPlayCardFlag = false;
		notEvolCardFlag = false;
	}

	public void SetDisableTouch()
	{
		notAttackFlag = true;
		notEmoteFlag = true;
		notDragPlayCardFlag = true;
		notEvolCardFlag = true;
	}

	protected override bool IsFeasibleAttack()
	{
		if (notAttackFlag)
		{
			return false;
		}
		return base.IsFeasibleAttack();
	}

	protected override bool IsFeasibleEmote()
	{
		if (notEmoteFlag)
		{
			return false;
		}
		return base.IsFeasibleEmote();
	}

	protected override bool IsFeasiblePlayCard()
	{
		if (notDragPlayCardFlag)
		{
			return false;
		}
		return base.IsFeasiblePlayCard();
	}

	protected override bool IsFeasibleEvol()
	{
		if (notEvolCardFlag)
		{
			return false;
		}
		return base.IsFeasibleEvol();
	}
}
