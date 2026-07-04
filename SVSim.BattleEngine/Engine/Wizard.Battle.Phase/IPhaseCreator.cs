namespace Wizard.Battle.Phase;

public interface IPhaseCreator
{
	IPhase CreateFirstPhase();

	IPhase CreateOpeningPhase();

	IPhase CreateMulliganPhase();

	IPhase CreateMainPhase();

	IResultPhase CreateResultPhase(bool winnerIsPlayer);
}
