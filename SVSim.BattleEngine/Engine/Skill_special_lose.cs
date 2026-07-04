using System.Collections.Generic;
using Wizard;
using Wizard.Battle.View.Vfx;

public class Skill_special_lose : SkillBase
{

	public Skill_special_lose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast)
		{
			return NullVfxWithLoading.GetInstance();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		IEnumerable<BattleCardBase> targetCards = parameter.targetCards;
		bool flag = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.ignore_vibration, "_OPT_NULL_") == "true";
		bool flag2 = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.invalid_ability, "_OPT_NULL_") == "when_special_lose";
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(targetCard.SelfBattlePlayer, targetCard.OpponentBattlePlayer);
			SkillProcessor.ProcessInfo processInfo = targetCard.SelfBattlePlayer.Class.Skills.CreateWhenSpecialLose(parameter.skillProcessor, playerInfoPair);
			if (processInfo != null && !flag2)
			{
				parameter.skillProcessor.Register(processInfo);
				continue;
			}
			targetCard.SelfBattlePlayer.Class.FlagCardAsDestroyedBySkill();
			sequentialVfxPlayer.Register(targetCard.OpponentBattlePlayer.BattleMgr.PlaySpecialWin(targetCard.OpponentBattlePlayer));
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BattleCardBase item in targetCards)
		{
			list.Add(item.SelfBattlePlayer.Class);
		}
		VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, list);
		VfxBase vfx = SequentialVfxPlayer.Create(WaitVfx.Create(base.SkillPrm.buildInfo._effectTime), InstantVfx.Create(delegate
		{
			Skill_special_win.ShakeScreen();
		}));
		SetInductionVoiceIndex(isSpecialWin: true);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create(vfxWithLoading.MainVfx, CreateSkillActivationVoiceVfx());
		if (!flag)
		{
			parallelVfxPlayer.Register(vfx);
		}
		VfxBase mainVfx = SequentialVfxPlayer.Create(parallelVfxPlayer, sequentialVfxPlayer);
		return VfxWithLoading.Create(vfxWithLoading.LoadingVfx, mainVfx);
	}
}
