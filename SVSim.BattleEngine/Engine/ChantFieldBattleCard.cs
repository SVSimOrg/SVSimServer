using Wizard;
using Wizard.Battle.Card;
using Wizard.Battle.View.Vfx;

public class ChantFieldBattleCard : FieldBattleCard
{
	protected readonly int _baseChantCount;

	public override bool IsChantField => true;

	public ChantFieldBattleCard(BuildInfo buildInfo)
		: base(buildInfo)
	{
		_baseChantCount = base.BaseParameter.ChantCount;
		AddChantSkill(buildInfo);
	}

	public void AddChantSkill(BuildInfo buildInfo)
	{
		if (!_normalSkillCollection.HaveNotAttachedResidentChantCountChangeSkill())
		{
			SkillBase skillBase = CreateSkillCreator(buildInfo.SelfBattlePlayer, buildInfo.OpponentBattlePlayer, buildInfo.ResourceMgr).Create(ChantSkillInfoCreate());
			SetChantSkill(skillBase);
			_normalSkillCollection.Add(skillBase);
		}
	}

	public override void FlagCardAsDestroyedBySkill()
	{
		base.IsDestroyedBySkill = true;
		base.DeathTypeInfo.ChantDestroy = true;
	}

	public override VfxBase SetUpInplay()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(base.SetUpInplay());
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		return parallelVfxPlayer;
	}

	protected override void InitSkillApplyInformationOnWhenReturn()
	{
		base.SkillApplyInformation.InitializeInformation(isReturnCard: true);
		base.SkillApplyInformation.ClearParameterModifier();
		ClearCostModifier();
		base.TransformInfo = default(TransformInformation);
		base.SkillApplyInformation.AttachedSkillsInfo.Clear();
		_normalSkillCollection.Clear();
		_evolveSkillCollection.Clear();
		SkillCreator.CardSkillsBuildInfo cardSkillsBuildInfo = SkillCreator.CreateBuildInfo(CardMaster.GetInstanceForBattle().GetCardParameterFromId(base.CardId));
		foreach (SkillBase item in CreateSkillCondition(cardSkillsBuildInfo.normalSkillBuildInfos, base.SelfBattlePlayer, base.OpponentBattlePlayer, _buildInfo.ResourceMgr))
		{
			_normalSkillCollection.Add(item);
			item.SetInductionVoiceIndex();
		}
		foreach (SkillBase item2 in CreateSkillCondition(cardSkillsBuildInfo.evolveSkillBuildInfos, base.SelfBattlePlayer, base.OpponentBattlePlayer, _buildInfo.ResourceMgr))
		{
			_evolveSkillCollection.Add(item2);
			item2.SetInductionVoiceIndex();
		}
		AddChantSkill(_buildInfo);
		base.Skills = _normalSkillCollection;
		base.Skills.Complete();
	}

	public override VfxBase ReturnCard(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(base.SkillApplyInformation.AllSkillEffectStop());
		InitializeParameterOnWhenReturn();
		VfxBase vfx = base.ReturnCard(skillProcessor);
		sequentialVfxPlayer.Register(vfx);
		return sequentialVfxPlayer;
	}

	public override VfxBase RecoveryInPlay(int inPlayIndex, bool newReplayMoveTurn = false)
	{
		return ParallelVfxPlayer.Create(base.RecoveryInPlay(inPlayIndex, newReplayMoveTurn), NullVfx.GetInstance());
	}

	public override BattleCardBase VirtualClone(BattlePlayerBase virtualSelfBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer)
	{
		VirtualChantFieldBattleCard virtualChantFieldBattleCard = new VirtualChantFieldBattleCard(_buildInfo.VirtualClone(virtualSelfBattlePlayer, virtualOpponentBattlePlayer));
		CopyToVirtualCardBase(virtualChantFieldBattleCard);
		return virtualChantFieldBattleCard;
	}

	public override VfxBase CombineVirtualCardSkill(BattleCardBase target)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		base.IsSkillLost = false;
		foreach (SkillBase item in CreateSkillCondition(target.GetBuildInfo.NormalSkillBuildInfos, base.SelfBattlePlayer, base.OpponentBattlePlayer, _buildInfo.ResourceMgr))
		{
			_normalSkillCollection.Add(item);
		}
		AddChantSkill(_buildInfo);
		base.Skills = _normalSkillCollection;
		base.SkillApplyInformation.Combine(target.SkillApplyInformation);
		int count = target.BuffInfoList.Count;
		for (int i = 0; i < count; i++)
		{
			BuffInfo buffInfo = target.BuffInfoList[i];
			if (!(buffInfo.SkillFrom is Skill_powerup) && !(buffInfo.SkillFrom is Skill_power_down) && !base.BuffInfoList.Contains(buffInfo))
			{
				AddBuffInfo(buffInfo);
			}
		}
		base.Skills.Complete();
		CostModifierList.AddRange(target.CostModifierList);
		if (!base.SelfBattlePlayer.BattleMgr.IsVirtualBattle && !base.SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			parallelVfxPlayer.Register(SequentialVfxPlayer.Create(base.SkillApplyInformation.AllSkillEffectRestart(), InstantVfx.Create(delegate
			{
				base.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
			}), base.BattleCardView.BattleCardIconAnimations.Initialize(this, base.Skills)));
		}
		return parallelVfxPlayer;
	}
}
