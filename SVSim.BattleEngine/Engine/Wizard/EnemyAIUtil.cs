using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle;

namespace Wizard;

internal static class EnemyAIUtil
{
	private class SelectableInfo
	{
		public List<BattleCardBase> Cards;

		public int Count;

		public SelectableInfo(List<BattleCardBase> cards, int count)
		{
			Cards = cards;
			Count = count;
		}
	}

	private static void SetupTargetsList(int depth, List<BattleCardBase> currentList, List<SelectableInfo> selectablesList, Dictionary<int, List<SelectableInfo>> choiceSelectableList, List<List<BattleCardBase>> out_targetsList, bool isFusion = false)
	{
		if (depth == selectablesList.Count)
		{
			out_targetsList.Add(new List<BattleCardBase>(currentList));
			return;
		}
		SelectableInfo selectableInfo = selectablesList[depth];
		if (isFusion)
		{
			int num = (1 << selectableInfo.Cards.Count) - 1;
			for (int i = 1; i <= num; i++)
			{
				int num2 = i;
				int num3 = 0;
				for (int j = 0; j < selectableInfo.Cards.Count; j++)
				{
					BattleCardBase item = selectableInfo.Cards[j];
					if (((num2 >> j) & 1) > 0)
					{
						currentList.Add(item);
						num3++;
					}
				}
				SetupTargetsList(depth + 1, currentList, selectablesList, choiceSelectableList, out_targetsList, isFusion: true);
				for (int k = 0; k < num3; k++)
				{
					currentList.RemoveAt(currentList.Count - 1);
				}
			}
			return;
		}
		List<int> list = new List<int>();
		for (int l = 0; l < selectableInfo.Cards.Count; l++)
		{
			list.Add(l);
		}
		foreach (int[] item2 in AIMathematicsLibrary.EnumerateCombinations(list, selectableInfo.Count))
		{
			BattleCardBase firstCard = selectableInfo.Cards[item2[0]];
			bool flag = true;
			for (int m = 0; m < item2.Length; m++)
			{
				BattleCardBase card = selectableInfo.Cards[item2[m]];
				if (currentList.Any((BattleCardBase c) => c.Index == card.Index && c.IsPlayer == card.IsPlayer))
				{
					flag = false;
				}
				currentList.Add(card);
			}
			if (flag)
			{
				if (choiceSelectableList != null && choiceSelectableList.Any((KeyValuePair<int, List<SelectableInfo>> p) => p.Key == firstCard.Index))
				{
					if (choiceSelectableList[firstCard.Index] != null)
					{
						SetupTargetsList(0, currentList, choiceSelectableList[firstCard.Index], null, out_targetsList);
					}
				}
				else
				{
					SetupTargetsList(depth + 1, currentList, selectablesList, choiceSelectableList, out_targetsList);
				}
			}
			for (int num4 = 0; num4 < item2.Length; num4++)
			{
				currentList.RemoveAt(currentList.Count - 1);
			}
		}
	}

	public static void TurnEnd(BattleManagerBase mgr, bool isPlayer)
	{
		if (isPlayer)
		{
			mgr.VfxMgr.RegisterSequentialVfx(mgr.OperateMgr.PlayerTurnEnd());
		}
		else
		{
			mgr.VfxMgr.RegisterSequentialVfx(mgr.OperateMgr.TurnEndOperation(isPlayer));
		}
	}

	public static void SetupPlayCardSkillOptionValue(BattleCardBase playCard, BattlePlayerPair pair)
	{
		IEnumerable<SkillBase> selectTypeSkill = playCard.GetSelectTypeSkill();
		if (selectTypeSkill == null)
		{
			return;
		}
		foreach (SkillBase item in selectTypeSkill)
		{
			SkillCollectionBase.SetupOptionValue(item.OptionValue, pair, playCard, item);
		}
	}
}
