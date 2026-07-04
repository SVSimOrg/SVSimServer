using System;
using LitJson;

namespace Wizard;

public class ShopExpirtyInfo
{

	public int? SaleEndSeries { get; private set; }

	public string SaleEndTimeText { get; private set; }

	public bool IsEnableText => GetText() != null;

	public ShopExpirtyInfo(JsonData json)
	{
		if (json.Count != 0)
		{
			if (json.Keys.Contains("sales_period_time"))
			{
				SaleEndTimeText = ConvertTime.ToLocal(DateTime.Parse(json["sales_period_time"].ToString()));
			}
			if (json.Keys.Contains("sales_period_series"))
			{
				SaleEndSeries = json["sales_period_series"].ToInt();
			}
		}
	}

	public ShopExpirtyInfo(string dummyText)
	{
		SaleEndTimeText = dummyText;
	}

	public string GetText()
	{
		if (SaleEndSeries.HasValue)
		{
			return Data.SystemText.Get("Shop_0247", SaleEndSeries.Value.ToString());
		}
		if (SaleEndTimeText != null)
		{
			return Data.SystemText.Get("Shop_0245", SaleEndTimeText);
		}
		return null;
	}
}
