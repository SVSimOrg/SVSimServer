using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Mulligan;

public class SingleMulliganMgr : MulliganMgrBase
{
	public override VfxBase Submit(BattleManagerBase battleManager)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(base.Submit(battleManager));
		if (battleManager.GameMgr.IsAINetwork && !battleManager.IsRecovery)
		{
			sequentialVfxPlayer.Register(parallelVfxPlayer);
			return sequentialVfxPlayer;
		}
		parallelVfxPlayer.Register(PlayerChangeCardVfx(battleManager));
		parallelVfxPlayer.Register(EnemyChangeCardVfx(battleManager));
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		sequentialVfxPlayer.Register(CompleteMulligan(battleManager));
		return sequentialVfxPlayer;
	}

	public override VfxBase InitMulligan(BattleManagerBase mgr, MulliganInfoControl mulliganInfo, IPlayerView view)
	{
		return base.InitMulligan(mgr, mulliganInfo, view);
	}

	public void AIMulliganEndAction(BattleManagerBase ins)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		parallelVfxPlayer.Register(PlayerChangeCardVfx(ins));
		parallelVfxPlayer.Register(EnemyChangeCardVfx(ins));
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		sequentialVfxPlayer.Register(CompleteMulligan(ins));
		ins.VfxMgr.RegisterSequentialVfx(sequentialVfxPlayer);
	}

	public override VfxBase EnemyChangeCardVfx(BattleManagerBase btlMgrIns)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		btlMgrIns.EnemyAI.Mulligan(list, btlMgrIns.BattleEnemy, btlMgrIns.BattlePlayer);
		VfxBase result = _opponentMulliganControl.SubmitMulliganVfx(list);
		btlMgrIns.BattleEnemy.CallRecordingMulligan(list, btlMgrIns.BattleEnemy.HandCardList.Select((BattleCardBase c) => c.Index).ToList());
		return result;
	}

	public override VfxBase MulliganStartDraw(bool firstAttack, SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(base.MulliganStartDraw(firstAttack, skillProcessor));
		_opponentMulliganControl.GetBattlePlayer().DrawCards(_opponentMulliganControl.GetFirstDrawList(), skillProcessor, isOpen: false, isMulligan: true);
		return sequentialVfxPlayer;
	}

	public override VfxBase CompleteMulligan(BattleManagerBase battleMgr)
	{
		VfxBase vfx = base.CompleteMulligan(battleMgr);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(vfx);
		sequentialVfxPlayer.Register(battleMgr.StartBattle());
		return sequentialVfxPlayer;
	}
}
