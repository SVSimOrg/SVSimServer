using System.Linq;

namespace Wizard;

public class AIBattleInfoReceiver
{

	private IEnemyAIBattleInfoRecieveDataAccessor _enemyAI;

	private AIGenerateTagOwnerTable _oprSimGenerateTagOwnerTable;

	private AIBattleInfoReceivedData _oprSimBattleInfoReceiveData;

	private bool _isExecutingOprSim;

	public bool IsNotNullOprSimCollections
	{
		get
		{
			if (_oprSimGenerateTagOwnerTable != null)
			{
				return _oprSimBattleInfoReceiveData != null;
			}
			return false;
		}
	}

	public static bool CheckCreatedValidEnemyAI()
	{
		// Pre-Phase-5b: no mgr in static scope; headless has no AI battle path
		return false;
	}

	public static AIBattleInfoReceiver GetInstance()
	{
		// Pre-Phase-5b: no mgr in static scope; headless has no SingleBattleMgr AI info receiver
		return null;
	}

	public static void ShowNotFoundInstanceLog()
	{
		LocalLog.AccumulateTraceLog("AIBattleInfoReceiver.GetInstance() error. battleMgr does not have battleInfoReceiver.");
	}

	public AIBattleInfoReceiver(IEnemyAI enemyAI)
	{
		_enemyAI = enemyAI as IEnemyAIBattleInfoRecieveDataAccessor;
	}

	public void ReceiveAttachedSkillInfo(BattleCardBase skillOwner, BattleCardBase target)
	{
		string lastAttachedSkillHash = GetLastAttachedSkillHash(target);
		if (_isExecutingOprSim && IsNotNullOprSimCollections)
		{
			RegisterAttachedSkillInfoToOperationSimulator(skillOwner, target, lastAttachedSkillHash);
		}
		else
		{
			RegisterAttachedSkillInfoToRealField(skillOwner, target, lastAttachedSkillHash);
		}
	}

	protected virtual void RegisterAttachedSkillInfoToRealField(BattleCardBase skillOwner, BattleCardBase target, string skillHash)
	{
		if (_enemyAI != null)
		{
			RegisterGenerateTagExecutingParameters(_enemyAI.GenerateTagOwnerTable, _enemyAI.BattleInfoReceivedData, skillOwner, target, skillHash);
		}
	}

	protected string GetLastAttachedSkillHash(BattleCardBase target)
	{
		AttachedSkillInformation attachedSkillsInfo = target.SkillApplyInformation.AttachedSkillsInfo;
		if (attachedSkillsInfo == null || attachedSkillsInfo.AttachedSkills == null || attachedSkillsInfo.AttachedSkills.Count() <= 0)
		{
			return null;
		}
		SkillBase skillBase = attachedSkillsInfo.AttachedSkills.Get(attachedSkillsInfo.AttachedSkills.Count() - 1);
		if (skillBase == null)
		{
			return null;
		}
		return CardSkillHashUtility.GetSingleSkillBaseHash(skillBase).ToString();
	}

	protected void RegisterGenerateTagExecutingParameters(AIGenerateTagOwnerTable generateTagOwnerTable, AIBattleInfoReceivedData battleInfoReceivedData, BattleCardBase skillOwner, BattleCardBase target, string skillHash)
	{
		battleInfoReceivedData.AttachedInfoReceiveCollection.GetInfoFromOwner(skillOwner.BaseParameter.BaseCardId, skillOwner.Index, skillOwner.IsPlayer)?.AddTargetAndSkillHash(target, skillHash);
	}

	public void SetUpOprSimAccessor(AIOperationSimulatorAccessor oprSimAccessor)
	{
		_isExecutingOprSim = true;
		_oprSimGenerateTagOwnerTable = oprSimAccessor.GenerateTagOwnerTable;
		_oprSimBattleInfoReceiveData = oprSimAccessor.BattleInfoReceiveDate;
	}

	public void CleanUpOprSimAccessor()
	{
		_isExecutingOprSim = false;
		_oprSimGenerateTagOwnerTable = null;
		_oprSimBattleInfoReceiveData = null;
	}

	protected void RegisterAttachedSkillInfoToOperationSimulator(BattleCardBase skillOwner, BattleCardBase target, string skillHash)
	{
		if (IsNotNullOprSimCollections)
		{
			RegisterGenerateTagExecutingParameters(_oprSimGenerateTagOwnerTable, _oprSimBattleInfoReceiveData, skillOwner, target, skillHash);
		}
	}
}
