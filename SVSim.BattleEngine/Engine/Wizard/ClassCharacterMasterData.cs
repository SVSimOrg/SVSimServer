using System;
using UnityEngine;
// TODO(engine-cleanup-pass2): 39 of 45 methods unrun in baseline
//   Type: Wizard.ClassCharacterMasterData
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class ClassCharacterMasterData
{

	private int index;

	public int chara_id { get; private set; }

	public string chara_name { get; private set; }

	public string path { get; private set; }

	public CardBasePrm.ClanType clan { get; private set; }

	public int class_id { get; private set; }

	public string _className { get; private set; }

	public bool is_usable { get; private set; }

	public int skin_id { get; private set; }

	public CardBasePrm.ClanType ClassColorId { get; private set; }

	public bool hide_class_name { get; private set; }

	public int battle_skin_reverse { get; private set; }

	public Vector2 StillPosition { get; private set; }

	public float StillScale { get; private set; }

	public bool IsAcquired { get; private set; }

	public bool IsNew { get; private set; }

	public bool IsHighRank { get; private set; }

	public bool Is3d { get; private set; }

	public bool IsEvolveSkin { get; private set; }

	public bool IsNoEvolveShift { get; private set; }

	public bool IsOpponentReverse { get; private set; }

	public int EvolutionDelayFrame { get; private set; }

	public ClassCharacterMasterData(string[] columns)
	{
		if (columns.Length < 20)
		{
			string text = "Given a bad argument. length : " + columns.Length + " ";
			for (int i = 0; i < columns.Length; i++)
			{
				text = text + columns[i] + " ";
			}
			LocalLog.AccumulateTraceLog(text);
		}
		chara_id = int.Parse(columns[index++]);
		chara_name = ConvClassCharaName(columns[index++]);
		index++;
		path = columns[index++];
		clan = CardBasePrm.ToStrClanType(columns[index++]);
		class_id = (int)clan;
		_className = class_id.ToString(); // Pre-Phase-5b: no clan-name lookup
		index++;
		is_usable = Convert.ToBoolean(int.Parse(columns[index++]));
		skin_id = int.Parse(columns[index++]);
		int num = int.Parse(columns[index++]);
		ClassColorId = (CardBasePrm.ClanType)((num == 0) ? class_id : num);
		hide_class_name = Convert.ToBoolean(int.Parse(columns[index++]));
		battle_skin_reverse = int.Parse(columns[index++]);
		StillPosition = new Vector2(float.Parse(columns[index++]), float.Parse(columns[index++]));
		StillScale = float.Parse(columns[index++]);
		IsHighRank = int.Parse(columns[index++]) == 1;
		Is3d = int.Parse(columns[index++]) == 1;
		IsNoEvolveShift = int.Parse(columns[index++]) == 1;
		IsOpponentReverse = int.Parse(columns[index++]) == 1;
		EvolutionDelayFrame = int.Parse(columns[index++]);
		if (columns.Length >= 20)
		{
			IsEvolveSkin = int.Parse(columns[index++]) == 1;
		}
	}

	private string ConvClassCharaName(string id)
	{
		return Data.Master.GetClassCharaText(id);
	}

	public void UnsetNew()
	{
		IsNew = false;
	}
}
