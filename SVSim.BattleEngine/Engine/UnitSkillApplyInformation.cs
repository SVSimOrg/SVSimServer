using Wizard.Battle.View.Vfx;

public class UnitSkillApplyInformation : SkillApplyInformation
{
	public UnitSkillApplyInformation(BattleCardBase card)
		: base(card)
	{
	}

	public override VfxBase GiveQuick()
	{
		base.QuickCount++;
		base.IsQuick = base.QuickCount > 0;
		if (_card.IsSummonDrunkenness && base.Player.IsSelfTurn)
		{
			_card.IsSummonDrunkenness = false;
		}
		return NullVfx.GetInstance();
	}

	public override VfxBase DepriveQuick()
	{
		base.QuickCount--;
		base.IsQuick = base.QuickCount > 0;
		GiveDrunkenness();
		bool isCantAttackClass = _card.IsCantAttackClass;
		return InstantVfx.Create(delegate
		{
			_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect(() => isCantAttackClass);
		});
	}

	public override VfxBase ForceDepriveQuick()
	{
		if (!_card.IsDead)
		{
			base.QuickCount = 0;
			base.IsQuick = false;
			GiveDrunkenness();
		}
		bool isCantAttackClass = _card.IsCantAttackClass;
		return InstantVfx.Create(delegate
		{
			_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect(() => isCantAttackClass);
		});
	}

	public override VfxBase GiveRush(RushInfo info)
	{
		base.RushInfo.Add(info);
		base.IsRush = base.RushInfo.Count > 0;
		if (_card.IsSummonDrunkenness && base.Player.IsSelfTurn)
		{
			_card.IsSummonDrunkenness = false;
		}
		return NullVfx.GetInstance();
	}

	public override VfxBase DepriveRush(RushInfo info)
	{
		base.RushInfo.Remove(info);
		base.IsRush = base.RushInfo.Count > 0;
		GiveDrunkenness();
		bool isCantAttackClass = _card.IsCantAttackClass;
		return InstantVfx.Create(delegate
		{
			_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect(() => isCantAttackClass);
		});
	}

	public override VfxBase ForceDepriveRush()
	{
		if (!_card.IsDead)
		{
			base.RushInfo.Clear();
			base.IsRush = false;
			GiveDrunkenness();
		}
		bool isCantAttackClass = _card.IsCantAttackClass;
		return InstantVfx.Create(delegate
		{
			_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect(() => isCantAttackClass);
		});
	}

	private void GiveDrunkenness()
	{
		if (base.RushInfo.Count <= 0 && base.QuickCount <= 0 && _card.IsFirstTurn)
		{
			_card.IsSummonDrunkenness = true;
		}
	}
}
