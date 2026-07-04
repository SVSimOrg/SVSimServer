using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class EnemyClassBattleCard : ClassBattleCardBase
{
	public EnemyClassBattleCard(ClassBuildInfo buildInfo)
		: base(buildInfo)
	{
	}


	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		if (isNullView)
		{
			return new NullClassBattleCardView(buildInfo);
		}
		return new EnemyClassBattleCardView(buildInfo);
	}
}
