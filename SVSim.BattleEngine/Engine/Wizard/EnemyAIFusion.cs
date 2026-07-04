namespace Wizard;

public class EnemyAIFusion
{
	private EnemyAI _ai;

	private AIFusionSituationInfo _fusionSituation;

	public EnemyAIFusion(EnemyAI ai)
	{
		_ai = ai;
		_fusionSituation = null;
	}

	public void InitOnIterationStart()
	{
		_fusionSituation = null;
	}

	public bool FusionAI()
	{
		if (_fusionSituation == null)
		{
			return false;
		}
		_ai.OprTargetSelect(_fusionSituation.Actor, _fusionSituation.SelectedTargets, AIOperationType.FUSION);
		return true;
	}

	public void SetFusionSituation(AIFusionSituationInfo situation)
	{
		_fusionSituation = situation;
	}
}
