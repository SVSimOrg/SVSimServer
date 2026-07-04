using System;
using Cute;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Phase;

public class ResultPhase : IResultPhase, IPhase
{
	private readonly BattleManagerBase _battleMgr;

	private readonly bool _winnerIsPlayer;

	public event Action OnSetupEnd;

	public ResultPhase(BattleManagerBase battleMgr, bool winnerIsPlayer)
	{
		_battleMgr = battleMgr;
		_winnerIsPlayer = winnerIsPlayer;
	}

	public virtual VfxBase Setup()
	{
		_battleMgr.TurnPanelControl.GameObject.SetActive(value: false);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		DataMgr dataMgr = _battleMgr.GameMgr.GetDataMgr();
		if (dataMgr.m_BattleType == DataMgr.BattleType.BossRushQuest && !_battleMgr.BattleResultControl.AlreadyResultRecovery)
		{
			_battleMgr.BattleResultControl.AlreadyResultRecovery = true;
			ClassBattleCardBase classCard = _battleMgr.BattlePlayer.Class as ClassBattleCardBase;
			VfxBase vfx = (classCard.SkillApplyInformation as ClassSkillApplyInformation).GiveCombatValueModifier(null, new MaxLifeSetModifier(classCard.BossRushStartLife), null);
			sequentialVfxPlayer.Register(vfx);
			if (_winnerIsPlayer && dataMgr.BossRushBattleData != null && dataMgr.BossRushBattleData.RecoveryPointWhenFinish > 0)
			{
				EffectBattle effectBattle = null;
				SkillBase.WaitEffectLoadVfx loadingVfx = new SkillBase.WaitEffectLoadVfx("btl_magic_cure_2", EffectMgr.EngineType.SHURIKEN, "se_btl_magic_cure_2", classCard.ResourceMgr, delegate(EffectBattle eb)
				{
					effectBattle = eb;
				});
				VfxBase mainVfx = NullVfx.GetInstance();
				VfxWithLoading vfxWithLoading = VfxWithLoading.Create(loadingVfx, mainVfx);
				BattleCardBase.HealParam healParam = new BattleCardBase.HealParam(dataMgr.BossRushBattleData.RecoveryPointWhenFinish, classCard, classCard);
				BattleCardBase.HealResult healResult = classCard.ApplyHealing(healParam, null);
				classCard.SkillApplyInformation.AddSkillHealValue(healResult.HealAmount);
				VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(healResult.PrehealVfxVfx, vfxWithLoading.MainVfx, healResult.HealVfx, healResult.PosthealVfxVfx);
				vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
				sequentialVfxPlayer.Register(vfxWithLoadingSequential);
			}
		}
		sequentialVfxPlayer.Register(WaitVfx.Create(1f));
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			_battleMgr.MenuButtonObject.SetActive(value: false);
			if (!_battleMgr.BattleResultControl.IsResultOn)
			{
				_battleMgr.BattlePlayer.PlayerBattleView.AllClear();
				_battleMgr.BackGround.SetShaderGlobalColorBG.ChangeGlobalShaderColorFadeIn();
				_battleMgr.isStorySuccessful = (_winnerIsPlayer ? 1 : 0);
				_battleMgr.DetailMgr.DetailPanelControl.Hide();
				_battleMgr.BattlePlayer.PlayerBattleView.TurnEndButtonUI.HideBtn();
				_battleMgr.BattleResultControl.StartUI(_winnerIsPlayer, _battleMgr.Camera);
				_battleMgr.BattlePlayer.StatusPanelControl.HideUI();
				_battleMgr.BattleEnemy.StatusPanelControl.HideUI();
				_battleMgr.BattlePlayer.PlayerBattleView.TurnEndButtonUI.HideAnimation();
				_battleMgr.BattlePlayer.PlayerBattleView.HideChoiceBraveButton();
				_battleMgr.BattleEnemy.BattleEnemyView.HideChoiceBraveButton();

				this.OnSetupEnd.Call();
			}
		}));
		sequentialVfxPlayer.Register(ParallelVfxPlayer.Create(_battleMgr.BattlePlayer.BattleView.HandView.HandFocus(), _battleMgr.BattleEnemy.BattleView.HandView.HandFocus()));
		return sequentialVfxPlayer;
	}

	public VfxWith<IPhase> Update(float dt)
	{
		return new VfxWith<IPhase>(NullVfx.GetInstance(), null);
	}

	public VfxBase Teardown()
	{
		return NullVfx.GetInstance();
	}

	public void Pause()
	{
	}
}
