using UnityEngine;

namespace Wizard;

public class QuestItemBase : MonoBehaviour
{
	[SerializeField]
	protected UILabel _questItemLabel;

	[SerializeField]
	protected UILabel _questItemPointLabel;

	[SerializeField]
	protected UILabel _questItemClearLabel;

	public void Initialize(string itemName, int point, bool isClear)
	{
		_questItemLabel.text = itemName;
		_questItemClearLabel.gameObject.SetActive(isClear);
		_questItemPointLabel.text = Data.SystemText.Get("Quest_0018", point.ToString());
	}

	public virtual void Initialize(string itemName, int point, bool isClear, float currentGaugeValue, float maxGaugeValue)
	{
	}
}
