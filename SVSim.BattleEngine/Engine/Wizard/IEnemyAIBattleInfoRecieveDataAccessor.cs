namespace Wizard;

public interface IEnemyAIBattleInfoRecieveDataAccessor
{
	AIGenerateTagOwnerTable GenerateTagOwnerTable { get; }

	AIBattleInfoReceivedData BattleInfoReceivedData { get; }
}
