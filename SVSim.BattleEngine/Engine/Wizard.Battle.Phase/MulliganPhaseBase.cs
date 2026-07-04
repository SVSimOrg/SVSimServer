using System;
using System.Collections.Generic;
using Wizard.Battle.Mulligan;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 9 of 12 methods unrun in baseline
//   Type: Wizard.Battle.Phase.MulliganPhaseBase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Phase;

public class MulliganPhaseBase : IPhase
{
	protected IMulliganMgr _mulliganMgr;

	private readonly BattleManagerBase _battleMgr;

	private MulliganOperateControl _mulliganOperateCtrl;

	private bool _enableTouch;

	protected MulliganPhaseBase(BattleManagerBase battleMgr)
	{
		_battleMgr = battleMgr;
	}

	protected void Initialize(IMulliganMgr mulliganMgr)
	{
		_mulliganMgr = mulliganMgr;
		_battleMgr.MulliganMgr = _mulliganMgr;
		MulliganInfoControl component = NGUITools.AddChild(_battleMgr.Battle3DContainer, _battleMgr.GameMgr.GetPrefabMgr().Get("Prefab/UI/MulliganInfo")).GetComponent<MulliganInfoControl>();
		_mulliganMgr.InitMulligan(_battleMgr, component, _battleMgr.BattlePlayer.PlayerBattleView);
		_mulliganOperateCtrl = CreateMulliganOperateControl();
	}

	protected virtual MulliganOperateControl CreateMulliganOperateControl()
	{
		return new MulliganOperateControl(_mulliganMgr.PlayerMlgCtrl);
	}

	protected virtual void ShowMulliganInfo()
	{
		_mulliganMgr.GetMulliganInfo().Show(MulliganInfoControl.ViewType.Normal);
	}

	public virtual VfxBase Setup()
	{
		IMulliganMgr mulliganMgr = _mulliganMgr;
		mulliganMgr.OnSubmit = (Action)Delegate.Combine(mulliganMgr.OnSubmit, (Action)delegate
		{
			_enableTouch = false;
		});
		_battleMgr.OnSubmitMulligan += SubmitMulligan;
		VfxBase vfxBase = _mulliganMgr.MulliganStartDraw(_battleMgr.IsFirst, new SkillProcessor());
		_battleMgr.BattleUIContainer.Battery.SetActive(value: false);
		_enableTouch = true;
		return SequentialVfxPlayer.Create(InstantVfx.Create(ShowMulliganInfo), vfxBase);
	}

	protected void StartDeal(List<int> playerDealIdxList, List<int> oppoDealIdxList)
	{
		if (playerDealIdxList != null && playerDealIdxList.Count == 3 && oppoDealIdxList != null && oppoDealIdxList.Count == 3)
		{
			_battleMgr.VfxMgr.RegisterSequentialVfx(_mulliganMgr.StartDeal(playerDealIdxList, oppoDealIdxList, new SkillProcessor()));
		}
	}

	public virtual VfxWith<IPhase> Update(float dt)
	{
		if (_enableTouch)
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			parallelVfxPlayer.Register(_mulliganOperateCtrl.Update());
			return new VfxWith<IPhase>(parallelVfxPlayer, null);
		}
		return new VfxWith<IPhase>(NullVfx.GetInstance(), null);
	}

	public virtual VfxBase Teardown()
	{
		_battleMgr.OnSubmitMulligan -= SubmitMulligan;
		MulliganInfoControl mulliganInfo = _mulliganMgr.GetMulliganInfo();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(mulliganInfo.SetPlayerReady());
		sequentialVfxPlayer.Register(InstantVfx.Create(mulliganInfo.HideTopPanels));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(mulliganInfo.DestroyMulliganUIVfx());
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			_battleMgr.MulliganMgr = null;
			_battleMgr.BattleUIContainer.Battery.SetActive(value: true);
		}));
		return sequentialVfxPlayer;
	}

	public void Pause()
	{
	}

	private VfxBase SubmitMulligan()
	{
		if (IsCardUndecided())
		{
			return NullVfx.GetInstance();
		}
		return _mulliganMgr.Submit(_battleMgr);
	}

	private bool IsCardUndecided()
	{
		return _mulliganOperateCtrl.State == MulliganOperateControl.STATE.CARD_SELECTED;
	}
}
