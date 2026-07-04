using UnityEngine;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class VirtualBattleEnemy : BattleEnemy
{
	public override IStatusPanelControl StatusPanelControl => null;

	public VirtualBattleEnemy(BattleManagerBase battleMgr, BattleCamera battleCamera, BackGroundBase backGround)
		: base(battleMgr, battleCamera, backGround, NullInnerOptionsBuilder.GetInstance())
	{
	}

	protected override void Initialize()
	{
		BattleEnemyView = new NullEnemyBattleView();
	}

	protected override IBattlePlayerVfxCreator CreateVfxCreator()
	{
		return new NullBattlePlayerVfxCreator();
	}

	public override void Setup(BattlePlayerBase opponentBattlePlayer)
	{
		base.Setup(opponentBattlePlayer);
	}

	protected override void SetActive()
	{
	}

	public override EffectBattle GetSkillEffect(string skillEffectPath)
	{
		return null;
	}

	public override Vector3 GetFieldCenterPosition()
	{
		return Vector3.zero;
	}

	public override VfxBase CreateUpdateDeckCountLabelVfx()
	{
		return NullVfx.GetInstance();
	}

	protected override VfxBase CreateUpdateClassInfoVfx(bool playEffect)
	{
		return NullVfx.GetInstance();
	}

	public override BattleCardBase CreateCard(int cardId, int cardIndex, bool isChoiceBrave = false)
	{
		BattleCardBase battleCardBase = CardCreatorBase.CreateVirtualCard(cardId, cardIndex, IsPlayer, base.BattleMgr, this, _opponentBattlePlayer, _innerOptionsBuilder);
		SetupCardEvent(battleCardBase);
		return battleCardBase;
	}

	public override IClassInfomationUI CreateClassInfomationUI(int orderCount = 1, int clanId = -1, int totalInfoNum = 1)
	{
		return NullClassInfomationUI.GetInstance();
	}

	public override void SetupCardEvent(BattleCardBase card)
	{
		base.SetupCardEvent(card);
	}
}
