using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Wizard.Battle.Resource;
using Wizard.Battle.View.Vfx;

public class StartPickMultiCardVfx : SequentialVfxPlayer
{
	private readonly IBattleResourceMgr _resourceMgr;

	public StartPickMultiCardVfx(SkillBaseSummon.SummonedCardsList summonedCardsList, IBattleResourceMgr resourceMgr, bool isPlayer = true, bool isToken = false, bool isIgnoreVoice = false, bool isRandomVoice = false, bool isGetoff = false, bool isEvoVoice = false, float voiceWaitTime = -1f, List<NetworkBattleReceiver.CardInfo> cardInfoList = null, bool isSeSysSummonLandingDuplicateCheck = false)
	{
		_resourceMgr = resourceMgr;
		Register(InstantVfx.Create(delegate
		{
			foreach (BattleCardBase summonedCards in summonedCardsList)
			{
				summonedCards.BattleCardView.Collider.center = new Vector3(0f, 0f, -0.1f);
			}
		}));
		Register(CreateSummonCardHandToInPlayVfx(summonedCardsList, isPlayer, isToken, isIgnoreVoice, isRandomVoice, cardInfoList, isSeSysSummonLandingDuplicateCheck, isGetoff, isEvoVoice, voiceWaitTime));
	}

	private VfxBase CreateSummonCardHandToInPlayVfx(SkillBaseSummon.SummonedCardsList summonedCardsList, bool isPlayer, bool isToken, bool isIgnoreVoice, bool isRandomVoice, List<NetworkBattleReceiver.CardInfo> cardInfoList, bool isSeSysSummonLandingDuplicateCheck, bool isGetoff = false, bool isEvoVoice = false, float voiceWaitTime = -1f)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		ReadOnlyCollection<SkillBaseSummon.SummonedCardsList.CardEffectPair> summonedCardEffectPairList = summonedCardsList.summonedCardEffectPairList;
		ReadOnlyCollection<SkillBaseSummon.SummonedCardsList.CardEffectPair> overflowCardEffectPairList = summonedCardsList.overflowCardEffectPairList;
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		SkillBaseSummon.SummonedCardsList.CardEffectPair cardEffectPair = summonedCardEffectPairList.FirstOrDefault();
		int num = (isRandomVoice ? new System.Random().Next(summonedCardsList.summonedCards.Count()) : (-1));
		for (int i = 0; i < summonedCardEffectPairList.Count; i++)
		{
			SkillBaseSummon.SummonedCardsList.CardEffectPair cardEffectPair2 = summonedCardEffectPairList[i];
			bool playVoice = !isIgnoreVoice && isToken && cardEffectPair2 == cardEffectPair;
			if (!isIgnoreVoice && num != -1 && num == i)
			{
				playVoice = true;
			}
			else if (num != -1)
			{
				playVoice = false;
			}
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		for (int j = 0; j < overflowCardEffectPairList.Count; j++)
		{
			SkillBaseSummon.SummonedCardsList.CardEffectPair cardEffectPair3 = overflowCardEffectPairList[j];
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		for (int k = 0; k < summonedCardsList.summonedCards.Count(); k++)
		{
			BattleCardBase battleCardBase = summonedCardsList.summonedCards.ElementAt(k);
			if (cardInfoList != null)
			{
				if (battleCardBase.HasStackWhiteRitualAndOtherIconSkill())
				{
					parallelVfxPlayer.Register(battleCardBase.BattleCardView.InitializeBattleCardStackIcon(battleCardBase, battleCardBase.Skills));
				}
				else
				{
					parallelVfxPlayer.Register(battleCardBase.BattleCardView.BattleCardIconAnimations.UpdateSkillIconInReplay(cardInfoList[k].InplaySkillEffectList, cardInfoList[k].InductionNumber, isInitialize: true));
				}
			}
			else if (battleCardBase.HasStackWhiteRitualAndOtherIconSkill())
			{
				parallelVfxPlayer.Register(battleCardBase.BattleCardView.InitializeBattleCardStackIcon(battleCardBase, battleCardBase.Skills));
			}
			else
			{
				parallelVfxPlayer.Register(battleCardBase.BattleCardView.InitializeBattleCardIcon(battleCardBase, battleCardBase.Skills));
			}
		}
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		return sequentialVfxPlayer;
	}
}
