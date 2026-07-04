using System;
using UnityEngine;
using Wizard;
using Wizard.Battle.Phase;

public class UnityEventAgent : MonoBehaviour
{
	private BattleManagerBase m_battleMgr;

	public bool HasFocus { get; private set; }

	public UnityEventAgent()
	{
		HasFocus = true;
	}

	public void SetBattleMgr(BattleManagerBase battleMgr)
	{
		m_battleMgr = battleMgr;
	}
}
