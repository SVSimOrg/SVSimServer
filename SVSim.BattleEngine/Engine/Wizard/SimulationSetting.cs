namespace Wizard;

public class SimulationSetting
{
	public bool IsHandRemovalValid { get; private set; }

	public bool UseLeaderAttackPreCheck { get; private set; }

	public bool NoSkipAttack { get; private set; }

	public bool CheckFirstActionFailure { get; private set; }

	public SimulationSetting(bool isHandRemovalValid, bool useLeaderAttackPreCheck, bool noSkipAttack = false, bool checkAct = true)
	{
		IsHandRemovalValid = isHandRemovalValid;
		UseLeaderAttackPreCheck = useLeaderAttackPreCheck;
		NoSkipAttack = noSkipAttack;
		CheckFirstActionFailure = checkAct;
	}
}
