using Wizard.Battle.Card;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class FieldBattleCard : BattleCardBase
{
	private static SkillCreator.SkillBuildInfo _sharedChantBuildInfo;

	public override bool IsEvolution => false;

	public override bool IsField => true;

	public override bool BaseMovable => base.Movable();

	public override bool Attackable => false;

	public override bool IsDead => base.IsDestroyedBySkill;

	public override bool IsCantActivateFanfare => base.SelfBattlePlayer.Class.SkillApplyInformation.IsCantActivateFanfareField;

	public override bool Movable(bool isCheckOnDraw = true, bool isSkipSelecting = false, CHECK_CONDITION_MUTATIONSKILL_TYPE type = CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE, bool isRecording = false)
	{
		type = IsCheckActiveMutationSkill;
		if (!base.Movable(isCheckOnDraw, isSkipSelecting, type, isRecording))
		{
			return false;
		}
		return IsMutationMovable(type);
	}

	public FieldBattleCard(BuildInfo buildInfo)
		: base(buildInfo)
	{
	}

	public override void Setup(bool createNullView = false, bool isRecreate = false)
	{
		base.Setup(createNullView, isRecreate);
		base.BattleCardView.SetupIconAnimations(this, base.Skills);
	}

	public override VfxBase SetUpInplay()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(base.SetUpInplay());
		return parallelVfxPlayer;
	}

	protected SkillCreator.SkillBuildInfo ChantSkillInfoCreate()
	{
		if (_sharedChantBuildInfo == null)
		{
			_sharedChantBuildInfo = new SkillCreator.SkillBuildInfo("chant_count_change", "self_turn_start", "{me.inplay_self.count}>0", "character=me&card_type=field&target=self", "gain_chant=1", "none");
		}
		return _sharedChantBuildInfo;
	}

	protected void SetChantSkill(SkillBase skill)
	{
		if (skill is Skill_chant_count_change skill_chant_count_change)
		{
			skill_chant_count_change.IsSelfChantSkill = true;
		}
	}

	protected override VfxBase StartPlayCard()
	{
		base.StartPlayCard();
		base.SelfBattlePlayer.HandCardToField(this);
		return NullVfx.GetInstance();
	}


	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		if (isNullView)
		{
			return new NullFieldBattleCardView(buildInfo);
		}
		return new FieldBattleCardView(buildInfo);
	}

	public override VfxBase RecoveryInPlay(int inPlayIndex, bool newReplayMoveTurn = false)
	{
		return ParallelVfxPlayer.Create(base.RecoveryInPlay(inPlayIndex, newReplayMoveTurn), NullVfx.GetInstance());
	}

	public override BattleCardBase VirtualClone(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
		VirtualFieldBattleCard virtualFieldBattleCard = new VirtualFieldBattleCard(_buildInfo.VirtualClone(selfBattlePlayer, opponentBattlePlayer));
		CopyToVirtualCardBase(virtualFieldBattleCard);
		return virtualFieldBattleCard;
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
}
