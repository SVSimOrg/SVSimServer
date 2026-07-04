using UnityEngine;

namespace Wizard;

public class QuestGaugeItem : QuestItemBase
{
	[SerializeField]
	private UIGauge _progressGauge;

	[SerializeField]
	private UILabel _progressGaugeLabel;

	public override void Initialize(string itemName, int point, bool isClear, float currentGaugeValue, float maxGaugeValue)
	{
		Initialize(itemName, point, isClear);
		_progressGaugeLabel.text = currentGaugeValue + "/" + maxGaugeValue;
		float num = currentGaugeValue / maxGaugeValue;
		if (num > 1f)
		{
			num = 1f;
		}
		_progressGauge.Value = num;
	}
}
