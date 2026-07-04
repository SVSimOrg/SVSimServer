using System.Collections.Generic;

namespace Wizard;

public interface IAIRemoveTagArgument
{
	AIPlayTag RemoveTag { get; }

	void Execute(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, AIPlayTag executingOwnerTag, List<int> playPtn = null);
}
