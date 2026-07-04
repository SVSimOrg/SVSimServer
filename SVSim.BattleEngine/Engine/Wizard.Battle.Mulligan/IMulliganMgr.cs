using System;
using System.Collections.Generic;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Mulligan;

public interface IMulliganMgr
{
	Action OnSubmit { get; set; }

	PlayerMulliganCtrl PlayerMlgCtrl { get; }

	OpponentMulliganCtrl OpponentMlgCtrl { get; }

	VfxBase StartDeal(List<int> playerDealIdxList, List<int> oppoDealIdxList, SkillProcessor skillProcessor);

	VfxBase MulliganStartDraw(bool firstAttack, SkillProcessor skillProcessor);

	VfxBase Submit(BattleManagerBase m_BtlMgrIns);

	VfxBase EnemyChangeCardVfx(BattleManagerBase btlMgrIns);

	VfxBase CompleteMulligan(BattleManagerBase m_BtlMgrIns);

	VfxBase InitMulligan(BattleManagerBase mgr, MulliganInfoControl mulliganInfo, IPlayerView view);

	VfxBase RecoverMulligan(bool didPlayerSubmitMulligan, BattleManagerBase battleMgr);

	MulliganInfoControl GetMulliganInfo();
}
