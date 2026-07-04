using LitJson;

namespace Wizard;

public class QuestMissionDetail
{
	public string Text { get; private set; }

	public int Point { get; private set; }

	public bool IsClear { get; private set; }

	public bool ShowGauge { get; private set; }

	public float CurrentGaugeValue { get; private set; }

	public float MaxGaugeValue { get; private set; }

	public QuestMissionDetail(JsonData data)
	{
		Text = data["name"].ToString();
		Point = data["point"].ToInt();
		IsClear = data["is_clear"].ToBoolean();
		if (data.Keys.Contains("progress"))
		{
			ShowGauge = true;
			CurrentGaugeValue = data["progress"]["gauge_current_number"].ToInt();
			MaxGaugeValue = data["progress"]["gauge_total_number"].ToInt();
		}
		else
		{
			ShowGauge = false;
			CurrentGaugeValue = 0f;
			MaxGaugeValue = 1f;
		}
	}
}
