using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class Skill_attach_skill : SkillBase
{
	public class AttachOptionInfo
	{
		protected string _info;

		private static readonly string[] INFO_MAKER_TEXT_LIST = new string[15]
		{
			"skill", "timing", "condition", "target", "option", "preprocess", "effect_condition", "icon", "effect_path", "se_path",
			"effect_move_type", "engine_type", "effect_time", "effect_target_type", "skill_voice"
		};

		public string Skill { get; private set; }

		public string Timing { get; private set; }

		public string Condition { get; private set; }

		public string Target { get; private set; }

		public string Option { get; private set; }

		public string Preprocess { get; private set; }

		public HandCardFrameEffectType EffectCondition { get; private set; }

		public string Icon { get; private set; }

		public string EffectPath { get; private set; }

		public string SEPath { get; private set; }

		public string EffectMoveType { get; private set; }

		public string EngineType { get; private set; }

		public string EffectTime { get; private set; }

		public string EffectTargetType { get; private set; }

		public string Voice { get; private set; }

		public AttachOptionInfo(string info)
		{
			_info = info;
			Skill = "none";
			Timing = "none";
			Condition = "none";
			Target = "none";
			Option = "none";
			Preprocess = "none";
			EffectCondition = HandCardFrameEffectType.NONE;
			Icon = string.Empty;
			EffectPath = string.Empty;
			SEPath = string.Empty;
			EffectMoveType = "SKIP";
			EngineType = "SHURIKEN";
			EffectTime = "1";
			EffectTargetType = "none";
			Voice = "none";
			SetupSkillInfo(_info);
		}

		protected void SetupSkillInfo(string info)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < info.Length; i++)
			{
				if (info[i] == '(')
				{
					if (num == 0)
					{
						num2 = i;
					}
					num++;
				}
				if (info[i] == ')')
				{
					num--;
					if (num == 0)
					{
						string value = info.Substring(num2, i - (num2 - 1));
						KeyIsSetting(value);
					}
				}
			}
		}

		protected void KeyIsSetting(string value)
		{
			int num = -1;
			string text = string.Empty;
			for (int i = 0; i < INFO_MAKER_TEXT_LIST.Length; i++)
			{
				int num2 = value.IndexOf(INFO_MAKER_TEXT_LIST[i] + ":");
				if (num2 != -1 && (num == -1 || num > num2))
				{
					num = num2;
					text = INFO_MAKER_TEXT_LIST[i];
				}
			}
			if (text != string.Empty)
			{
				value = value.Remove(value.Count() - 1, 1).Remove(0, 1);
			}
			if (text != null)
			{
				switch (text)
				{
				case "skill":
					value = value.Remove(0, "skill:".Count());
					Skill = value;
					break;
				case "timing":
					value = value.Remove(0, "timing:".Count());
					Timing = value;
					break;
				case "condition":
					value = value.Remove(0, "condition:".Count());
					Condition = value;
					break;
				case "target":
					value = value.Remove(0, "target:".Count());
					Target = value;
					break;
				case "option":
					value = value.Remove(0, "option:".Count());
					Option = value;
					break;
				case "preprocess":
					value = value.Remove(0, "preprocess:".Count());
					Preprocess = value;
					break;
				case "effect_condition":
					value = value.Remove(0, "effect_condition:".Count());
					EffectCondition = HandCardFrameEffectControl.ToStrFrameEffect(value)[0];
					break;
				case "icon":
					value = value.Remove(0, "icon:".Count());
					Icon = value;
					break;
				case "effect_path":
					value = value.Remove(0, "effect_path:".Count());
					EffectPath = value;
					break;
				case "se_path":
					value = value.Remove(0, "se_path:".Count());
					SEPath = value;
					break;
				case "effect_move_type":
					value = value.Remove(0, "effect_move_type:".Count());
					EffectMoveType = value;
					break;
				case "engine_type":
					value = value.Remove(0, "engine_type:".Count());
					EngineType = value;
					break;
				case "effect_time":
					value = value.Remove(0, "effect_time:".Count());
					EffectTime = value;
					break;
				case "effect_target_type":
					value = value.Remove(0, "effect_target_type:".Count());
					EffectTargetType = value;
					break;
				case "skill_voice":
					value = value.Remove(0, "skill_voice:".Count());
					Voice = value;
					break;
				}
			}
		}
	}

	protected List<SkillBase> _attachSkills = new List<SkillBase>();

	public Action OnIndividualIdSkillStop;

	protected List<BattleCardBase> _effectTargets;

	public SkillCreator.SkillBuildInfo BuildInfo { get; protected set; }

	public string SaveTurnSkillId { get; private set; } = string.Empty;

	public int AttachedTurn { get; private set; } = -1;

	public bool IsEvolutionEndStop { get; set; }

	public override bool IsTargetIndicate => false;

	protected virtual bool IsTargetSelectedCard
	{
		get
		{
			if (!base.IsUserSelectType)
			{
				return base.ApplyingTargetFilter is SkillTargetLastTargetFilter;
			}
			return true;
		}
	}

	public bool AttachWhenDestroy => BuildInfo._timing == "when_destroy";

	public List<SkillBase> GetAttachSkills()
	{
		return _attachSkills;
	}

	public Skill_attach_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		string option2 = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.skill);
		SkillCreator.SkillBuildInfo skillBuildInfo = skillPrm.ownerCard.SelfBattlePlayer.BattleMgr.BattleLifeTimeSharedObject.GetSkillBuildInfo(option2);
		if (skillBuildInfo == null)
		{
			BuildInfo = CreateAttachSkillBuildInfo(option2);
			skillPrm.ownerCard.SelfBattlePlayer.BattleMgr.BattleLifeTimeSharedObject.SetSkillBuildInfo(option2, BuildInfo);
		}
		else
		{
			BuildInfo = skillBuildInfo;
		}
		SetDuplicateBanSkillNum();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		_attachSkills = new List<SkillBase>();
		long banNum = base.OptionValue.GetLong(SkillFilterCreator.ContentKeyword.duplicate_ban_id, 0);
		bool isAttachEvolveSkill = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_evolve, "false") == "true";
		string[] array = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.save_turn_skill_id).Split(':');
		SaveTurnSkillId = array[0];
		if (array.Count() > 1 && array[1] == SkillFilterCreator.ContentKeyword.is_individual.ToString())
		{
			SaveTurnSkillId += base.IndividualId;
		}
		int voiceIndex = AddSkillVoice(this, BuildInfo);
		ParallelVfxPlayer parallelVfx = ParallelVfxPlayer.Create();
		SequentialVfxPlayer iconVfx = SequentialVfxPlayer.Create();
		List<BattleCardBase> list = parameter.targetCards.Where((BattleCardBase s) => s.IsInDeck || s.IsInHand || s.IsInplay).ToList();
		if (base.DuplicateBanSkillNum != string.Empty)
		{
			List<BattleCardBase> list2 = new List<BattleCardBase>();
			for (int num = 0; num < list.Count; num++)
			{
				IEnumerable<Skill_attach_skill> enumerable = from s in list[num].Skills
					where s.IsAttachedSkill && s.GetAttachSkill != null
					select s.GetAttachSkill as Skill_attach_skill;
				if (enumerable != null && enumerable.Count() > 0)
				{
					if (!enumerable.Any((Skill_attach_skill s) => (!base.IsDuplicateBanSelfSkill || s.SkillPrm.ownerCard == base.SkillPrm.ownerCard) && s.DuplicateBanSkillNum == base.DuplicateBanSkillNum))
					{
						list2.Add(list[num]);
					}
				}
				else
				{
					list2.Add(list[num]);
				}
			}
			list = list2;
		}
		CallOnAttachSkill(list.Where((BattleCardBase c) => IsEffectTarget(c)).ToList());
		if (GiveSkill(list, BuildInfo, parameter, banNum, voiceIndex, isAttachEvolveSkill, ref parallelVfx, ref iconVfx))
		{
			AttachedTurn = base.SkillPrm.ownerCard.SelfBattlePlayer.Turn;
		}
		if (IsBattleLog && IsTargetInOpponentHand() && !IsTargetSelectedCard)
		{
			LoggingOpponentPrivateCard(list.ToList(), BuildInfo);
		}
		_effectTargets = new List<BattleCardBase>();
		if (list.Count() == 0)
		{
			return NullVfxWithLoading.GetInstance();
		}
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
		{
			_effectTargets = list;
		}
		else
		{
			_effectTargets = list.Where((BattleCardBase c) => IsEffectTarget(c)).ToList();
		}
		bool flag = (base.SkillPrm.ownerCard.IsPlayer && base.ApplyBattlePlayerFilter is SelfBattlePlayerFilter) || (!base.SkillPrm.ownerCard.IsPlayer && base.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter);
		bool isFollowInHand = parameter.targetCards.Count() > 0 && flag && IsTargetInHand();
		VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, _effectTargets, isFollowInHand, addToLastOperation: true);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(vfxWithLoading.MainVfx, parallelVfx, iconVfx);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		return vfxWithLoadingSequential;
	}

	public virtual bool GiveSkill(List<BattleCardBase> targets, SkillCreator.SkillBuildInfo buildInfo, CallParameter parameter, long banNum, int voiceIndex, bool isAttachEvolveSkill, ref ParallelVfxPlayer parallelVfx, ref SequentialVfxPlayer iconVfx)
	{
		bool flag = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.show_battle_log, "true") == "true";
		bool result = false;
		foreach (BattleCardBase target in targets)
		{
			if (target.SkillApplyInformation.AttachedSkillsInfo.DuplicateBanNum.Any((long c) => c != 0L && c == banNum))
			{
				continue;
			}
			BattleCardBase battleCardBase = target;
			SkillBase skill = target.SkillApplyInformation.AttachSkill(BuildInfo, base.SkillPrm.resourceMgr, base.SkillPrm.ownerCard.GetName(), base.SkillPrm.ownerCard.CardId, banNum, this, isAttachEvolveSkill);
			if (skill == null)
			{
				continue;
			}
			result = true;
			_attachSkills.Add(skill);
			if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessInPlayPeriodOfTime))
			{
				skill.OnSkillStopStart += delegate(SkillBase _skill, List<BattleCardBase> cards, SkillProcessor skillProcessorOneTime)
				{
					Stop(skillProcessorOneTime);
				};
			}
			else if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessRemoveAfterAction))
			{
				skill.ChacheIsMakeFoil();
			}
			else if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessDamageAfterStop))
			{
				skill.OnSkillStart += delegate
				{
					BattleCardBase card = skill.SkillPrm.ownerCard;
					card.OnDamageAfter += (SkillProcessor skillProcessorOneTime) => StopSpecificCard(card);
				};
			}
			else if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessReflectionAfterStop))
			{
				skill.OnSkillStart += delegate
				{
					BattleCardBase card = skill.SkillPrm.ownerCard;
					card.OnReflectionAfter += (SkillProcessor skillProcessorOneTime) => StopSpecificCard(card);
				};
			}
			BuffInfo buffInfo = (IsBattleLog ? AddBuffInfoIfNeeded(target) : null);
			if (base.HasIndividualId && (battleCardBase.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch))
			{
				skill.AddIndividualIdSkillBuffLog(this, battleCardBase);
			}
			if (target.IsClass)
			{
				UpdateClassBuffIfActive(target);
			}
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(target, buffInfo, base.SkillPrm.ownerCard.Skills.IndexOf(this), "", skill, banNum);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			parallelVfx.Register(CheckResidentAndImmediateAttachSkillCondition(parameter.skillProcessor, targets.ToList(), skill));
			if (!target.IsInHand)
			{
				iconVfx.Register(target.BattleCardView.InitializeBattleCardIcon(target, target.Skills));
			}
			base.buffInfoContainer.Add(buffInfoContainer);
			parallelVfx.Register(InstantVfx.Create(delegate
			{
			}));
			if (IsBattleLog && IsEffectTarget(target) && flag)
			{
				BattleLogManager.GetInstance().AddLogSkillAttachSkill(target, skill, this);
			}
			base.IsActivity = true;
			skill.SetSkillVoiceIndex(voiceIndex);
			string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(skill.SkillPrm.buildInfo._effectPath, ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true);
			VfxBase vfx = base.SkillPrm.resourceMgr.LoadEffectBattle(assetTypePath, skill.SkillPrm.buildInfo._effectPath, skill.SkillPrm.buildInfo._sePath);
			parallelVfx.Register(vfx);
		}
		return result;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (!IsEvolutionEndStop)
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			foreach (BuffInfoContainer item in buffInfoContainer)
			{
				item._targetCard.SkillApplyInformation.RemoveSkill(item._attachSkill, base.SkillPrm.ownerCard, item._duplicateBanNum, this, item._intValue);
				list.Add(item._targetCard);
				item._targetCard.RemoveBuffInfo(item._buffInfo);
				if (item._targetCard.IsClass)
				{
					UpdateClassBuffIfActive(item._targetCard);
				}
				if (!item._targetCard.IsClass && !item._targetCard.IsInHand)
				{
					parallelVfxPlayer.Register(item._targetCard.BattleCardView.InitializeBattleCardIcon(item._targetCard, item._targetCard.Skills));
				}
			}
			CallOnUpdateSkillEffect(list);
			buffInfoContainer.Clear();
		}
		OnIndividualIdSkillStop.Call();
		StopEnd(skillProcessor);
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public VfxWithLoading StopSpecificCard(BattleCardBase card)
	{
		BuffInfoContainer buffInfoContainer = base.buffInfoContainer.FirstOrDefault((BuffInfoContainer b) => b._targetCard == card);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (buffInfoContainer != null)
		{
			buffInfoContainer._targetCard.SkillApplyInformation.RemoveSkill(buffInfoContainer._attachSkill, base.SkillPrm.ownerCard, buffInfoContainer._duplicateBanNum, this, buffInfoContainer._intValue);
			buffInfoContainer._targetCard.RemoveBuffInfo(buffInfoContainer._buffInfo);
			if (buffInfoContainer._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(buffInfoContainer._targetCard);
			}
			if (!buffInfoContainer._targetCard.IsClass && !buffInfoContainer._targetCard.IsInHand)
			{
				parallelVfxPlayer.Register(buffInfoContainer._targetCard.BattleCardView.InitializeBattleCardIcon(buffInfoContainer._targetCard, buffInfoContainer._targetCard.Skills));
			}
			base.buffInfoContainer.Remove(buffInfoContainer);
		}
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override VfxWithLoading RemoveAfter()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			item._targetCard.SkillApplyInformation.RemoveSkill(item._attachSkill, base.SkillPrm.ownerCard, item._duplicateBanNum, this, item._intValue);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			if (!item._targetCard.IsClass && !item._targetCard.IsInHand)
			{
				parallelVfxPlayer.Register(item._targetCard.BattleCardView.InitializeBattleCardIcon(item._targetCard, item._targetCard.Skills));
			}
		}
		buffInfoContainer.Clear();
		OnIndividualIdSkillStop.Call();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	private bool IsEffectTarget(BattleCardBase target)
	{
		if (!target.IsPlayer && !target.IsInplay)
		{
			return SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch;
		}
		return true;
	}

	public static SkillCreator.SkillBuildInfo CreateAttachSkillBuildInfo(string option)
	{
		if (string.IsNullOrEmpty(option))
		{
			return null;
		}
		AttachOptionInfo attachOptionInfo = new AttachOptionInfo(option);
		return new SkillCreator.SkillBuildInfo(attachOptionInfo.Skill, attachOptionInfo.Timing, attachOptionInfo.Condition, attachOptionInfo.Target, attachOptionInfo.Option, attachOptionInfo.Preprocess, attachOptionInfo.EffectCondition, attachOptionInfo.Icon, null, attachOptionInfo.EffectPath, sePath: attachOptionInfo.SEPath, effectMoveType: EffectMgr.ToStrMoveType(attachOptionInfo.EffectMoveType), engineType: EffectMgr.ToStrEngineType(attachOptionInfo.EngineType), effectTime: float.Parse(attachOptionInfo.EffectTime), effectTargetType: EffectMgr.ToStrTargetType(attachOptionInfo.EffectTargetType), voice: attachOptionInfo.Voice);
	}

	public static SkillBase CreateAndAttachSkill(BattleCardBase targetCard, SkillBase originalSkill, SkillCreator.SkillBuildInfo buildInfo)
	{
		SkillBase skill = targetCard.SkillApplyInformation.AttachSkill(buildInfo, originalSkill.SkillPrm.resourceMgr, originalSkill.SkillPrm.ownerCard.GetName(), originalSkill.SkillPrm.ownerCard.CardId, originalSkill.OptionValue.GetLong(SkillFilterCreator.ContentKeyword.duplicate_ban_id, 0), originalSkill);
		if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessInPlayPeriodOfTime))
		{
			skill.OnSkillStopStart += delegate(SkillBase _skill, List<BattleCardBase> cards, SkillProcessor skillProcessorOneTime)
			{
				originalSkill.Stop(skillProcessorOneTime);
			};
		}
		else if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessRemoveAfterAction))
		{
			skill.OnSkillEnd += (SkillBase _skill, List<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessorOneTime) => skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessRemoveAfterAction && (s as SkillPreprocessRemoveAfterAction).IsEnd()) ? ((VfxBase)originalSkill.Stop(skillProcessorOneTime)) : ((VfxBase)NullVfx.GetInstance());
		}
		else if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessDamageAfterStop))
		{
			skill.OnSkillStart += delegate
			{
				skill.SkillPrm.ownerCard.OnDamageAfter += (SkillProcessor skillProcessorOneTime) => originalSkill.Stop(skillProcessorOneTime);
			};
		}
		else if (skill.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessReflectionAfterStop))
		{
			skill.OnSkillStart += delegate
			{
				skill.SkillPrm.ownerCard.OnReflectionAfter += (SkillProcessor skillProcessorOneTime) => originalSkill.Stop(skillProcessorOneTime);
			};
		}
		int skillVoiceIndex = AddSkillVoice(skill, buildInfo);
		skill.SetSkillVoiceIndex(skillVoiceIndex);
		return skill;
	}

	private static int AddSkillVoice(SkillBase skill, SkillCreator.SkillBuildInfo buildInfo)
	{
		if (buildInfo._voice != string.Empty && skill.SkillPrm.ownerCard.BattleCardView != null)
		{
			return skill.SkillPrm.ownerCard.BattleCardView.VoiceInfo.AddAttachSkillVoice(buildInfo._voice);
		}
		return -1;
	}

	private VfxBase CheckResidentAndImmediateAttachSkillCondition(SkillProcessor skillProcessor, List<BattleCardBase> targets, SkillBase skill)
	{
		BattlePlayerBase turnPlayer = (base.SkillPrm.selfBattlePlayer.IsSelfTurn ? base.SkillPrm.selfBattlePlayer : base.SkillPrm.opponentBattlePlayer);
		BattlePlayerBase notTurnPlayer = (base.SkillPrm.selfBattlePlayer.IsSelfTurn ? base.SkillPrm.opponentBattlePlayer : base.SkillPrm.selfBattlePlayer);
		BattlePlayerPair onTurnPlayerPair = new BattlePlayerPair(turnPlayer, notTurnPlayer);
		BattlePlayerPair notTurnPlayerPair = new BattlePlayerPair(notTurnPlayer, turnPlayer);
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		list = targets.Where((BattleCardBase c) => c.IsInplay && c.IsPlayer == turnPlayer.IsPlayer).ToList();
		list2 = targets.Where((BattleCardBase c) => c.IsInplay && c.IsPlayer == notTurnPlayer.IsPlayer).ToList();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(ProcessWhenChangeImmediate(list, onTurnPlayerPair, notTurnPlayerPair));
		parallelVfxPlayer.Register(ProcessWhenChangeImmediate(list2, onTurnPlayerPair, notTurnPlayerPair));
		parallelVfxPlayer.Register(RegisterInPlaySkill(list, skillProcessor, onTurnPlayerPair, notTurnPlayerPair));
		parallelVfxPlayer.Register(RegisterInPlaySkill(list2, skillProcessor, onTurnPlayerPair, notTurnPlayerPair));
		return parallelVfxPlayer;
	}

	private VfxBase ProcessWhenChangeImmediate(List<BattleCardBase> inPlayCards, BattlePlayerPair onTurnPlayerPair, BattlePlayerPair notTurnPlayerPair)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		for (int i = 0; i < inPlayCards.Count; i++)
		{
			sequentialVfxPlayer.Register(inPlayCards[i].Skills.RegisterAndProcessWhenChangeInplayImmediateInfo(inPlayCards[i].IsSelfTurn ? onTurnPlayerPair : notTurnPlayerPair));
		}
		return sequentialVfxPlayer;
	}

	private VfxBase RegisterInPlaySkill(List<BattleCardBase> inPlayCards, SkillProcessor skillProcessor, BattlePlayerPair onTurnPlayerPair, BattlePlayerPair notTurnPlayerPair)
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		for (int i = 0; i < inPlayCards.Count; i++)
		{
			inPlayCards[i].Skills.CreateAndRegisterWhenChangeInplayInfo(inPlayCards, skillProcessor, inPlayCards[i].IsSelfTurn ? onTurnPlayerPair : notTurnPlayerPair, isSummonCheck: false);
		}
		return result;
	}

	protected void LoggingOpponentPrivateCard(List<BattleCardBase> targets, SkillCreator.SkillBuildInfo buildInfo)
	{
		if (!IsBattleLog || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.BattleEnemy.HandCardList.Count <= 0)
		{
			return;
		}
		BattleLogManager instance = BattleLogManager.GetInstance();
		SkillBase attachedSkill = base.SkillPrm.ownerCard.CreateSkillCreator(base.SkillPrm.ownerCard.SelfBattlePlayer, base.SkillPrm.ownerCard.OpponentBattlePlayer, base.SkillPrm.resourceMgr).Create(buildInfo, null, isAttachSkill: true, this);
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				instance.AddLogSkillAttachSkill(targets[i], attachedSkill, this);
			}
		}
		else if (!(base.ApplyingTargetFilter is SkillTargetSkillDrewCardFilter) || targets.Count != 0)
		{
			instance.AddLogSkillAttachSkill(SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.BattleEnemy.Class, attachedSkill, this, isTargetInOpponentHand: true);
		}
	}

	public static void GiveIndividualId(SkillBase attachSkill, SkillBase createdSkill)
	{
		BattleCardBase ownerCard = attachSkill.SkillPrm.ownerCard;
		bool flag = ownerCard.NormalSkills.Any((SkillBase s) => s == attachSkill);
		int num = (flag ? ownerCard.NormalIndividualId : ownerCard.EvolutionIndividualId);
		if (num != -1)
		{
			createdSkill.SetIndividualId(num);
			return;
		}
		createdSkill.InitSetIndividualId();
		if (createdSkill.IndividualId != -1)
		{
			if (flag)
			{
				ownerCard.NormalIndividualId = createdSkill.IndividualId;
			}
			else
			{
				ownerCard.EvolutionIndividualId = createdSkill.IndividualId;
			}
			ownerCard.SelfBattlePlayer.BattleMgr.IncrementIndividualId();
		}
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			OnIndividualIdSkillStop.Call();
			return NullVfx.GetInstance();
		};
	}

	public virtual void CallOnAttachSkill(List<BattleCardBase> targetCards)
	{
	}
}
