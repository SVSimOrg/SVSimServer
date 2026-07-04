using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class ArenaCommonLobbyBattleInfo : MonoBehaviour
{
	[SerializeField]
	private UIGrid _battleStateObjectsGrid;

	[SerializeField]
	private ArenaCommonLobbyBattleStateObject _battleStateObjectOriginal;

	[SerializeField]
	private UISprite _battleStateObjectsLine;

	[SerializeField]
	private UILabel _winNumLabel;

	public ArenaCommonLobbyLoadRequest Init(ArenaCommonLobbyInitParam initParam, List<string> unloadAssetList)
	{
		int maxNum = initParam.BattleMaxNum;
		bool[] battleResultList = initParam.BattleResultList;
		int num = battleResultList.Length;
		ArenaCommonLobbyBattleStateObject[] stateObjectList = new ArenaCommonLobbyBattleStateObject[maxNum];
		for (int i = 0; i < maxNum; i++)
		{
			ArenaCommonLobbyBattleStateObject component = NGUITools.AddChild(_battleStateObjectsGrid.gameObject, _battleStateObjectOriginal.gameObject).GetComponent<ArenaCommonLobbyBattleStateObject>();
			stateObjectList[i] = component;
			ArenaCommonLobbyBattleStateObject.eState state = ArenaCommonLobbyBattleStateObject.eState.None;
			if (i < num)
			{
				state = (battleResultList[i] ? ArenaCommonLobbyBattleStateObject.eState.Won : ArenaCommonLobbyBattleStateObject.eState.Lost);
			}
			else if (i == num)
			{
				state = ArenaCommonLobbyBattleStateObject.eState.Next;
			}
			component.ChangeState(state);
			component.SetTitleText(Data.SystemText.Get("Common_0103", (i + 1).ToString()));
		}
		_battleStateObjectsGrid.cellWidth = _battleStateObjectsLine.width / (maxNum - 1);
		_battleStateObjectsGrid.Reposition();
		int battleWinNum = initParam.BattleWinNum;
		_winNumLabel.text = battleWinNum.ToString();
		ArenaCommonLobbyLoadRequest arenaCommonLobbyLoadRequest = new ArenaCommonLobbyLoadRequest();
		bool loadExists = false;
		if (battleWinNum > 0)
		{
			arenaCommonLobbyLoadRequest.LoadAssetList.Add(Toolbox.ResourcesManager.GetAssetTypePath("cmn_ui_orb_1", ResourcesManager.AssetLoadPathType.Effect2D));
			loadExists = true;
		}
		if (num - battleWinNum > 0)
		{
			arenaCommonLobbyLoadRequest.LoadAssetList.Add(Toolbox.ResourcesManager.GetAssetTypePath("cmn_ui_orb_2", ResourcesManager.AssetLoadPathType.Effect2D));
			loadExists = true;
		}
		arenaCommonLobbyLoadRequest.LoadEndCallback = delegate
		{
			if (loadExists)
			{
				for (int j = 0; j < maxNum; j++)
				{
					ArenaCommonLobbyBattleStateObject arenaCommonLobbyBattleStateObject = stateObjectList[j];
					bool isWon = arenaCommonLobbyBattleStateObject.IsWon;
					if (isWon || arenaCommonLobbyBattleStateObject.IsLost)
					{
						EffectUtility.CreateEffect2D(new Effect2dCreateParam
						{
							Parent = arenaCommonLobbyBattleStateObject.gameObject,
							EffectName = (isWon ? "cmn_ui_orb_1" : "cmn_ui_orb_2"),
							ColorCode = (isWon ? eColorCodeId.WIN_ORB_EFFECT_COLOR : eColorCodeId.LOSE_ORB_EFFECT_COLOR),
							InitActive = true,
							UnloadAssetList = unloadAssetList
						});
					}
				}
			}
		};
		return arenaCommonLobbyLoadRequest;
	}
}
