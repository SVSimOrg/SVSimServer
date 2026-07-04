using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Wizard.Battle;
using Wizard.Battle.Operation;

namespace Wizard.AutoTest;

public static class AutoTestBattleMgr
{
	public class CardInfo : IBattleCardUniqueID
	{
		public string Name { get; private set; }

		public bool IsPlayer { get; private set; }

		public int Index { get; private set; }

		public int CardId { get; private set; }

		public int Cost { get; private set; }

		public int AddAtk { get; set; }

		public int AddLife { get; set; }

		public int ChangeClan { get; set; }

		public CardInfo(string cardName, int cardId = 0, int cost = -1, int addAtk = 0, int addLife = 0, int clan = 0)
		{
			Name = cardName;
			IsPlayer = cardName[0] == 'p';
			Index = int.Parse(cardName.Substring(1));
			CardId = cardId;
			Cost = cost;
			AddAtk = addAtk;
			AddLife = addLife;
			ChangeClan = clan;
		}

		public CardInfo(IReadOnlyBattleCardInfo cardInfo)
		{
			IsPlayer = cardInfo.IsPlayer;
			Index = cardInfo.Index;
			Name = cardInfo.GetName();
			CardId = cardInfo.CardId;
			Cost = cardInfo.Cost;
		}
	}

	public static IEnumerable<IOperationCommand> CreateOperationCommands(IEnumerable<JsonData> actionJsonDatas, bool startTurnIsPlayer)
	{
		bool currentTurnIsPlayer = startTurnIsPlayer;
		foreach (JsonData actionJsonData in actionJsonDatas)
		{
			string text = actionJsonData["ope"].ToString();
			switch (text)
			{
			case "play":
				yield return new PlayOperationCommand(actionJsonData);
				break;
			case "attack":
				yield return new AttackOperationCommand(actionJsonData);
				break;
			case "evolve":
				yield return new EvolveOperationCommand(actionJsonData);
				break;
			case "turn_end":
				yield return new TurnEndOperationCommand(currentTurnIsPlayer);
				currentTurnIsPlayer = !currentTurnIsPlayer;
				break;
			case "change_ai":
				yield return new ChangeAIOperationCommand(actionJsonData);
				break;
			case "comp_fusion":
				yield return new FusionOperationCommand(actionJsonData);
				break;
			default:
				Debug.LogError("Auto test \"" + text + "\" is not supported.");
				break;
			}
		}
	}
}
