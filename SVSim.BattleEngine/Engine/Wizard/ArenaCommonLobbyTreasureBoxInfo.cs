using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class ArenaCommonLobbyTreasureBoxInfo : MonoBehaviour
{
	[Header("現在の宝箱")]
	[SerializeField]
	private GameObject _nowBoxTitleRoot;

	[SerializeField]
	private UISprite _nowBoxSprite;

	[SerializeField]
	private Transform _movePos;

	[Header("次の宝箱")]
	[SerializeField]
	private GameObject _nextBoxRoot;

	[SerializeField]
	private UISprite _nextBoxSprite;

	private GameObject _openEffect;

	private Vector3 _backupNowBoxPos = Vector3.zero;

	private bool _backupNextBoxRootActive;

	public ArenaCommonLobbyLoadRequest Init(ArenaCommonLobbyInitParam initParam, List<string> unloadAssetList)
	{
		int battleWinNum = initParam.BattleWinNum;
		_nowBoxSprite.spriteName = $"box_2pick_{battleWinNum:D2}_close";
		bool flag = initParam.BattleResultList.Length < initParam.BattleMaxNum;
		_nextBoxRoot.SetActive(flag);
		if (flag)
		{
			_nextBoxSprite.spriteName = $"box_2pick_{battleWinNum + 1:D2}_close";
		}
		ArenaCommonLobbyLoadRequest arenaCommonLobbyLoadRequest = new ArenaCommonLobbyLoadRequest();
		string effectName = $"cmn_arena_treasure_{battleWinNum + 1}";
		arenaCommonLobbyLoadRequest.LoadAssetList.Add(Toolbox.ResourcesManager.GetAssetTypePath(effectName, ResourcesManager.AssetLoadPathType.Effect2D));
		arenaCommonLobbyLoadRequest.LoadEndCallback = delegate
		{
			_openEffect = EffectUtility.CreateEffect2D(new Effect2dCreateParam
			{
				Parent = _nowBoxSprite.gameObject,
				EffectName = effectName,
				InitActive = false,
				UnloadAssetList = unloadAssetList
			});
		};
		return arenaCommonLobbyLoadRequest;
	}

	public void OpenBox(Action animationEndCallback = null)
	{
		StartCoroutine(OpenBoxCoroutine(animationEndCallback));
	}

	private IEnumerator OpenBoxCoroutine(Action animationEndCallback)
	{

		_backupNowBoxPos = _nowBoxSprite.transform.position;
		_backupNextBoxRootActive = _nextBoxRoot.activeSelf;
		_nowBoxTitleRoot.SetActive(value: false);
		_nextBoxRoot.SetActive(value: false);
		iTween.MoveTo(_nowBoxSprite.gameObject, iTween.Hash("position", _movePos.position, "time", 0.5f, "islocal", false, "easetype", iTween.EaseType.easeOutExpo));
		yield return new WaitForSeconds(0.8f);
		_nowBoxSprite.enabled = false;
		_openEffect.SetActive(value: true);
		yield return new WaitForSeconds(1.5f);
		animationEndCallback.Call();
	}

	public void Reset()
	{
		_nowBoxTitleRoot.SetActive(value: true);
		_nowBoxSprite.enabled = true;
		_nowBoxSprite.transform.position = _backupNowBoxPos;
		_nextBoxRoot.SetActive(_backupNextBoxRootActive);
		_openEffect.SetActive(value: false);
	}
}
