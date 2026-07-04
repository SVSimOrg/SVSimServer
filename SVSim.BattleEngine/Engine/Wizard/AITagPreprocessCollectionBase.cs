using System.Collections.Generic;

namespace Wizard;

public class AITagPreprocessCollectionBase
{
	public List<AITagPreprocessInformationBase> InfoList { get; private set; } = new List<AITagPreprocessInformationBase>();

	public bool HasInfo => InfoList.Count > 0;

	protected void CopyInfoListWithReplaceCardReference(List<AIVirtualCard> overrideCardList, List<AITagPreprocessInformationBase> originalInfoList)
	{
		if (originalInfoList != null)
		{
			Clear();
			for (int i = 0; i < originalInfoList.Count; i++)
			{
				AITagPreprocessInformationBase originalInfo = originalInfoList[i];
				GetOverrideCardAndAppendCopyInfo(overrideCardList, originalInfo);
			}
		}
	}

	protected virtual void GetOverrideCardAndAppendCopyInfo(List<AIVirtualCard> overrideCardList, AITagPreprocessInformationBase originalInfo)
	{
	}

	public void Clear()
	{
		InfoList.Clear();
	}
}
