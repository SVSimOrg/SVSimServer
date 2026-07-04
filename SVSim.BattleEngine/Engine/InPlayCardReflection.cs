using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class InPlayCardReflection : ReceivePlayActionsReflectionBase
{
	protected int _playIndex;

	private List<NetworkBattleReceiver.TargetData> _playActions;

	private NetworkBattleDefine.PlayActionType _actionType;

	public InPlayCardReflection(BattleManagerBase battleMgr, OperateMgr operateMgr)
		: base(battleMgr, operateMgr)
	{
	}

	public void ReadySetting(List<NetworkBattleReceiver.TargetData> action, NetworkBattleDefine.PlayActionType actionType, int playIndex)
	{
		_actionType = actionType;
		_playIndex = playIndex;
		if (action != null)
		{
			_playActions = action;
		}
	}

	public void Play(bool isPlayer, List<int> choiceId, bool isChoice)
	{
		switch (_actionType)
		{
		case NetworkBattleDefine.PlayActionType.ATTACK:
			Attack(isPlayer);
			break;
		case NetworkBattleDefine.PlayActionType.EVOLUTION:
		case NetworkBattleDefine.PlayActionType.EVOLUTION_SELECT:
			Evol(isPlayer, choiceId, isChoice);
			break;
		}
	}

	private void Attack(bool isPlayer)
	{
		BattleCardBase targetCard = NetworkBattleGenericTool.LookForActionDataToTargetCard(_battleMgr, _playActions)[0];
		int playIndex = _playIndex;
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isPlayer);
		BattleCardBase battleCardIdx = _battleMgr.GetBattleCardIdx(battlePlayer.ClassAndInPlayCardList, playIndex);
		if (isPlayer)
		{
			RegisterPairToAttackSelectControl(battleCardIdx, targetCard);
		}
		VfxBase vfx = _operateMgr.Attack(battleCardIdx, targetCard, isPlayer);
		_battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
	}

	protected virtual void RegisterPairToAttackSelectControl(BattleCardBase attackCard, BattleCardBase targetCard)
	{
		AttackSelectControl.AttackPair attackPair = new AttackSelectControl.AttackPair(attackCard.BattleCardView, targetCard.BattleCardView);
		AttackSelectControl attackSelectControl = _battleMgr.BattlePlayer.BattleView.AttackSelectControl;
		attackPair._attackTarget._isReady = !attackSelectControl.IsCardTranslatable(targetCard.BattleCardView);
		attackSelectControl.RegisterAttackPair(attackPair);
	}

	private void Evol(bool isPlayer, List<int> choiceId, bool isChoice)
	{
		int playIndex = _playIndex;
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isPlayer);
		BattleCardBase battleCardIdx = _battleMgr.GetBattleCardIdx(battlePlayer.ClassAndInPlayCardList, playIndex);
		if (_battleMgr.GameMgr.IsReplayBattle && battleCardIdx == null)
		{
			return;
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		if (_actionType == NetworkBattleDefine.PlayActionType.EVOLUTION_SELECT)
		{
			list = NetworkBattleGenericTool.GetOpposingCardObjTarget(_battleMgr, _playActions);
			bool flag = true;
			foreach (BattleCardBase item in list)
			{
				if (item != null)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list = null;
			}
		}
		_battleMgr.VfxMgr.RegisterSequentialVfx(CreateEvolveVfx(battleCardIdx, list, isPlayer, choiceId, isChoice));
	}

	public override void RecordSelectStart(BattleCardBase receivedCard, BattleCardBase choiceTransformCard = null)
	{
		_actingCard = receivedCard;
		_selectSkills = GetSelectSkills(receivedCard, isEvolve: true);
		_currentSkill = _selectSkills.First();
		_isBurialRiteSelect = _currentSkill.IsBurialRite;
		List<BattleCardBase> selectableCards = GetSelectableCards(receivedCard.IsPlayer);
		_isBurialRiteSelect = false;
		_operateMgr.StartSelectCard(receivedCard, isEvolve: true, selectableCards, isChoiceBrave: false);
	}

	public override void RecordSelectCard(BattleCardBase targetCard, bool isBurialRiteSelect)
	{
		_operateMgr.SelectCard(targetCard, isEvolve: true, _actingCard, isChoiceBrave: false, isBurialRiteSelect);
		_selectSkills.Remove(_currentSkill);
		_currentSkill = _selectSkills.First();
		_selectedCards.Add(targetCard);
		int num = _actingCard.EvolutionSkills.Where((SkillBase s) => s.IsBurialRite).Count();
		_isBurialRiteSelect = _currentSkill.IsBurialRite && _selectedCards.Count < num;
		List<BattleCardBase> selectableCards = GetSelectableCards(_actingCard.IsPlayer, null, _selectedCards);
		_isBurialRiteSelect = false;
		_operateMgr.StartMultipleSelectCard(_actingCard, isEvolve: true, selectableCards, isChoiceBrave: false);
	}

	public override void RecordCompleteSelect(BattleCardBase targetCard, bool isBurialRiteSelect, bool isChoiceBraveSelect)
	{
		_operateMgr.CompleteSelectCard(targetCard, isEvolve: true, _actingCard, isChoiceBraveSelect, isBurialRiteSelect);
		ClearData();
	}

	public override void RecordCancelSelect()
	{
		if (_actingCard != null)
		{
			_operateMgr.CancelSelect(_actingCard, isEvolve: true, _actingCard.Index == 0);
		}
		ClearData();
	}

	public override void RecordStartChoiceSelect(BattleCardBase playedCard)
	{
		_actingCard = playedCard;
		_currentSkill = GetSelectSkills(playedCard, isEvolve: true).FirstOrDefault();
		List<BattleCardBase> choiceCards = _currentSkill.GetSelectableCards(new BattlePlayerReadOnlyInfoPair(_currentSkill.SkillPrm.selfBattlePlayer, _currentSkill.SkillPrm.opponentBattlePlayer), new SkillConditionCheckerOption()).ToList();
		_operateMgr.StartChoiceCard(playedCard, isEvolve: true, choiceCards, isChoiceBrave: false);
	}

	public override void RecordCompleteChoiceSelect(List<int> choiceIdList)
	{
		List<int> choiceTokenIds = (from i in SkillOptionValue.ParseOptionTokenID(_currentSkill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.card_id, "_OPT_NULL_"))
			select (!_actingCard.BaseParameter.IsFoil) ? i : (i + 1)).ToList();
		_operateMgr.CompleteChoiceCard(null, isEvolve: true, new List<BattleCardBase>(), _actingCard, choiceIdList.Select((int i) => choiceTokenIds.IndexOf(i)).ToList(), hasSelectionSkill: false, isChoiceBrave: false);
	}

	public override void RecordCancelChoice()
	{
		if (_actingCard != null)
		{
			_operateMgr.CancelChoice(_actingCard, isEvolve: true, isChoiceBrave: false);
		}
		ClearData();
	}

	protected virtual VfxBase CreateEvolveVfx(BattleCardBase evolvedCard, List<BattleCardBase> targetCards, bool isPlayer, List<int> choiceId, bool isChoice)
	{
		if (isChoice && (choiceId == null || choiceId.Count <= 0))
		{
			choiceId = new List<int>();
			choiceId.Add(100011010);
		}
		return _operateMgr.EvolutionCard(evolvedCard, isPlayer, targetCards, choiceId);
	}

	protected override VfxBase CreateAfterSelectVfx(BattleCardBase actingCard, List<int> selectedChoiceCardIds, bool isPlayer = true, bool isChoiceBrave = false)
	{
		return NullVfx.GetInstance();
	}
}
