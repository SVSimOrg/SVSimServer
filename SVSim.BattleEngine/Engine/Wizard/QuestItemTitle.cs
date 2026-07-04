using UnityEngine;

namespace Wizard;

public class QuestItemTitle : MonoBehaviour
{
	[SerializeField]
	private UILabel _questItemTitleLabel;

	[SerializeField]
	private UILabel _questItemPointLabel;

	[SerializeField]
	private FlexibleGrid _flexibleGrid;

	public void Initialize(string name, CardBasePrm.ClanType classId, int totalPoint, int maxPoint)
	{
		_questItemTitleLabel.text = name;
		_questItemPointLabel.text = Data.SystemText.Get("Quest_0010", totalPoint.ToString(), maxPoint.ToString());
		if (classId >= CardBasePrm.ClanType.MIN && classId < CardBasePrm.ClanType.MAX)
		{
			ClassCharaPrm.SetClassLabelSetting(_questItemTitleLabel, classId);
		}
	}

	public void AddChild(GameObject item)
	{
		item.transform.parent = _flexibleGrid.transform;
		item.transform.localScale = Vector3.one;
		_flexibleGrid.Reposition();
	}
}
