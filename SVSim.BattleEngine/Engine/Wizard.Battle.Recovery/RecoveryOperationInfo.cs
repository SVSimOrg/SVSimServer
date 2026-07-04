using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cute;
using LitJson;
using Wizard.AutoTest;
using Wizard.Battle.Operation;

namespace Wizard.Battle.Recovery;

public class RecoveryOperationInfo
{
	public DataMgr.BattleType BattleType { get; private set; }

	public long RecordTime { get; private set; }

	public SetupConditionInfo SetupInfo { get; private set; }

	public IEnumerable<IOperationCommand> ActionCommands { get; private set; }

	public ResultConditionInfo CheckInfo { get; private set; }

	public IEnumerable<string> SkillTargetCardNames { get; private set; }

	public long MulliganStartTime { get; private set; }

	public long TurnStartTime { get; private set; }

	public long OpeningStartTime { get; private set; }

	public RecoveryOperationInfo(string filePath)
	{
		JsonData jsonData = ReadRecoveryFile(filePath);
		BattleType = (DataMgr.BattleType)jsonData.ToIntOrDefault("battle_type", 100);
		MulliganStartTime = jsonData["setup"].ToLongOrDefault("start_mulligan_time", 0);
		OpeningStartTime = jsonData["setup"].ToLongOrDefault("opening_start_time", 0);
		TurnStartTime = jsonData.ToLongOrDefault("turn_start_time", 0);
		RecordTime = jsonData.ToLongOrDefault("record_time", 0);
		SetupInfo = new SetupConditionInfo(jsonData["setup"], BattleType);
		ActionCommands = AutoTestBattleMgr.CreateOperationCommands(jsonData.ToJsonDataCollection("operations"), SetupInfo.DidPlayerGoFirst);
		CheckInfo = new ResultConditionInfo(jsonData["check"]);
		SkillTargetCardNames = from n in jsonData.ToJsonDataCollection("skill_targets")
			select n.ToString();
	}

	public static JsonData ReadRecoveryFile(string filePath)
	{
		JsonData result = new JsonData();
		using (StreamReader streamReader = new StreamReader(filePath))
		{
			string text = streamReader.ReadToEnd();
			if (!string.IsNullOrEmpty(text))
			{
				result = JsonMapper.ToObject(CryptAES.decryptForNode(text));
			}
		}
		return result;
	}
}
