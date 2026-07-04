using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.Touch;
using Wizard.Battle.View.Vfx;

public class PlayHandCardReflection : ReceivePlayActionsReflectionBase
{
	protected int _cardIdx;

	protected readonly NetworkBattleData _networkBattleData;

	private List<NetworkBattleReceiver.TargetData> _playHandTargetDataList;

	public PlayHandCardReflection(BattleManagerBase battleMgr, OperateMgr operateMgr, NetworkBattleData networkBattleData)
		: base(battleMgr, operateMgr)
	{
		_networkBattleData = networkBattleData;
	}

	public void ReadySetting(int cardIndex, List<NetworkBattleReceiver.TargetData> targetDataList)
	{
		_cardIdx = cardIndex;
		_playHandTargetDataList = targetDataList;
		if (_networkBattleData != null)
		{
			_networkBattleData.isEchoWait = true;
		}
	}

	public BattleCardBase Play(BattlePlayerBase player, bool isPlayer = false, List<int> choiceId = null, bool isChoice = false)
	{
		BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, player, _cardIdx);
		PlayMove(indexToCardBase, isPlayer, choiceId, isChoice);
		return indexToCardBase;
	}

	protected virtual void PlayMove(BattleCardBase playedCard, bool isPlayer = false, List<int> choiceId = null, bool isChoice = false)
	{
		if (isChoice && (choiceId == null || choiceId.Count == 0))
		{
			choiceId = new List<int>();
			int num = 1;
			SkillBase skillBase = playedCard.Skills.FirstOrDefault((SkillBase s) => s is Skill_choice);
			if (skillBase != null)
			{
				num = ChoiceUtility.GetNumberOfCardsToSelect(skillBase);
			}
			for (int num2 = 0; num2 < num; num2++)
			{
				choiceId.Add(100011010);
			}
		}
		Skill_transform accelerateOrCrystallizeTransformSkill = playedCard.GetAccelerateOrCrystallizeTransformSkill();
		BattleCardBase checkCard = ((accelerateOrCrystallizeTransformSkill != null) ? _battleMgr.CreateTransformCardRegisterVfx(accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard, accelerateOrCrystallizeTransformSkill.TransformId, accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard.IsPlayer) : playedCard);
		if (checkCard is SpellBattleCard)
		{
			BattlePlayerReadOnlyInfoPair battlePlayerPair = new BattlePlayerReadOnlyInfoPair(checkCard.SelfBattlePlayer, checkCard.OpponentBattlePlayer);
			SkillConditionCheckerOption option = new SkillConditionCheckerOption();
			int lastActiveDontSelectStartSkillIndex = checkCard.Skills.IndexOf(checkCard.Skills.LastOrDefault((SkillBase s) => s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessDontSelectStart) && s.CheckCondition(battlePlayerPair, option, isPrePlay: true)));
			int ppFixedUseSkillIndex = checkCard.Skills.IndexOf(checkCard.Skills.FirstOrDefault((SkillBase s) => s is Skill_pp_fixeduse));
			if (checkCard.Skills.Any((SkillBase s) => s.IsWhenPlaySkill && s.IsUserSelectType && !s.IsEmptyHandedUserSelectType && checkCard.Skills.IndexOf(s) > lastActiveDontSelectStartSkillIndex && (lastActiveDontSelectStartSkillIndex == -1 || ppFixedUseSkillIndex == -1 || checkCard.Skills.IndexOf(s) < ppFixedUseSkillIndex)))
			{
				SendEcho(playedCard, _networkBattleData.GetReceiveData().actionType);
				return;
			}
		}
		SequentialVfxPlayer vfx = SequentialVfxPlayer.Create(_operateMgr.InitSetCard(playedCard, isPlayer), _operateMgr.PlayCard(playedCard, isPlayer, null, isRecovery: false, choiceId));
		_battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
		SendEcho(playedCard, _networkBattleData.GetReceiveData().actionType);
	}

	public BattleCardBase PlayAction(bool isPlayer = false, List<int> choiceId = null)
	{
		BattleCardBase battleCardBase = null;
		List<BattleCardBase> list = null;
		if (isPlayer)
		{
			battleCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattlePlayer, _cardIdx);
			list = NetworkBattleGenericTool.LookForActionDataToTargetCard(_battleMgr, _playHandTargetDataList);
		}
		else
		{
			battleCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattleEnemy, _cardIdx);
			list = NetworkBattleGenericTool.GetOpposingCardObjTarget(_battleMgr, _playHandTargetDataList);
		}
		PlayActionMove(battleCardBase, list, isPlayer, choiceId);
		return battleCardBase;
	}

	protected virtual void PlayActionMove(BattleCardBase receivedCard, List<BattleCardBase> targetCards, bool isPlayer = false, List<int> choiceId = null)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(_operateMgr.InitSetCard(receivedCard, isPlayer, isSelect: true));
		sequentialVfxPlayer.Register(_operateMgr.PlayCard(receivedCard, isPlayer, targetCards, isRecovery: false, choiceId));
		_battleMgr.VfxMgr.RegisterSequentialVfx(sequentialVfxPlayer);
		SendEcho(receivedCard, _networkBattleData.GetReceiveData().actionType);
	}

	public virtual BattleCardBase FusionMove(BattlePlayerBase player)
	{
		BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, player, _cardIdx);
		List<BattleCardBase> list = null;
		list = ((!player.IsPlayer) ? NetworkBattleGenericTool.GetOpposingCardObjTarget(_battleMgr, _playHandTargetDataList) : NetworkBattleGenericTool.LookForActionDataToTargetCard(_battleMgr, _playHandTargetDataList));
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if ((_battleMgr.GameMgr.IsWatchBattle && player.IsPlayer) || _battleMgr.GameMgr.IsAdminWatch)
		{
			VfxBase canNotTouchCardVfx = NullVfx.GetInstance();
			_battleMgr.VfxMgr.RegisterImmediateVfx(canNotTouchCardVfx);
			parallelVfxPlayer.Register(_actingCard.SelfBattlePlayer.BattleView.RemoveFusionSelectedCardFromHand(_selectedCards));
			parallelVfxPlayer.Register(_actingCard.SelfBattlePlayer.BattleView.CreateStopShowSelectVfx(_actingCard, isAct: true, stopChoiceSelectUiImmediately: false));
			parallelVfxPlayer.Register(_operateMgr.FusionCard(indexToCardBase, player.IsPlayer, list));
			_battleMgr.VfxMgr.RegisterSequentialVfx(SequentialVfxPlayer.Create(parallelVfxPlayer, InstantVfx.Create(delegate
			{
			})));
		}
		else
		{
			parallelVfxPlayer.Register(_operateMgr.FusionCard(indexToCardBase, player.IsPlayer, list));
			_battleMgr.VfxMgr.RegisterSequentialVfx(parallelVfxPlayer);
		}
		SendEcho(indexToCardBase, _networkBattleData.GetReceiveData().actionType);
		return indexToCardBase;
	}

	public void ChoiceBraveMove(BattlePlayerBase player, List<int> choiceIdList)
	{
		BattleCardBase battleCardBase = player.Class;
		if (!player.CanChoiceBraveThisTurn && !_battleMgr.IsRecovery)
		{
			SendEcho(battleCardBase, _networkBattleData.GetReceiveData().actionType);
			return;
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		GameMgr gameMgr = _battleMgr.GameMgr;
		if ((!gameMgr.IsWatchBattle || !player.IsPlayer) && !gameMgr.IsAdminWatch && !_battleMgr.IsRecovery)
		{
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				gameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_CARD_SELECT_3, _battleMgr.BattleUIContainer.EnemyChoiceBraveBtn.position);

				player.BattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
			}));
		}
		parallelVfxPlayer.Register(_operateMgr.PlayCard(battleCardBase, player.IsPlayer, NetworkBattleGenericTool.GetOpposingCardObjTarget(_battleMgr, _playHandTargetDataList), isRecovery: false, choiceIdList, isChoiceBrave: true));
		_battleMgr.VfxMgr.RegisterSequentialVfx(parallelVfxPlayer);
		SendEcho(battleCardBase, _networkBattleData.GetReceiveData().actionType);
	}

	protected virtual void SendEcho(BattleCardBase receivedCard, NetworkBattleDefine.PlayActionType actionType)
	{
		if (_battleMgr is NetworkBattleManagerBase networkBattleManagerBase)
		{
			networkBattleManagerBase.SendEcho(receivedCard.Index, actionType);
		}
		else
		{
			LocalLog.AccumulateLastTraceLog("Not PlayHand Echo");
		}
		if (_networkBattleData != null)
		{
			_networkBattleData.isEchoWait = false;
		}
	}

	protected override VfxBase CreateAfterSelectVfx(BattleCardBase actingCard, List<int> selectedChoiceCardIds, bool isPlayer, bool isChoiceBrave)
	{
		return NullVfx.GetInstance();
	}

	public override void RecordSelectStart(BattleCardBase receivedCard, BattleCardBase choiceTransformCard = null)
	{
		_actingCard = receivedCard;
		BattleCardBase battleCardBase = receivedCard;
		Skill_transform accelerateOrCrystallizeTransformSkill = receivedCard.GetAccelerateOrCrystallizeTransformSkill();
		if (accelerateOrCrystallizeTransformSkill != null)
		{
			battleCardBase = _battleMgr.CreateTransformCardRegisterVfx(receivedCard, accelerateOrCrystallizeTransformSkill.TransformId, receivedCard.IsPlayer);
		}
		if (choiceTransformCard != null)
		{
			battleCardBase = choiceTransformCard;
		}
		_selectSkills = GetSelectSkills(battleCardBase, isEvolve: false);
		_currentSkill = _selectSkills.First();
		battleCardBase.Skills.Where((SkillBase s) => s.IsBurialRite).Count();
		_isBurialRiteSelect = _currentSkill.IsBurialRite;
		if (_isBurialRiteSelect && _currentSkill.IsUserSelectType)
		{
			_selectSkills.Add(_currentSkill);
		}
		List<BattleCardBase> selectableCards = GetSelectableCards(battleCardBase.IsPlayer);
		_isBurialRiteSelect = false;
		_operateMgr.StartSelectCard(receivedCard, isEvolve: false, selectableCards, _actingCard.Index == 0);
	}

	public override void RecordSelectCard(BattleCardBase targetCard, bool isBurialRiteSelect)
	{
		_operateMgr.SelectCard(targetCard, isEvolve: false, _actingCard, _actingCard.Index == 0, isBurialRiteSelect);
		_selectSkills.Remove(_currentSkill);
		_currentSkill = _selectSkills.First();
		_selectedCards.Add(targetCard);
		int num = _actingCard.Skills.Where((SkillBase s) => s.IsBurialRite).Count();
		_isBurialRiteSelect = _currentSkill.IsBurialRite && _selectedCards.Count < num;
		List<BattleCardBase> selectableCards = GetSelectableCards(_actingCard.IsPlayer, null, _selectedCards);
		_isBurialRiteSelect = false;
		_operateMgr.StartMultipleSelectCard(_actingCard, isEvolve: false, selectableCards, _actingCard.Index == 0);
	}

	public override void RecordCompleteSelect(BattleCardBase targetCard, bool isBurialRiteSelect, bool isChoiceBraveSelect)
	{
		_operateMgr.CompleteSelectCard(targetCard, isEvolve: false, _actingCard, isChoiceBraveSelect, isBurialRiteSelect);
		ClearData();
	}

	public override void RecordCancelSelect()
	{
		if (_actingCard != null)
		{
			if (_currentSkill is Skill_fusion)
			{
				_operateMgr.CancelFusion(_actingCard);
			}
			else
			{
				_operateMgr.CancelSelect(_actingCard, isEvolve: false, _actingCard.Index == 0);
			}
		}
		ClearData();
	}

	public override void RecordStartChoiceSelect(BattleCardBase playedCard)
	{
		_actingCard = playedCard;
		BattleCardBase actingCard = playedCard;
		Skill_transform accelerateOrCrystallizeTransformSkill = playedCard.GetAccelerateOrCrystallizeTransformSkill();
		if (accelerateOrCrystallizeTransformSkill != null)
		{
			actingCard = _battleMgr.CreateTransformCardRegisterVfx(playedCard, accelerateOrCrystallizeTransformSkill.TransformId, playedCard.IsPlayer);
		}
		_currentSkill = GetSelectSkills(actingCard, isEvolve: false).FirstOrDefault();
		List<BattleCardBase> choiceCards = _currentSkill.GetSelectableCards(new BattlePlayerReadOnlyInfoPair(_currentSkill.SkillPrm.selfBattlePlayer, _currentSkill.SkillPrm.opponentBattlePlayer), new SkillConditionCheckerOption()).ToList();
		_operateMgr.StartChoiceCard(playedCard, isEvolve: false, choiceCards, playedCard.Index == 0);
	}

	public override void RecordCompleteChoiceSelect(List<int> choiceIdList)
	{
		List<int> choiceTokenIds = (from i in SkillOptionValue.ParseOptionTokenID(_currentSkill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.card_id, "_OPT_NULL_"))
			select (!_actingCard.BaseParameter.IsFoil) ? i : (i + 1)).ToList();
		Skill_transform skill_transform = _actingCard.Skills.Get(_actingCard.Skills.IndexOf(_currentSkill) + 1) as Skill_transform;
		bool flag = choiceIdList.Any((int i) => CardMaster.IsChoiceBraveCardCheck(i));
		if ((skill_transform != null && skill_transform.OnWhenChoicePlayStart != 0) || flag)
		{
			BattleCardBase battleCardBase = _battleMgr.CreateTransformCardRegisterVfx(_actingCard, choiceIdList.First(), _actingCard.IsPlayer);
			if (ChoiceUtility.DoesChoiceCardHaveSelectSkill(battleCardBase, _currentSkill))
			{
				_operateMgr.CompleteChoiceCard(null, isEvolve: false, new List<BattleCardBase>(), _actingCard, choiceIdList.Select((int i) => choiceTokenIds.IndexOf(i)).ToList(), hasSelectionSkill: true, flag);
				RecordSelectStart(_actingCard, battleCardBase);
			}
			else
			{
				_operateMgr.CompleteChoiceCard(null, isEvolve: false, new List<BattleCardBase>(), _actingCard, choiceIdList.Select((int i) => choiceTokenIds.IndexOf(i)).ToList(), hasSelectionSkill: false, flag);
				ClearData();
			}
		}
		else
		{
			_operateMgr.CompleteChoiceCard(null, isEvolve: false, new List<BattleCardBase>(), _actingCard, choiceIdList.Select((int i) => choiceTokenIds.IndexOf(i)).ToList(), hasSelectionSkill: false, flag);
			ClearData();
		}
	}

	public override void RecordCancelChoice()
	{
		if (_actingCard != null)
		{
			_operateMgr.CancelChoice(_actingCard, isEvolve: false, _actingCard.Index == 0);
		}
		ClearData();
	}

	public override void RecordStartFusion(BattleCardBase fusionCard)
	{
		_actingCard = fusionCard;
		_currentSkill = fusionCard.Skills.FirstOrDefault((SkillBase s) => s is Skill_fusion);
		_fusionMetamorphoseSkill = fusionCard.Skills.FirstOrDefault((SkillBase s) => s is Skill_fusion_metamorphose) as Skill_fusion_metamorphose;
		_selectableCards = _currentSkill.GetSelectableCards(_battleMgr.GetBattlePlayerPair(fusionCard.IsPlayer), new SkillConditionCheckerOption()).ToList();
		_operateMgr.StartFusionSelect(fusionCard, _selectableCards);
	}

	public override void RecordSelectFusion(BattleCardBase targetCard)
	{
		int skillSelectCount = _currentSkill.GetSkillSelectCount();
		bool flag = !_selectedCards.Contains(targetCard);
		if (!flag)
		{
			_selectedCards.Remove(targetCard);
		}
		else
		{
			if (_selectedCards.Count == skillSelectCount && skillSelectCount == 1)
			{
				_selectedCards.Clear();
			}
			_selectedCards.Add(targetCard);
		}
		bool canFusionMetamorphose = _fusionMetamorphoseSkill != null && _fusionMetamorphoseSkill.IsShowFusionMetamorphoseFrameEffect(_selectedCards);
		_operateMgr.SelectFusion(_selectableCards.IndexOf(targetCard), flag, canFusionMetamorphose, skillSelectCount, targetCard);
	}
}
