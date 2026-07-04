using System.Collections.Generic;
using UnityEngine;
using Wizard;
using Wizard.Battle.View.Vfx;

public class Skill_special_win : SkillBase
{

	public Skill_special_win(SkillParameter skillPrm, string option)
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
		bool flag = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.invalid_ability, "_OPT_NULL_") == "when_special_lose";
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(targetCard.OpponentBattlePlayer, targetCard.SelfBattlePlayer);
			SkillProcessor.ProcessInfo processInfo = targetCard.OpponentBattlePlayer.Class.Skills.CreateWhenSpecialLose(parameter.skillProcessor, playerInfoPair);
			if (processInfo != null && !flag)
			{
				parameter.skillProcessor.Register(processInfo);
				if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
				{
					BattleCardBase targetClass = targetCard.OpponentBattlePlayer.Class;
					sequentialVfxPlayer.Register(SkillBase.CreateSingleVfx(base.SkillPrm.resourceMgr, () => targetClass.BattleCardView.GameObject.transform.position, new List<BattleCardBase> { targetClass }, targetClass.IsPlayer, targetClass.BattleCardView, "btl_nerva_2", EffectMgr.EngineType.SHURIKEN, "se_btl_nerva_2", EffectMgr.MoveType.DIRECT_LEADER, EffectMgr.TargetType.SINGLE, 0f));
				}
			}
			else
			{
				targetCard.OpponentBattlePlayer.Class.FlagCardAsDestroyedBySkill();
				sequentialVfxPlayer.Register(targetCard.SelfBattlePlayer.BattleMgr.PlaySpecialWin(targetCard.SelfBattlePlayer));
			}
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BattleCardBase item in targetCards)
		{
			list.Add(item.OpponentBattlePlayer.Class);
		}
		VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, list);
		VfxBase vfxBase = SequentialVfxPlayer.Create(WaitVfx.Create(base.SkillPrm.buildInfo._effectTime), InstantVfx.Create(delegate
		{
			ShakeScreen();
		}));
		SetInductionVoiceIndex(isSpecialWin: true);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create(vfxWithLoading.MainVfx, CreateSkillActivationVoiceVfx(), vfxBase);
		VfxBase mainVfx = SequentialVfxPlayer.Create(parallelVfxPlayer, sequentialVfxPlayer);
		return VfxWithLoading.Create(vfxWithLoading.LoadingVfx, mainVfx);
	}

	// Pre-Phase-5b: static camera-shake helper. Headless has no BattleCamera / BackGround view;
	// the enclosing "special win" celebratory VFX is unreachable. Method body stubbed to no-op.
	public static void ShakeScreen(int shakeScreenMinValue = 10)
	{
	}
}
