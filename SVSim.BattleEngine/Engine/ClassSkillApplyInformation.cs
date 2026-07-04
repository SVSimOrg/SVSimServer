using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class ClassSkillApplyInformation : SkillApplyInformation
{
	public class LifeInfomation
	{
		public int Life { get; private set; }

		public int MaxLife { get; private set; }

		public int BeforeLife { get; private set; }

		public int BeforeMaxLife { get; private set; }

		public LifeInfomation(int life, int maxLife, int beforeLife, int beforeMaxLife)
		{
			Life = life;
			MaxLife = maxLife;
			BeforeLife = beforeLife;
			BeforeMaxLife = beforeMaxLife;
		}
	}

	public class PpModifyInformation
	{
		public int AddPpValue { get; private set; }

		public PpModifyInformation(int addPpValue)
		{
			AddPpValue = addPpValue;
		}
	}

	public event Action<RegisterActionBase.ActionBaseParameter, LifeInfomation> OnLifeChange;

	public event Action<RegisterActionBase.ActionBaseParameter, PpModifyInformation> OnPpChange;

	public ClassSkillApplyInformation(BattleCardBase card)
		: base(card)
	{
	}

	public override void DamageLife(int damage, int turn, bool isSelfTurn)
	{
		int life = _card.Life;
		int maxLife = _card.MaxLife;
		base.DamageLife(damage, turn, isSelfTurn);
		if (this.OnLifeChange != null)
		{
			this.OnLifeChange(RegisterActionBase.ActionBaseParameter.damage, new LifeInfomation(_card.Life, _card.MaxLife, life, maxLife));
		}
	}

	public override void HealLife(int healAmount, int turn, bool isSelfTurn)
	{
		int life = _card.Life;
		int maxLife = _card.MaxLife;
		base.HealLife(healAmount, turn, isSelfTurn);
		if (this.OnLifeChange != null)
		{
			this.OnLifeChange(RegisterActionBase.ActionBaseParameter.heal, new LifeInfomation(_card.Life, _card.MaxLife, life, maxLife));
		}
	}

	public override void AddPp(int addPp, int currentTurn, bool isSelfTurn)
	{
		base.AddPp(addPp, currentTurn, isSelfTurn);
		if (this.OnPpChange != null)
		{
			this.OnPpChange(RegisterActionBase.ActionBaseParameter.addPP, new PpModifyInformation(addPp));
		}
	}

	public override VfxBase GiveForceBerserk(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		VfxBase vfx = SetForceBerserkCount(base.ForceBerserkCount + 1, skillProcessor);
		sequentialVfxPlayer.Register(((ClassBattleCardBase)_card).GetOnBerserkCheck(base.ForceBerserkCount == 1 && _card.Life > 10));
		sequentialVfxPlayer.Register(vfx);
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return sequentialVfxPlayer;
	}

	public override VfxBase DepriveForceBerserk(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(SetForceBerserkCount(base.ForceBerserkCount - 1, skillProcessor));
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(((ClassBattleCardBase)_card).GetOnBerserkCheck(flg: false), NullVfx.GetInstance(), InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return sequentialVfxPlayer;
	}

	public override VfxBase ForceDepriveForceBerserk(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(SetForceBerserkCount(0, skillProcessor));
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(((ClassBattleCardBase)_card).GetOnBerserkCheck(flg: false), NullVfx.GetInstance(), InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return sequentialVfxPlayer;
	}

	private VfxBase SetForceBerserkCount(int count, SkillProcessor skillProcessor)
	{
		base.ForceBerserkCount = count;
		base.IsForceBerserk = base.ForceBerserkCount > 0;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (skillProcessor != null)
		{
			sequentialVfxPlayer.Register(base.Player.StartSkillWhenChangeClassLife(skillProcessor));
		}
		((ClassBattleCardBase)_card).CallOnForceBerserkChange(base.ForceBerserkCount);
		return sequentialVfxPlayer;
	}

	public override VfxBase GiveForceAvarice(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SetForceAvariceCount(base.ForceAvariceCount + 1);
		sequentialVfxPlayer.Register(((ClassBattleCardBase)_card).GetOnAvariceCheck(base.ForceAvariceCount == 1 && base.Player.TurnDrawCards.Count() > 2));
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return sequentialVfxPlayer;
	}

	public override VfxBase DepriveForceAvarice()
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		SetForceAvariceCount(base.ForceAvariceCount - 1);
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(((ClassBattleCardBase)_card).GetOnAvariceCheck(flag: false), NullVfx.GetInstance(), InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return result;
	}

	public override VfxBase ForceDepriveForceAvarice()
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		SetForceAvariceCount(0);
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(((ClassBattleCardBase)_card).GetOnAvariceCheck(flag: false), NullVfx.GetInstance(), InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return result;
	}

	private void SetForceAvariceCount(int count)
	{
		base.ForceAvariceCount = count;
		base.IsForceAvarice = base.ForceAvariceCount > 0;
		((ClassBattleCardBase)_card).CallOnForceAvariceChange(base.ForceAvariceCount);
	}

	public override VfxBase GiveForceWrath(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SetForceWrathCount(base.ForceWrathCount + 1);
		sequentialVfxPlayer.Register(((ClassBattleCardBase)_card).GetOnWrathCheck(base.ForceWrathCount == 1 && base.Player.SkillInfoClass.DamagedCounter.GetDamageCount(selfTurn: true) > 7));
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return sequentialVfxPlayer;
	}

	public override VfxBase DepriveForceWrath()
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		SetForceWrathCount(base.ForceWrathCount - 1);
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(((ClassBattleCardBase)_card).GetOnWrathCheck(flag: false), NullVfx.GetInstance(), InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return result;
	}

	public override VfxBase ForceDepriveForceWrath()
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		SetForceWrathCount(0);
		base.Player.BattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(((ClassBattleCardBase)_card).GetOnWrathCheck(flag: false), NullVfx.GetInstance(), InstantVfx.Create(delegate
		{
			base.Player.UpdateHandCardsPlayability();
		})));
		return result;
	}

	private void SetForceWrathCount(int count)
	{
		base.ForceWrathCount = count;
		base.IsForceWrath = base.ForceWrathCount > 0;
		((ClassBattleCardBase)_card).CallOnForceWrathChange(base.ForceWrathCount);
	}

	public override VfxBase GiveCantActivateFanfare(string type)
	{
		if (type == "unit")
		{
			base.CantActivateFanfareUnitCount++;
		}
		else if (type == "field")
		{
			base.CantActivateFanfareFieldCount++;
		}
		else
		{
			base.CantActivateFanfareUnitCount++;
			base.CantActivateFanfareFieldCount++;
		}
		base.IsCantActivateFanfareUnit = base.CantActivateFanfareUnitCount > 0;
		base.IsCantActivateFanfareField = base.CantActivateFanfareFieldCount > 0;
		return NullVfx.GetInstance();
	}

	public override VfxBase SetCantActivateFanfareCount(int count)
	{
		base.CantActivateFanfareUnitCount = count;
		base.CantActivateFanfareFieldCount = count;
		base.IsCantActivateFanfareUnit = base.CantActivateFanfareUnitCount > 0;
		base.IsCantActivateFanfareField = base.CantActivateFanfareFieldCount > 0;
		return NullVfx.GetInstance();
	}

	public override VfxBase DepriveCantActivateFanfare(string type)
	{
		if (type == "unit")
		{
			base.CantActivateFanfareUnitCount--;
		}
		else if (type == "field")
		{
			base.CantActivateFanfareFieldCount--;
		}
		else
		{
			base.CantActivateFanfareUnitCount--;
			base.CantActivateFanfareFieldCount--;
		}
		base.IsCantActivateFanfareUnit = base.CantActivateFanfareUnitCount > 0;
		base.IsCantActivateFanfareField = base.CantActivateFanfareFieldCount > 0;
		return NullVfx.GetInstance();
	}

	public override VfxBase ForceDepriveCantActivateFanfare(string type)
	{
		if (type == "unit")
		{
			base.CantActivateFanfareUnitCount = 0;
		}
		else if (type == "field")
		{
			base.CantActivateFanfareFieldCount = 0;
		}
		else
		{
			base.CantActivateFanfareUnitCount = 0;
			base.CantActivateFanfareFieldCount = 0;
		}
		base.IsCantActivateFanfareUnit = base.CantActivateFanfareUnitCount > 0;
		base.IsCantActivateFanfareField = base.CantActivateFanfareFieldCount > 0;
		return NullVfx.GetInstance();
	}

	public override VfxBase GiveCantActivateShortageDeckWin()
	{
		base.CantActivateShortageDeckWinCount++;
		base.IsCantActivateShortageDeckWin = base.CantActivateShortageDeckWinCount > 0;
		return NullVfx.GetInstance();
	}

	public override VfxBase DepriveCantActivateShortageDeckWin()
	{
		base.CantActivateShortageDeckWinCount--;
		base.IsCantActivateShortageDeckWin = base.CantActivateShortageDeckWinCount > 0;
		return NullVfx.GetInstance();
	}

	public override VfxBase ForceDepriveCantActivateShortageDeckWin()
	{
		base.CantActivateShortageDeckWinCount = 0;
		base.IsCantActivateShortageDeckWin = false;
		return NullVfx.GetInstance();
	}

	public override VfxBase GiveRepeatSkill(string repeatTiming, string repeatTarget, SkillBase skill)
	{
		base.RepeatSkillTimingList.Add(new RepeatSkillInfo(repeatTiming, repeatTarget, skill));
		return NullVfx.GetInstance();
	}

	public override VfxBase DepriveRepeatSkill(string repeatTiming, string repeatTarget, bool reservation, bool isProcess, SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		List<RepeatSkillInfo> list = new List<RepeatSkillInfo>();
		for (int i = 0; i < base.RepeatSkillTimingList.Count; i++)
		{
			if (base.RepeatSkillTimingList[i].Timing == repeatTiming && base.RepeatSkillTimingList[i].Target == repeatTarget)
			{
				if (reservation)
				{
					base.RepeatSkillTimingList[i].IsRemoveReservation = true;
				}
				else if (isProcess || !base.RepeatSkillTimingList.Any((RepeatSkillInfo s) => s.IsRemoveReservation && s.Timing == repeatTiming && s.Target == repeatTarget))
				{
					sequentialVfxPlayer.Register(base.RepeatSkillTimingList[i].Skill.Stop(skillProcessor));
					list.Add(base.RepeatSkillTimingList[i]);
				}
			}
		}
		if (list.Count != 0 && !reservation)
		{
			for (int num = 0; num < list.Count; num++)
			{
				base.RepeatSkillTimingList.Remove(list[num]);
			}
		}
		return sequentialVfxPlayer;
	}

	public override VfxBase ReservationAllDepriveRepeatSkill()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		List<RepeatSkillInfo> list = new List<RepeatSkillInfo>();
		for (int i = 0; i < base.RepeatSkillTimingList.Count; i++)
		{
			if (base.RepeatSkillTimingList[i].IsRemoveReservation)
			{
				sequentialVfxPlayer.Register(base.RepeatSkillTimingList[i].Skill.Stop(null));
				list.Add(base.RepeatSkillTimingList[i]);
			}
		}
		if (list.Count != 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				base.RepeatSkillTimingList.Remove(list[j]);
			}
		}
		return sequentialVfxPlayer;
	}

	public override VfxBase ForceDepriveRepeatSkill()
	{
		base.RepeatSkillTimingList.Clear();
		return NullVfx.GetInstance();
	}

	public override VfxBase GiveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter)
	{
		base.CantPlayFilterList.Add(cantPlayCardFilter);
		return InstantVfx.Create(delegate
		{
			foreach (BattleCardBase handCard in _card.SelfBattlePlayer.HandCardList)
			{
				handCard.BattleCardView.UpdateMovability();
			}
		});
	}

	public override VfxBase DepriveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter)
	{
		base.CantPlayFilterList.Remove(cantPlayCardFilter);
		return InstantVfx.Create(delegate
		{
			foreach (BattleCardBase handCard in _card.SelfBattlePlayer.HandCardList)
			{
				handCard.BattleCardView.UpdateMovability();
			}
		});
	}

	public override VfxBase ForceDepriveCantPlay()
	{
		base.CantPlayFilterList.Clear();
		return InstantVfx.Create(delegate
		{
			foreach (BattleCardBase handCard in _card.SelfBattlePlayer.HandCardList)
			{
				handCard.BattleCardView.UpdateMovability();
			}
		});
	}

	public override VfxBase GiveAddTarget(AddTargetInfo info)
	{
		base.AddTargetList.Add(info);
		return NullVfx.GetInstance();
	}

	public override VfxBase DepriveAddTarget(AddTargetInfo info)
	{
		base.AddTargetList.Remove(info);
		return NullVfx.GetInstance();
	}

	public override VfxBase ForceDepriveAddTarget()
	{
		base.AddTargetList.Clear();
		return NullVfx.GetInstance();
	}

	public override VfxBase GiveDecreaseTurnStartPP(int value)
	{
		base.DecreaseTurnStartPPList.Add(value);
		return NullVfx.GetInstance();
	}

	public override VfxBase DepriveDecreaseTurnStartPP(int value)
	{
		base.DecreaseTurnStartPPList.Remove(value);
		return NullVfx.GetInstance();
	}

	public override VfxBase ForceDepriveDecreaseTurnStartPP()
	{
		base.DecreaseTurnStartPPList.Clear();
		return NullVfx.GetInstance();
	}

	public override VfxBase GiveCombatValueModifier(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier, SkillProcessor skillProcessor)
	{
		int life = _card.Life;
		int maxLife = _card.MaxLife;
		VfxBase vfxBase = base.GiveCombatValueModifier(offenseModifier, lifeModifier, skillProcessor);
		VfxBase vfxBase2 = _card.SelfBattlePlayer.StartSkillWhenChangeClassLife(skillProcessor);
		if (this.OnLifeChange != null)
		{
			this.OnLifeChange(RegisterActionBase.ActionBaseParameter.set, new LifeInfomation(_card.Life, _card.MaxLife, life, maxLife));
		}
		return SequentialVfxPlayer.Create(vfxBase, vfxBase2);
	}

	public override VfxBase DepriveCombatValueModifire(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier)
	{
		VfxBase vfxBase = base.DepriveCombatValueModifire(offenseModifier, lifeModifier);
		return SequentialVfxPlayer.Create(vfxBase);
	}

	public override VfxBase ForceDepriveCombatValueModifire()
	{
		VfxBase vfxBase = base.ForceDepriveCombatValueModifire();
		return SequentialVfxPlayer.Create(vfxBase);
	}

	protected override VfxBase CombatModifierChangeCalc(bool isOldAtkBuff, bool isNowAtkBuff, bool isOldMaxLifeBuff, bool isNowMaxLifeBuff, bool isOldAtkDebuff, bool isNowAtkDebuff, bool isOldMaxLifeDebuff, bool isNowMaxLifeDebuff, bool isSkillPowerDown = false, bool isNoBuff = false, bool skipWait = false)
	{
		return NullVfx.GetInstance();
	}

	public override void AddTokenDrawModifier(TokenDrawModifier modifier)
	{
		base.TokenDrawModifiers.Add(modifier);
	}

	public override void RemoveTokenDrawModifier(TokenDrawModifier modifier)
	{
		foreach (TokenDrawModifier tokenDrawModifier in base.TokenDrawModifiers)
		{
			if (tokenDrawModifier.Equals(modifier))
			{
				base.TokenDrawModifiers.Remove(tokenDrawModifier);
				break;
			}
		}
	}
}
