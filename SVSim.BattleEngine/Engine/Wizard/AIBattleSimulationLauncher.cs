using System.Collections;
using System.Collections.Generic;

namespace Wizard;

public class AIBattleSimulationLauncher
{
	private EnemyAI _ai;

	private bool _isTimeOverLogic;

	private bool _isUseEmptyPlayPtnSimulator;

	private bool _isEvol;

	private AIVirtualField _field;

	private SimulationAdditionalActionInfoSet _additionalActions;

	public IBattleSimulationAI BattleSimAI { get; private set; }

	public AIBattleSimulationLauncher(AIVirtualField field, EnemyAI ai, bool useEvo, IBattleSimulationAI emptyPlayPtnSimulator = null, SimulationAdditionalActionInfoSet additionalActionInfoSet = null)
	{
		_ai = ai;
		_isEvol = useEvo;
		_field = field;
		_additionalActions = additionalActionInfoSet;
		CreateSimulator(emptyPlayPtnSimulator);
	}

	private void CreateSimulator(IBattleSimulationAI emptyPlayPtnSimulator)
	{
		if (_ai.IsRunWeakLogic)
		{
			_isTimeOverLogic = true;
			if (_isEvol && _ai.BestPlayPtn.Count <= 0 && emptyPlayPtnSimulator != null)
			{
				BattleSimAI = emptyPlayPtnSimulator;
				_isUseEmptyPlayPtnSimulator = true;
			}
			else
			{
				BattleSimAI = new AITimeOverAttackSimulator();
			}
			return;
		}
		List<AIVirtualActionInfo> allMovesForFullSimulation = AISimulationUtility.GetAllMovesForFullSimulation(_field, _isEvol, _additionalActions);
		if (allMovesForFullSimulation.Count <= AISimulationUtility.FULL_SIMULATION_MOVE_COUNT_TURESHOLD)
		{
			BattleSimAI = new FullSimulationAI(_ai, allMovesForFullSimulation);
		}
		else if (_isEvol && _ai.BestPlayPtn.Count <= 0 && emptyPlayPtnSimulator != null)
		{
			BattleSimAI = emptyPlayPtnSimulator;
			_isUseEmptyPlayPtnSimulator = true;
		}
		else
		{
			BattleSimAI = new BestInPlayMoveAI(_ai);
		}
	}

	public IEnumerator ExecuteBattleSimulationAI(PlayPtnWithToken[] playPtnWithToken, bool checkTimeOverLogic)
	{
		if (!_isUseEmptyPlayPtnSimulator)
		{
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(BattleSimAI._CrSimulate(_field, _isEvol, !_isTimeOverLogic && checkTimeOverLogic, playPtnWithToken, _additionalActions));
			if (_ai.IsRunWeakLogic && !_isTimeOverLogic && checkTimeOverLogic)
			{
				BattleSimAI = new AITimeOverAttackSimulator();
				yield return EnemyAICoroutine.GetInstance().StartCoroutine(BattleSimAI._CrSimulate(_field, _isEvol, checkTimeOverLogic: false, playPtnWithToken, _additionalActions));
			}
		}
	}
}
