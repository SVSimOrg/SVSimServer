using System.Collections.Generic;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SingleSkill_attach_skill : Skill_attach_skill
{
	public SingleSkill_attach_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override bool GiveSkill(List<BattleCardBase> targets, SkillCreator.SkillBuildInfo buildInfo, CallParameter parameter, long banNum, int voiceIndex, bool isAttachEvolveSkill, ref ParallelVfxPlayer parallelVfx, ref SequentialVfxPlayer iconVfx)
	{
		bool result = base.GiveSkill(targets, buildInfo, parameter, banNum, voiceIndex, isAttachEvolveSkill, ref parallelVfx, ref iconVfx);
		if (!AIBattleInfoReceiver.CheckCreatedValidEnemyAI() || targets == null)
		{
			return result;
		}
		AIBattleInfoReceiver instance = AIBattleInfoReceiver.GetInstance();
		if (instance == null)
		{
			AIBattleInfoReceiver.ShowNotFoundInstanceLog();
			return result;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			BattleCardBase target = targets[i];
			instance.ReceiveAttachedSkillInfo(base.SkillPrm.ownerCard, target);
		}
		return result;
	}
}
