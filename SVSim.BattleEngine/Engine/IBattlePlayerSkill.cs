using Wizard.Battle.View.Vfx;

public interface IBattlePlayerSkill
{
	VfxBase StartBattleHandCard(BattleCardBase card);

	VfxBase StopBattleHandCard(BattleCardBase card);
}
