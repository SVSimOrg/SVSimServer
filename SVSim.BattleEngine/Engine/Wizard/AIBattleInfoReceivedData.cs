namespace Wizard;

public class AIBattleInfoReceivedData
{
	public AttachedSkillInfoReceiveDataCollection AttachedInfoReceiveCollection { get; private set; }

	public AIBattleInfoReceivedData()
	{
		AttachedInfoReceiveCollection = new AttachedSkillInfoReceiveDataCollection();
	}
}
