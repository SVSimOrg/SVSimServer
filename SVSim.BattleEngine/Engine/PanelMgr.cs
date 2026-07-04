using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Wizard;

public class PanelMgr : MonoBehaviour
{
	public enum BattleAlertType
	{
		SelectChoiceCard,
		DisconnectInfomation,
		None
	}

	private BattleManagerBase _battleMgr;

	private void Awake()
	{
		_battleMgr = _battleMgr;
	}
}
