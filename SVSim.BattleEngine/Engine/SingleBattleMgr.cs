using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Recovery;
using Wizard.Battle.View.Vfx;
using Wizard.BattleMgr;

public class SingleBattleMgr : BattleManagerBase
{
	private bool? _isPlayerFirstTurn;

	public AIBattleInfoReceiver BattleInfoReceiver { get; protected set; }

	public event Action OnBattleRetire;

	public SingleBattleMgr(IBattleMgrContentsCreator contentsCreator)
		: base(contentsCreator)
	{
	}

	// Phase-5 chunk 45: overload accepting a pre-seeded GameMgr so test fixtures can construct
	// with a chara-id/net-user-seeded GameMgr without going through the ambient bridge.
	public SingleBattleMgr(IBattleMgrContentsCreator contentsCreator, GameMgr gameMgr)
		: base(contentsCreator, gameMgr)
	{
	}

	public override IInnerOptionsBuilder CreateEnemyInnerOptionsBuilder()
	{
		return new EnemyAIInnerOptionsBuilder();
	}

	protected override void SetupEvent()
	{
		base.SetupEvent();
		BattlePlayer.OnShortageDeck += () => OnShortageDeck(BattlePlayer);
		BattleEnemy.OnShortageDeck += () => OnShortageDeck(BattleEnemy);
		BattlePlayer.OnTurnEndFinish += delegate
		{
			base.IsTurnEnd = false;
			VfxBase result = ControlTurnStartOpponent();
			base.IsTurnEnd = true;
			return result;
		};
		BattleEnemy.OnTurnEndFinish += delegate
		{
			base.IsTurnEnd = false;
			VfxBase result = ControlTurnStartPlayer();
			base.IsTurnEnd = true;
			return result;
		};
	}

	public override void SetupBattlePlayersEvent()
	{
		base.SetupBattlePlayersEvent();
		if (BattlePlayer.PlayerEmotion.IsContainsEmotionType(ClassCharaPrm.EmotionType.NEGOTIATION_1))
		{
			SetUpStoryNegotiationsEmote();
		}
		if (BattlePlayer.PlayerEmotion.IsContainsEmotionType(ClassCharaPrm.EmotionType.PLAYER_TURN_START_1))
		{
			SetUpStoryPlayerTurnStartEmote();
		}
		switch (this.GameMgr.GetDataMgr().m_BattleType)
		{
		case DataMgr.BattleType.BossRushQuest:
		case DataMgr.BattleType.SecretBossQuest:
			SetUpBossRushSpecialSkill();
			break;
		case DataMgr.BattleType.Story:
		case DataMgr.BattleType.Quest:
			SetUpStorySpecialBattle();
			break;
		}
	}

