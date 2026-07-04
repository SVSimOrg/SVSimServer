using System;
using LitJson;
using Wizard;

public class ConventionInfo
{
	public enum ConventionStatus
	{
		GameStart
	}

	public ConventionStatus Status { get; private set; }

	public string Id { get; private set; }

	public string Name { get; private set; }

	public string DeckEntryLimitText { get; private set; }

	public BattleParameter BattleParameterInstance { get; set; }

	public bool IsSelectableTurn { get; private set; }

	public ConventionInfo(JsonData data)
	{
		Id = data["tournament_id"].ToString();
		Name = data["name"].ToString();
		BattleParameterInstance = BattleParameter.JsonToBattleParameter(data);
		Status = (ConventionStatus)data["status"].ToInt();
		DeckEntryLimitText = ConvertTime.ToLocal(DateTime.Parse(data["tournament_start_date"].ToString())).ToString();
		IsSelectableTurn = data["is_selectable_turn"].ToInt() == 1;
	}

	public ConventionInfo(string id)
	{
		Id = id;
	}
}
