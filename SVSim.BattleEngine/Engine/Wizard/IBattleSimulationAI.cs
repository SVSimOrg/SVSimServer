using System.Collections;
using System.Collections.Generic;

namespace Wizard;

public interface IBattleSimulationAI
{
	AIVirtualActionInfo Cr_FirstActionInfo { get; }

	AIVirtualField Cr_MaxField { get; }

	float Cr_MaxFieldValue { get; }

	float Cr_MaxFieldValueAtSelfTurnEnd { get; }

	List<AIVirtualActionInfo> Cr_MaxActionInfoSequence { get; }

	IEnumerator _CrSimulate(AIVirtualField originalField, bool doesUseEvo, bool checkTimeOverLogic, PlayPtnWithToken[] bestPlayPtnsWithToken = null, SimulationAdditionalActionInfoSet additionalActions = null);
}