	private void SetUpBossRushSpecialSkill()
	{
		DataMgr dataMgr = this.GameMgr.GetDataMgr();
		DataMgr.SpecialBattleSetting specialBattleSettingInfo = dataMgr.SpecialBattleSettingInfo;
		BossRushBattleData bossRushBattleData = dataMgr.BossRushBattleData;
		if (specialBattleSettingInfo == null || bossRushBattleData == null)
		{
			return;
		}
		_isPlayerFirstTurn = specialBattleSettingInfo.IsPlayerFirstTurn;
		BattlePlayer.PpTotal = specialBattleSettingInfo.PlayerStartPp;
		BattleEnemy.PpTotal = specialBattleSettingInfo.EnemyStartPp;
		BattlePlayer.SetPpTotal(BattlePlayer.PpTotal, isUpdatePp: true, null);
		BattleEnemy.SetPpTotal(BattleEnemy.PpTotal, isUpdatePp: true, null);
		string[] playerAttachSkills = specialBattleSettingInfo.PlayerAttachSkills;
		string[] enemyAttachSkills = specialBattleSettingInfo.EnemyAttachSkills;
		List<BossRushSpecialSkill> list = new List<BossRushSpecialSkill>();
		if (bossRushBattleData.PlayerSkillList != null)
		{
			list.AddRange(bossRushBattleData.PlayerSkillList);
		}
		if (bossRushBattleData.EnemySkill != null)
		{
			list.Add(bossRushBattleData.EnemySkill);
		}
		List<BossRushSpecialSkill> list2 = new List<BossRushSpecialSkill>();
		for (int i = 0; i < list.Count(); i++)
		{
			for (int num = 0; num < list.ElementAt(i).SkillText.Split(',').Length; num++)
			{
				list2.Add(list.ElementAt(i));
			}
		}
		List<AttachInfo> source = SetUpAttachSkill(playerAttachSkills, enemyAttachSkills);
		for (int j = 0; j < source.Count(); j++)
		{
			AttachInfo attachInfo = source.ElementAt(j);
			BossRushSpecialSkill bossRushSpecialSkill = list2.ElementAt(j);
			if (attachInfo._classCard.SelfBattlePlayer.IsPlayer)
			{
				attachInfo._attachSkill.SkillPrm.ownerCard = CardCreatorBase.CreateSpecialSkillCard(bossRushSpecialSkill.OriginalCardId, attachInfo._classCard.SelfBattlePlayer.IsPlayer, 0, SBattleLoad, this, base.BattleResourceMgr, NullInnerOptionsBuilder.GetInstance(), list2.ElementAt(j));
			}
			attachInfo._classCard.SelfBattlePlayer.BossRushSpecialSkillList.Add(bossRushSpecialSkill);
			BuffInfo buffInfo = new BuffInfo(attachInfo._attachSkill.SkillPrm.ownerCard.BaseParameter.BaseCardId, attachInfo._attachSkill.SkillPrm.ownerCard.BaseParameter.BaseCardId, attachInfo._attachSkill);
			buffInfo.SpecialSkillInfo = bossRushSpecialSkill;
			buffInfo.SkillFrom.SkillPrm.ownerCard = attachInfo._attachSkill.SkillPrm.ownerCard;
			attachInfo._classCard.AddBuffInfo(buffInfo);
		}
	}

