using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class PuzzleGenerator
{
	private readonly PuzzleBattleManager _puzzleBattleMgr;

	public PuzzleGenerator(PuzzleBattleManager mgr)
	{
		_puzzleBattleMgr = mgr;
	}

	public VfxBase Generate(PuzzleQuestData puzzleQuestData)
	{
		PuzzleBattleManager obj = _puzzleBattleMgr;
		BattlePlayer battlePlayer = obj.BattlePlayer;
		BattleEnemy battleEnemy = obj.BattleEnemy;
		IEnumerable<BattleCardBase> playerFieldCards = new List<BattleCardBase>(battlePlayer.InPlayCards);
		IEnumerable<BattleCardBase> playerHandCards = new List<BattleCardBase>(battlePlayer.HandCardList);
		IEnumerable<BattleCardBase> playerDeckCards = new List<BattleCardBase>(battlePlayer.DeckCardList);
		IEnumerable<BattleCardBase> enemyFieldCards = new List<BattleCardBase>(battleEnemy.InPlayCards);
		IEnumerable<BattleCardBase> enemyHandCards = new List<BattleCardBase>(battleEnemy.HandCardList);
		IEnumerable<BattleCardBase> enemyDeckCards = new List<BattleCardBase>(battleEnemy.DeckCardList);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(ClearInPlayAndHand(battlePlayer, playerFieldCards, playerHandCards));
		parallelVfxPlayer.Register(ClearInPlayAndHand(battleEnemy, enemyFieldCards, enemyHandCards));
		battlePlayer.ClearBattleCount();
		battleEnemy.ClearBattleCount();
		battlePlayer.EvolveWaitTurnCount = 0;
		battleEnemy.EvolveWaitTurnCount = 0;
		battlePlayer.DeckCardList.Clear();
		battleEnemy.DeckCardList.Clear();
		battlePlayer.DeckSkillCardList.Clear();
		battleEnemy.DeckSkillCardList.Clear();
		bool isSkillLost = battlePlayer.Class.IsSkillLost;
		bool isSkillLost2 = battleEnemy.Class.IsSkillLost;
		battlePlayer.Class.LoseSkill().Play();
		battleEnemy.Class.LoseSkill().Play();
		battlePlayer.Class.DamagedCounter.Clear();
		battlePlayer.Class.SkillApplyInformation.ForceDepriveForceWrath();
		battleEnemy.Class.DamagedCounter.Clear();
		battleEnemy.Class.SkillApplyInformation.ForceDepriveForceWrath();
		battlePlayer.Class.IsSkillLost = isSkillLost;
		battleEnemy.Class.IsSkillLost = isSkillLost2;
		if (_puzzleBattleMgr.DetailMgr.DetailPanelControl.EvoTargetPanelColliderGameObject != null)
		{
			_puzzleBattleMgr.DetailMgr.DetailPanelControl.EvoTargetPanelColliderGameObject.transform.parent = _puzzleBattleMgr.DetailMgr.DetailPanel.transform;
		}
		SetUpField(battlePlayer, battleEnemy, puzzleQuestData);
		battlePlayer.EvolvedCards.Clear();
		if (puzzleQuestData.Id != 109)
		{
			battleEnemy.EvolvedCards.Clear();
		}
		battlePlayer.TurnEvolveCardCountInfo.Clear();
		battleEnemy.TurnEvolveCardCountInfo.Clear();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create(parallelVfxPlayer, InstantVfx.Create(delegate
		{
			DestroyCardAndCorutine(playerDeckCards);
			DestroyCardAndCorutine(playerHandCards);
			DestroyCardAndCorutine(playerFieldCards);
			DestroyCardAndCorutine(enemyDeckCards);
			DestroyCardAndCorutine(enemyHandCards);
			DestroyCardAndCorutine(enemyFieldCards);
		}), RecoveryClassView(battlePlayer, battleEnemy), RecoveryInPlayAndHand(battlePlayer), RecoveryInPlayAndHand(battleEnemy), NullVfx.GetInstance(), NullVfx.GetInstance(), battlePlayer.StartBattleMainView(playEffect: false), battleEnemy.StartBattleMainView(playEffect: false));
		return ParallelVfxPlayer.Create(sequentialVfxPlayer);
	}

	private VfxBase RecoveryClassView(BattlePlayerBase player, BattlePlayerBase enemy)
	{
		return InstantVfx.Create(delegate
		{
			if (player.Class.BattleCardView is PlayerClassBattleCardView playerClassBattleCardView)
			{
				iTween.Stop(playerClassBattleCardView.GameObject);
				playerClassBattleCardView.GameObject.transform.localPosition = Vector3.zero;
				playerClassBattleCardView.GameObject.SetActive(value: true);
				playerClassBattleCardView.ClassCharacter.SetAnimationEnable(PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_LEADER_ANIMATION));
				playerClassBattleCardView.GameObject.GetComponent<CardTemplate>().Collider.enabled = true;
			}
			if (enemy.Class.BattleCardView is EnemyClassBattleCardView enemyClassBattleCardView)
			{
				iTween.Stop(enemyClassBattleCardView.GameObject);
				enemyClassBattleCardView.GameObject.transform.localPosition = Vector3.zero;
				enemyClassBattleCardView.GameObject.SetActive(value: true);
				enemyClassBattleCardView.ClassCharacter.SetAnimationEnable(PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_LEADER_ANIMATION));
				enemyClassBattleCardView.GameObject.GetComponent<CardTemplate>().Collider.enabled = true;
			}
		});
	}

	private void SetUpField(BattlePlayer player, BattleEnemy enemy, PuzzleQuestData puzzleQuestData)
	{
		BattleLogManager.GetInstance().AddLogTurn(isSelfTurn: true);
		player.BattleView.ClearPlayQueue();
		if (!player.ClassAndInPlayCardList.Any((BattleCardBase c) => c.IsClass))
		{
			player.ClassAndInPlayCardList.Insert(0, player.Class);
		}
		if (!enemy.ClassAndInPlayCardList.Any((BattleCardBase c) => c.IsClass))
		{
			enemy.ClassAndInPlayCardList.Insert(0, enemy.Class);
		}
		player.ClassAndInPlayCardList.RemoveAll((BattleCardBase c) => !c.IsClass);
		enemy.ClassAndInPlayCardList.RemoveAll((BattleCardBase c) => !c.IsClass);
		player.TurnPlayCards.Clear();
		player.Class.SkillApplyInformation.LifeModifierList.Clear();
		player.Class.SkillApplyInformation.DamageList.Clear();
		enemy.Class.SkillApplyInformation.LifeModifierList.Clear();
		enemy.Class.SkillApplyInformation.DamageList.Clear();
		player.NowTurnEvol = true;
		player.Turn = 0;
		player.Class.SkillApplyInformation.LifeModifierList.Add(new DamageCardParameterModifier(20 - puzzleQuestData.BattleData.PlayerLife, -1, isSelfTurn: false));
		int playerPPCount = puzzleQuestData.BattleData.PlayerPPCount;
		player.SetCurrentEpCount(puzzleQuestData.BattleData.PlayerEPCount);
		int playerGraveCount = puzzleQuestData.BattleData.PlayerGraveCount;
		player.PpTotal = puzzleQuestData.BattleData.PlayerPPCount;
		if (player.IsShortageDeck)
		{
			PuzzleBattleManager puzzleBattleManager = _puzzleBattleMgr;
			player.ResetIsShortageDeck();
			iTween.Stop(puzzleBattleManager.ReaperCard);
			puzzleBattleManager.ReaperCard.transform.SetParent(puzzleBattleManager.CardHolder.transform);
			MotionUtils.SetLayerAll(puzzleBattleManager.ReaperCard, 10);
			puzzleBattleManager.ReaperCard.transform.localPosition = new Vector3(-4.1f, 16.4f, 4.1f);
			puzzleBattleManager.ReaperCard.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			puzzleBattleManager.ReaperCard.transform.localScale = Global.CARD_BASE_STAY_SCALE;
			TweenColor.Begin(puzzleBattleManager.ReaperCard, 0f, Color.white);
		}
		enemy.NowTurnEvol = true;
		enemy.Turn = 0;
		int enemyLife = puzzleQuestData.BattleData.EnemyLife;
		if (enemyLife <= 20)
		{
			enemy.Class.SkillApplyInformation.LifeModifierList.Add(new DamageCardParameterModifier(20 - puzzleQuestData.BattleData.EnemyLife, -1, isSelfTurn: false));
		}
		else
		{
			((ClassBattleCardBase)enemy.Class).InitBaseMaxLife(enemyLife);
		}
		int enemyPPCount = puzzleQuestData.BattleData.EnemyPPCount;
		enemy.SetCurrentEpCount(puzzleQuestData.BattleData.EnemyEPCount);
		int enemyGraveCount = puzzleQuestData.BattleData.EnemyGraveCount;
		enemy.Pp = enemyPPCount;
		enemy.PpTotal = enemyPPCount;
		CardPrm[] playerInplay = puzzleQuestData.BattleData.PlayerInplay;
		foreach (CardPrm cardPrm in playerInplay)
		{
			player.Pp = playerPPCount;
			FieldGenSetCard(player, enemy, cardPrm);
		}
		playerInplay = puzzleQuestData.BattleData.EnemyInplay;
		foreach (CardPrm cardPrm2 in playerInplay)
		{
			enemy.Pp = enemyPPCount;
			FieldGenSetCard(enemy, player, cardPrm2);
		}
		int[] playerHand = puzzleQuestData.BattleData.PlayerHand;
		foreach (int cardId in playerHand)
		{
			player.HandCardList.Add(player.CreateNextIndexCard(cardId));
		}
		playerHand = puzzleQuestData.BattleData.EnemyHand;
		foreach (int cardId2 in playerHand)
		{
			enemy.HandCardList.Add(enemy.CreateNextIndexCard(cardId2));
		}
		playerHand = puzzleQuestData.BattleData.PlayerDeck;
		foreach (int cardId3 in playerHand)
		{
			AddToDeckNoIndexChange(player, player.CreateNextIndexCard(cardId3));
		}
		playerHand = puzzleQuestData.BattleData.EnemyDeck;
		foreach (int cardId4 in playerHand)
		{
			AddToDeckNoIndexChange(enemy, enemy.CreateNextIndexCard(cardId4));
		}
		player.Pp = playerPPCount;
		enemy.Pp = enemyPPCount;
		foreach (BattleCardBase handCard in player.HandCardList)
		{
			handCard.BattleCardView.ShowHandCardInfo().Play();
		}
		player.GainCemetery(1000);
		player.CemeteryList.Clear();
		for (int num2 = 0; num2 < playerGraveCount; num2++)
		{
			BattleCardBase targetCard = CardCreatorBase.CreateDummyInstance();
			player.DummyCardToCemetery(targetCard);
		}
		enemy.GainCemetery(1000);
		enemy.CemeteryList.Clear();
		for (int num3 = 0; num3 < enemyGraveCount; num3++)
		{
			BattleCardBase targetCard2 = CardCreatorBase.CreateDummyInstance();
			enemy.DummyCardToCemetery(targetCard2);
		}
		for (int num4 = 0; num4 < puzzleQuestData.BattleData.PlayerDeckCount - puzzleQuestData.BattleData.PlayerDeck.Count(); num4++)
		{
			player.AddToDeck(player.CreateNextIndexCard(100011040));
		}
		for (int num5 = 0; num5 < puzzleQuestData.BattleData.EnemyDeckCount - puzzleQuestData.BattleData.EnemyDeck.Count(); num5++)
		{
			enemy.AddToDeck(enemy.CreateNextIndexCard(100011040));
		}
	}

	private VfxBase ClearInPlayAndHand(BattlePlayerBase player, IEnumerable<BattleCardBase> fieldCardList, IEnumerable<BattleCardBase> handCardList)
	{
		if (_puzzleBattleMgr == null)
		{
			return NullVfx.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase fieldCard in fieldCardList)
		{
			parallelVfxPlayer.Register(fieldCard.SkillApplyInformation.AllSkillEffectStop());
			player.BattleView.InPlayView.RemoveCardFromView(fieldCard.BattleCardView);
			player.ClassAndInPlayCardList.Remove(fieldCard);
		}
		foreach (BattleCardBase handCard in handCardList)
		{
			parallelVfxPlayer.Register(handCard.SkillApplyInformation.AllSkillEffectStop());
			player.BattleView.HandView.RemoveCardFromViewWithoutRearrange(handCard.BattleCardView);
			player.HandCardList.Remove(handCard);
		}
		return parallelVfxPlayer;
	}

	private void DestroyCardAndCorutine(IEnumerable<BattleCardBase> cardList)
	{
		foreach (BattleCardBase card in cardList)
		{
			if (card.BattleCardView._inPlayRearrangeCoroutine != null)
			{
				BattleCoroutine.GetInstance().StopCoroutine(card.BattleCardView._inPlayRearrangeCoroutine);
			}
			Object.Destroy(card.BattleCardView.GameObject);
		}
	}

	private VfxBase RecoveryInPlayAndHand(BattlePlayerBase player)
	{
		return ParallelVfxPlayer.Create(NullVfx.GetInstance(), player.Class.SkillApplyInformation.CreateVfxSkillProtection(), player.BattleView.RecoveryInPlayCards(), player.BattleView.RecoveryInHandCards(), OpeningVfx.ShowBattleUIImmediatelyVfx(player, fixDirection: true), player.UsePp(0), InstantVfx.Create(player.BattleView.HideCommonPanel));
	}

	private void AddToDeckNoIndexChange(BattlePlayerBase player, BattleCardBase card)
	{
		player.DeckCardList.Add(card);
		if (card.HasDeckSelfSkill)
		{
			player.AddDeckSkillCard(card);
		}
	}

	private void FieldGenSetCard(BattlePlayerBase player, BattlePlayerBase enemy, CardPrm cardPrm)
	{
		bool isRecovery = _puzzleBattleMgr.IsRecovery;
		_puzzleBattleMgr.IsRecovery = true;
		SkillProcessor skillProcessor = new SkillProcessor();
		BattleCardBase battleCardBase = player.CreateNextIndexCard(cardPrm.Id);
		player.HandCardList.Add(battleCardBase);
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.PlayedCard = battleCardBase;
		skillConditionCheckerOption.SummonedCard = battleCardBase;
		battleCardBase.PlayCard(skillProcessor, skillConditionCheckerOption, isInplayGeneration: true);
		battleCardBase.SelfBattlePlayer.StartSkillWhenChangeInplay(null, new List<BattleCardBase> { battleCardBase }, skillProcessor, isSummonCheck: false, null, skillConditionCheckerOption);
		skillProcessor.Process(new BattlePlayerPair(player, enemy));
		if (cardPrm.IsEvolve)
		{
			SkillProcessor skillProcessor2 = new SkillProcessor();
			battleCardBase.Evolution(isSkill: true, skillProcessor2, skillConditionCheckerOption);
			skillProcessor2.Process(new BattlePlayerPair(player, enemy));
		}
		if (cardPrm.ChantCount != -1)
		{
			battleCardBase.SkillApplyInformation.GiveChantCount(new ChantCountSetModifier(cardPrm.ChantCount));
		}
		battleCardBase.BattleCardView.InitializeBattleCardIcon(battleCardBase, battleCardBase.Skills).Play();
		_puzzleBattleMgr.IsRecovery = isRecovery;
	}
}
