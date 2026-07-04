using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class BattleInformation : Master.ReadFromCsv
{
	public string Id;

	public string Condition;

	public string Value;

	public string Desc;

	public string CardListDesc;

	public int Priority;

	public string CardListTarget;

	public List<string> ClanIdList;

	public void ReadCsvColumns(string[] columns)
	{
		Id = columns[0];
		Condition = columns[1];
		Value = columns[2];
		Desc = columns[3];
		CardListDesc = columns[3].Replace("BattleInfo", "BattleInfoList");
		if (columns.Length > 4)
		{
			CardListTarget = columns[4];
		}
		if (columns.Length > 5 && int.TryParse(columns[5], out var result))
		{
			Priority = result;
		}
		else
		{
			Priority = -1;
		}
		ClanIdList = ((columns.Length > 6) ? (ClanIdList = columns[6].Split('|').ToList()) : new List<string> { "1", "2", "3", "4", "5", "6", "7", "8" });
	}
}