	protected virtual void SetUpStoryNegotiationsEmote()
	{
		Func<SkillProcessor, VfxBase> callOneTime = null;
		if (BattleEnemy.Turn == 0)
		{
			callOneTime = delegate
			{
				SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
				if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_OTHER_PLAYER_EMOTE))
				{
					BattleEnemy.OnTurnStartBeforeDraw -= callOneTime;
					sequentialVfxPlayer.Register(BattleEnemy.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.NEGOTIATION_1, 0f));
					sequentialVfxPlayer.Register(new OpeningVfx.WaitVoiceEndVfx());
					sequentialVfxPlayer.Register(BattlePlayer.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.NEGOTIATION_1, 0f));
					SetUpStoryNegotiationsEmote();
				}
				return sequentialVfxPlayer;
			};
			BattleEnemy.OnTurnStartBeforeDraw += callOneTime;
		}
		else if (BattleEnemy.Turn == 1)
		{
			callOneTime = delegate
			{
				SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
				if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_OTHER_PLAYER_EMOTE))
				{
					BattleEnemy.OnTurnStartBeforeDraw -= callOneTime;
					sequentialVfxPlayer.Register(BattleEnemy.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.NEGOTIATION_2, 0f));
					sequentialVfxPlayer.Register(new OpeningVfx.WaitVoiceEndVfx());
					sequentialVfxPlayer.Register(BattlePlayer.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.NEGOTIATION_2, 0f));
					SetUpStoryNegotiationsEmote();
				}
				return sequentialVfxPlayer;
			};
			BattleEnemy.OnTurnStartBeforeDraw += callOneTime;
		}
		else
		{
			if (BattleEnemy.Turn != 2)
			{
				return;
			}
			callOneTime = delegate
			{
				SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
				if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_OTHER_PLAYER_EMOTE))
				{
					BattleEnemy.OnTurnStartBeforeDraw -= callOneTime;
					sequentialVfxPlayer.Register(BattleEnemy.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.NEGOTIATION_3, 0f));
					sequentialVfxPlayer.Register(new OpeningVfx.WaitVoiceEndVfx());
					sequentialVfxPlayer.Register(BattlePlayer.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.NEGOTIATION_3, 0f));
					SetUpStoryNegotiationsEmote();
				}
				return sequentialVfxPlayer;
			};
			BattleEnemy.OnTurnStartBeforeDraw += callOneTime;
		}
	}

	private void SetUpStoryPlayerTurnStartEmote()
	{
		Func<SkillProcessor, VfxBase> callOneTime = null;
		if (BattlePlayer.Turn == 0)
		{
			callOneTime = delegate
			{
				SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
				BattlePlayer.OnTurnStartBeforeDraw -= callOneTime;
				sequentialVfxPlayer.Register(BattlePlayer.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.PLAYER_TURN_START_1, 0f));
				sequentialVfxPlayer.Register(new OpeningVfx.WaitVoiceEndVfx());
				return sequentialVfxPlayer;
			};
			BattlePlayer.OnTurnStartBeforeDraw += callOneTime;
		}
	}

	private void SetUpStorySpecialBattle()
	{
		DataMgr.SpecialBattleSetting specialBattleSettingInfo = this.GameMgr.GetDataMgr().SpecialBattleSettingInfo;
		if (specialBattleSettingInfo != null)
		{
			_isPlayerFirstTurn = specialBattleSettingInfo.IsPlayerFirstTurn;
			SetUpAttachSkill(specialBattleSettingInfo.PlayerAttachSkills, specialBattleSettingInfo.EnemyAttachSkills);
			BattlePlayer.PpTotal = specialBattleSettingInfo.PlayerStartPp;
			BattleEnemy.PpTotal = specialBattleSettingInfo.EnemyStartPp;
			BattlePlayer.SetPpTotal(BattlePlayer.PpTotal, isUpdatePp: true, null);
			BattleEnemy.SetPpTotal(BattleEnemy.PpTotal, isUpdatePp: true, null);
		}
	}

	private List<AttachInfo> SetUpAttachSkill(string[] playerAttachSkills, string[] enemyAttachSkills)
	{
		List<AttachInfo> list = new List<AttachInfo>();
		for (int i = 0; i < playerAttachSkills.Length; i++)
		{
			AttachInfo attachInfo = AddAttachSkillToClass(isPlayer: true, playerAttachSkills[i]);
			if (attachInfo != null)
			{
				list.Add(attachInfo);
			}
		}
		for (int j = 0; j < enemyAttachSkills.Length; j++)
		{
			AttachInfo attachInfo2 = AddAttachSkillToClass(isPlayer: false, enemyAttachSkills[j]);
			if (attachInfo2 != null)
			{
				list.Add(attachInfo2);
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			AttachInfo attachInfo3 = list[k];
			SkillBase skillBase = Skill_attach_skill.CreateAndAttachSkill(attachInfo3._classCard, attachInfo3._attachSkill, attachInfo3._targetSkillBuildInfo);
			string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(skillBase.SkillPrm.buildInfo._effectPath, ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true);
			skillBase.SkillPrm.resourceMgr.LoadEffectBattle(assetTypePath, skillBase.SkillPrm.buildInfo._effectPath, skillBase.SkillPrm.buildInfo._sePath);
		}
		return list;
	}

	protected override void FirstRecoverySetting()
	{
		EmotionDataSetting();
	}

	private void EmotionDataSetting()
	{
		DataMgr dataMgr = this.GameMgr.GetDataMgr();
		if (dataMgr.m_BattleType != DataMgr.BattleType.Story)
		{
			if (dataMgr.m_BattleType == DataMgr.BattleType.Quest)
			{
				string playerEmotionId = ((dataMgr.QuestBattleData != null && dataMgr.QuestBattleData.PlayerEmotionOverride != 0) ? dataMgr.QuestBattleData.PlayerEmotionOverride.ToString() : string.Empty);
				dataMgr.SetPlayerEmotionId(playerEmotionId);
			}
			else if (dataMgr.m_BattleType == DataMgr.BattleType.BossRushQuest || dataMgr.m_BattleType == DataMgr.BattleType.SecretBossQuest)
			{
				string playerEmotionId2 = ((dataMgr.BossRushBattleData != null && dataMgr.BossRushBattleData.PlayerEmotionOverride != 0) ? dataMgr.BossRushBattleData.PlayerEmotionOverride.ToString() : string.Empty);
				dataMgr.SetPlayerEmotionId(playerEmotionId2);
			}
			else
			{
				dataMgr.SetPlayerEmotionId("");
			}
		}
		if (dataMgr.m_BattleType != DataMgr.BattleType.Story)
		{
			if (dataMgr.m_BattleType == DataMgr.BattleType.Quest)
			{
				string enemyEmotionId = ((dataMgr.QuestBattleData != null && dataMgr.QuestBattleData.EnemyEmotionOverride != 0) ? dataMgr.QuestBattleData.EnemyEmotionOverride.ToString() : string.Empty);
				dataMgr.SetEnemyEmotionId(enemyEmotionId);
			}
			else if (dataMgr.m_BattleType == DataMgr.BattleType.BossRushQuest || dataMgr.m_BattleType == DataMgr.BattleType.SecretBossQuest)
			{
				string enemyEmotionId2 = ((dataMgr.BossRushBattleData != null && dataMgr.BossRushBattleData.EnemyEmotionOverride != 0) ? dataMgr.BossRushBattleData.EnemyEmotionOverride.ToString() : string.Empty);
				dataMgr.SetEnemyEmotionId(enemyEmotionId2);
			}
			else
			{
				dataMgr.SetEnemyEmotionId("");
			}
		}
	}

	protected override int GetFirstAttack(int FirstAttack)
	{
		bool? didPlayerGoFirst = _contentsCreator.RecoveryManager.DidPlayerGoFirst;
		if (didPlayerGoFirst.HasValue)
		{
			if (!didPlayerGoFirst.Value)
			{
				return 1;
			}
			return 0;
		}
		if (_isPlayerFirstTurn.HasValue)
		{
			if (_isPlayerFirstTurn == true)
			{
				return 0;
			}
			return 1;
		}
		return base.GetFirstAttack(FirstAttack);
	}

	public override void SetupEnemyAI()
	{
		EnemyAI enemyAI = new SoloBattleEnemyAI(this);
		enemyAI.LoadBufferedBattleState();
		EnemyAI = enemyAI;
		BattleInfoReceiver = new AIBattleInfoReceiver(EnemyAI);
		EnemyAI.InitOnGame(BattleEnemy, BattlePlayer);
		if (!enemyAI.IsRankMatchAI)
		{
			enemyAI.EmoteCtrl().SetUpEmoteEvent(BattleEnemy, BattlePlayer, OperateMgr);
		}
		BattleEnemy.EnableEnemyAI = true;
	}

	public override void SetupInitialGameState(bool isPlayerFirstTurn, bool isRandomDraw, int playerMaxLife, int enemyMaxLife)
	{
		DataMgr dataMgr = this.GameMgr.GetDataMgr();
		enemyMaxLife = dataMgr.m_EnemyAIMaxLife;
		DataMgr.SpecialBattleSetting specialBattleSettingInfo = dataMgr.SpecialBattleSettingInfo;
		if ((dataMgr.m_BattleType == DataMgr.BattleType.Story || dataMgr.IsQuestBattleType()) && specialBattleSettingInfo != null)
		{
			if (dataMgr.m_BattleType != DataMgr.BattleType.BossRushQuest && dataMgr.m_BattleType != DataMgr.BattleType.SecretBossQuest)
			{
				playerMaxLife = specialBattleSettingInfo.PlayerStartMaxLife;
			}
			else
			{
				(BattlePlayer.Class as ClassBattleCardBase).BossRushStartLife = specialBattleSettingInfo.PlayerStartMaxLife;
			}
			enemyMaxLife = specialBattleSettingInfo.EnemyStartMaxLife;
		}
		if (dataMgr.m_BattleType == DataMgr.BattleType.Practice && Data.CurrentFormat == Format.Avatar)
		{
			dataMgr.SetPlayerAvatarBattleInfo(dataMgr.GetPlayerCharaId().ToString());
			dataMgr.ClearEnemyAvatarBattleInfo();
		}
		base.SetupInitialGameState(isPlayerFirstTurn, isRandomDraw, playerMaxLife, enemyMaxLife);
		if ((dataMgr.m_BattleType == DataMgr.BattleType.Story || dataMgr.IsQuestBattleType()) && specialBattleSettingInfo != null)
		{
			if ((dataMgr.m_BattleType == DataMgr.BattleType.BossRushQuest || dataMgr.m_BattleType == DataMgr.BattleType.SecretBossQuest) && specialBattleSettingInfo.PlayerStartMaxLife > playerMaxLife)
			{
				BattlePlayer.Class.SkillApplyInformation.GiveCombatValueModifier(null, new MaxLifeSetModifier(specialBattleSettingInfo.PlayerStartMaxLife), null);
				BattlePlayer.Class.ApplyHealing(new BattleCardBase.HealParam(specialBattleSettingInfo.PlayerStartMaxLife - BattlePlayer.Class.Life, BattlePlayer.Class, BattlePlayer.Class), null);
			}
			if (specialBattleSettingInfo.PlayerStartLife < specialBattleSettingInfo.PlayerStartMaxLife)
			{
				BattlePlayer.Class.ApplyDamage(null, new BattleCardBase.DamageParam(specialBattleSettingInfo.PlayerStartMaxLife - specialBattleSettingInfo.PlayerStartLife, BattlePlayer.Class), doesAttackerPossessKiller: false, isReflectedDamage: false, null, null);
			}
		}
	}

	public override void FinishBattle()
	{
		this.GameMgr.GetDataMgr().ClearSpecialBattleSettingInfo();
		EnemyAI.StopEnemyAI();
	}

	public override void DisposeBattleGameObj()
	{
		base.DisposeBattleGameObj();
		BattleCoroutine.GetInstance().StopAllCoroutines();
		EnemyAICoroutine.GetInstance().StopAllCoroutines();
	}

	public void RecordChangeAI(string logicName, int operationQueueCount)
	{
		if (_contentsCreator.RecoveryRecordManager is SingleBattleRecoveryRecordManager singleBattleRecoveryRecordManager)
		{
			singleBattleRecoveryRecordManager.RecordChangeAI(logicName, operationQueueCount);
		}
	}

	public override void PlayRetire()
	{
		if (RecoveryRecordManagerBase.IsExistsSingleRecoveryFile() && this.GameMgr.GetDataMgr().BossRushBattleData == null)
		{
			this.GameMgr.GetDataMgr().SetRecoveryData(RecoveryOperationInfo.ReadRecoveryFile(OperationRecorderBase.RecordDirectoryPath + "recovery_single.json"));
			RecoveryRecordManagerBase.DeleteRecoveryFile();
		}
		base.PlayRetire();
		this.OnBattleRetire.Call();
	}

	public override void Update(float dt)
	{
		base.Update(dt);
		LifeZeroActivateLeonSkillIfNeeded();
	}

	public void LifeZeroActivateLeonSkillIfNeeded()
	{
		if (!BattleEnemy.Class.SkillApplyInformation.IsLifeZeroActivateLeonSkill)
		{
			return;
		}
		bool flag = false;
		int num = BattleEnemy.Class.BaseMaxLife;
		int num2 = BattleEnemy.Class.BaseMaxLife;
		int count = BattleEnemy.Class.SkillApplyInformation.LifeModifierList.Count;
		for (int i = 0; i < count; i++)
		{
			ICardLifeModifier cardLifeModifier = BattleEnemy.Class.SkillApplyInformation.LifeModifierList[i];
			num2 = cardLifeModifier.CalcMaxLife(num2);
			num = cardLifeModifier.CalcLife(num);
			num = Math.Min(num, num2);
			if (num <= 0)
			{
				flag = true;
				break;
			}
		}
		if (BattleEnemy.Class.Life <= 0 || flag)
		{
			base.VfxMgr.RegisterSequentialVfx(((ClassBattleCardBase)BattleEnemy.Class).LifeZeroActivateLeonSkill());
		}
	}
}
