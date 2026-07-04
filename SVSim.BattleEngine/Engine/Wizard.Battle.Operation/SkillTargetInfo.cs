using Wizard.AutoTest;

namespace Wizard.Battle.Operation;

public class SkillTargetInfo
{
	public AutoTestBattleMgr.CardInfo OwnerInfo { get; private set; }

	public AutoTestBattleMgr.CardInfo TargetCard { get; private set; }

	public SkillTargetInfo(AutoTestBattleMgr.CardInfo ownerInfo, string targetTest)
	{
		OwnerInfo = ownerInfo;
		TargetCard = new AutoTestBattleMgr.CardInfo(targetTest);
	}
}
