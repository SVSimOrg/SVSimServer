using System;
using System.Collections.Generic;

public class NetworkBattleDefine
{
	public enum PlayActionType
	{
		NONE = 0,
		ATTACK = 10,
		EVOLUTION = 20,
		EVOLUTION_SELECT = 21,
		PLAY_HAND = 30,
		PLAY_HAND_SELECT = 31,
		FUSION = 40
	}

	public enum NetworkCardPlaceState
	{
		Deck = 0,
		Hand = 10,
		Field = 20,
		Cemetery = 30,
		Banish = 40,
		None = 50,
		FusionIngredient = 60,
		Riding = 70,
		Reservation = 80,
		Unite = 90,
		BlackHole = 999
	}

	public enum NetworkBattleURI
	{
		None,
		Retry,
		Loaded,
		Deal,
		Swap,
		Ready,
		TurnStart,
		TurnEndActions,
		TurnEnd,
		TurnEndFinal,
		PlayActions,
		BattleFinish,
		ChatStamp,
		Echo,
		Retire,
		OppoDisconnect,
		End,
		Judge,
		Touch,
		SelectSkill,
		SelectObject,
		SlideObject,
		TurnEndReady,
		RecoveryStart,
		RecoveryEnd,
		JudgeResult,
		Maintenance,
		ReplayFinish,
		Watch
	}

	public enum NetworkParameter
	{
		playIdx,
		idx,
		idxList,
		isSelf,
		type,
		targetIdx,
		skillIndex,
		selectSkillIndex,
		targetList,
		from,
		to,
		cardId,
		cost,
		is_open,
		addAtk,
		setAtk,
		addLife,
		setLife,
		clan,
		tribe,
		knownList,
		oppoTargetList,
		stamp,
		chatStamp,
		key,
		skill,
		activate,
		log,
		endType,
		result,
		targetUri,
		uList,
		keyAction,
		skillKeyCardIdx,
		skillIdx,
		skillCount,
		skillTarget,
		count,
		param,
		attachTarget,
		isFlood,
		highlander,
		spellboost,
		addChantCount,
		setChantCount,
		callCount,
		unionburst,
		skyboundArt,
		randomTargetIdx,
		idxChangeSeed,
		oppoIdxChangeSeed,
		spin,
		resultCode,
		isWin,
		pos,
		self,
		oppo,
		time,
		cards,
		touch,
		value,
		vid,
		finishData,
		fusion,
		isInvoke,
		notBuff,
		hasGuard,
		byOppo
	}

	public enum ReceiveNodeResultCode
	{
		None = 0,
		CurrentBattleError = 30212	}

	public static readonly Dictionary<NetworkBattleURI, string> NetworkURINames;

	public static readonly Dictionary<NetworkParameter, string> NetworkParameterNames;

	static NetworkBattleDefine()
	{
		NetworkURINames = new Dictionary<NetworkBattleURI, string>(Enum.GetValues(typeof(NetworkBattleURI)).Length);
		foreach (NetworkBattleURI value in Enum.GetValues(typeof(NetworkBattleURI)))
		{
			NetworkURINames[value] = Enum.GetName(typeof(NetworkBattleURI), value);
		}
		NetworkParameterNames = new Dictionary<NetworkParameter, string>(Enum.GetValues(typeof(NetworkParameter)).Length);
		foreach (NetworkParameter value2 in Enum.GetValues(typeof(NetworkParameter)))
		{
			NetworkParameterNames[value2] = Enum.GetName(typeof(NetworkParameter), value2);
		}
	}
}
