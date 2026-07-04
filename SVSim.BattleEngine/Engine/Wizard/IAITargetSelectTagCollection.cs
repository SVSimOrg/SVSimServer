using System.Collections.Generic;

namespace Wizard;

public interface IAITargetSelectTagCollection
{
	void AddSelectInfoToSelectInfoList(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, ref List<AIVirtualTargetSelectInfo> selectInfoList);

	AIVirtualTargetSelectInfo GetSelectInfo(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType selectGroup = AIScriptTokenArgType.TARGET_SELECT);
}
