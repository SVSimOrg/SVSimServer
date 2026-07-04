using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Touch;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class ReceivePlayActionsReflectionBase
{
	public enum SelectChoiceState
	{
		NONE,
		SELECT,
		CHOICE,
		FUSION
	}

	public class CompleteSelectData
	{
		public VfxBase PlayCardVfx { get; private set; }

		public CompleteSelectData(VfxBase selectCardVfx)
		{
			PlayCardVfx = selectCardVfx;
		}
	}

	public class CompleteChoiceData
	{
		public VfxBase PlayCardVfx { get; private set; }

		public void SetChoiceCardVfx(VfxBase choiceCardVfx)
		{
			PlayCardVfx = choiceCardVfx;
		}
	}

	protected readonly BattleManagerBase _battleMgr;

	protected OperateMgr _operateMgr;

	protected BattleCardBase _actingCard;

	protected List<BattleCardBase> _selectedCards = new List<BattleCardBase>();

	protected List<SkillBase> _selectSkills;

	protected SkillBase _currentSkill;

	protected Skill_fusion_metamorphose _fusionMetamorphoseSkill;

	protected bool _isBurialRiteSelect;

	protected List<BattleCardBase> _selectableCards = new List<BattleCardBase>();

	protected BattleCardBase _actingChoiceCard;

	protected List<BattleCardBase> _choiceCards = new List<BattleCardBase>();

	protected List<BattleCardBase> _selectedChoiceCards = new List<BattleCardBase>();

	protected SkillBase _choiceSkill;

	protected int _numberOfChoiceCardsToSelect = 1;

	public SelectChoiceState CurrentState;

	public CompleteSelectData CompleteSelectDataIns;

	public CompleteChoiceData CompleteChoiceDataIns;

	public BattleCardBase ActingCard => _actingCard;

	public ReceivePlayActionsReflectionBase(BattleManagerBase battleMgr, OperateMgr operateMgr)
	{
		_battleMgr = battleMgr;
		_operateMgr = operateMgr;
	}

	protected List<SkillBase> GetSelectSkills(BattleCardBase actingCard, bool isEvolve)
	{
		return actingCard.GetSelectSkillsNoDuplication(actingCard.GetSelectTypeSkill(isEvolve).ToList());
	}

	public static VfxBase CreateHideChoiceCardsVfx(BattleCardBase actingCard, List<BattleCardBase> choiceCardsToHide, bool dontUnloadResources = false)
	{
		return InstantVfx.Create(delegate
		{
			bool flag = ChoiceUtility.DoesDuplicateCardNotExistInHand(actingCard) && !dontUnloadResources;
			for (int i = 0; i < choiceCardsToHide.Count; i++)
			{
				BattleCardBase battleCardBase = choiceCardsToHide[i];
				if (flag)
				{
					battleCardBase.BattleCardView.UnloadResource();
				}
				battleCardBase.BattleCardView.GameObject.SetActive(value: false);
			}
		});
	}

	public virtual void OverrideChoicecard(bool isPlayer)
	{
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isPlayer);
		for (int i = 0; i < _selectedChoiceCards.Count; i++)
		{
			BattleCardBase selectedChoiceCard = GetSelectedChoiceCard(_selectedChoiceCards[i].CardId, _choiceCards);
			int selectedChoiceCardIndex = _choiceCards.IndexOf(selectedChoiceCard);
			if (_selectedChoiceCards.Remove(selectedChoiceCard))
			{
				ToggleSelectedCardButtonSprite(selectedChoiceCardIndex, setActive: false, battlePlayer);
			}
		}
		_selectedChoiceCards.Clear();
	}

	public virtual void StartSelect(int actingCardIndex, bool isPlayer = true)
	{
	}

	public virtual void StartChoiceSelect(int actingCardIndex, bool isPlayer = true)
	{
	}

	public virtual void StartSelectFusion(int actingCardIndex, bool isPlayer = true)
	{
	}

	public virtual void CancelSelect(bool isPlayer = true)
	{
		if (_actingCard != null)
		{
			_operateMgr.SelectCancel(_actingCard, isPlayer, isPlay: false);
		}
	}

	public virtual void CancelChoiceSelect(bool isPlayer = true)
	{
		if (_actingChoiceCard != null)
		{
			BattleCardBase detailOpenCard = _battleMgr.BattlePlayer.PlayerBattleView.DetailOpenCard;
			_operateMgr.SelectCancel(_actingChoiceCard, isPlayer, isPlay: false, isTransformedSkill: false, detailOpenCard != null && !detailOpenCard.IsClass);
		}
	}

	public virtual void SelectCard(int selectedCardIndex, bool isSelfCard, bool isEvolve, bool isPlayer = true, bool isBurialRite = false, bool isChoiceBrave = false, bool isComplete = true)
	{
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isSelfCard);
		BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, battlePlayer, selectedCardIndex);
		VfxBase vfx = CreateSelectCardVfx(indexToCardBase, isEvolve, isPlayer, isBurialRite, isChoiceBrave, isComplete);
		_battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
	}

	public virtual void CompleteSelectCard(int selectedCardIndex, bool isSelfCard, bool isEvolve, bool isPlayer, bool isBurialRite, bool isChoiceBrave)
	{
		VfxBase selectCardVfx = InstantVfx.Create(delegate
		{
			SelectCard(selectedCardIndex, isSelfCard, isEvolve, isPlayer, isBurialRite, isChoiceBrave);
			_actingCard = null;
		});
		CompleteSelectDataIns = new CompleteSelectData(selectCardVfx);
	}

	public virtual void CompleteChoiceEvolve(List<int> selectedChoiceCardIdList)
	{
		VfxBase choiceCardVfx = InstantVfx.Create(delegate
		{
			_battleMgr.VfxMgr.RegisterSequentialVfx(CreateAfterSelectVfx(_actingChoiceCard, selectedChoiceCardIdList));
		});
		CompleteChoiceDataIns.SetChoiceCardVfx(choiceCardVfx);
	}

	public virtual void SelectChoiceCard(int selectedChoiceCardId, bool isEvolve = false, bool isPlayer = true, bool isComplete = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isPlayer);
		BattleCardBase selectedChoiceCard = GetSelectedChoiceCard(selectedChoiceCardId, _choiceCards);
		int selectedChoiceCardIndex = _choiceCards.IndexOf(selectedChoiceCard);
		if (_selectedChoiceCards.Remove(selectedChoiceCard))
		{
			ToggleSelectedCardButtonSprite(selectedChoiceCardIndex, setActive: false, battlePlayer);
			return;
		}
		_selectedChoiceCards.Add(selectedChoiceCard);
		ToggleSelectedCardButtonSprite(selectedChoiceCardIndex, setActive: true, battlePlayer);
		if (_selectedChoiceCards.Count < _numberOfChoiceCardsToSelect || !isComplete)
		{
			return;
		}
		_selectedChoiceCards = ChoiceUtility.SortSelectedChoiceCards(_choiceCards, _selectedChoiceCards);
		List<int> selectedChoiceCardIdList = GetSelectedChoiceCardIds();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer battleCardSelectVfx = ParallelVfxPlayer.Create(_operateMgr.BattleCardSelect(_actingChoiceCard, _selectedChoiceCards, isPlayer, registerEffectsDirectlyToVfxMgr: false), InstantVfx.Create(delegate
		{
			ChoiceUtility.StopChoiceEffects(_choiceCards);
		}));
		IPlayerView playerBattleView = _battleMgr.BattlePlayer.PlayerBattleView;
		if (playerBattleView.DetailOpenCard != null && !playerBattleView.DetailOpenCard.IsClass)
		{
			battleCardSelectVfx.Register(InstantVfx.Create(delegate
			{
				_battleMgr.DetailMgr.DetailPanelControl.Hide();
			}));
		}
		bool isChoiceBrave = CardMaster.IsChoiceBraveCardCheck(selectedChoiceCardId);
		bool num = _actingChoiceCard.Skills.HaveChoiceTransformSkill() || isChoiceBrave;
		bool flag = ChoiceUtility.DoesChoiceCardHaveSelectSkill(selectedChoiceCard, _choiceSkill);
		if (num)
		{
			parallelVfxPlayer.Register(battleCardSelectVfx);
			if (flag)
			{
				battlePlayer.BattleView.SetCancelSkillChoiceTransformCards(_actingChoiceCard, selectedChoiceCard);
				SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
				sequentialVfxPlayer2.Register(InstantVfx.Create(delegate
				{
					ChoiceUtility.SetupChoiceCardForSkillTargetSelect(selectedChoiceCard);
				}));
				sequentialVfxPlayer2.Register(NullVfx.GetInstance());
				sequentialVfxPlayer2.Register(CreateStartSelectVfx(selectedChoiceCard, isEvolve: false, isChoice: true));
				parallelVfxPlayer.Register(sequentialVfxPlayer2);
				parallelVfxPlayer.Register(CreateHideChoiceCardsVfx(_actingChoiceCard, _choiceCards.FindAll((BattleCardBase card) => card != selectedChoiceCard), dontUnloadResources: true));
				CurrentState = SelectChoiceState.SELECT;
				CompleteChoiceDataIns = null;
			}
			else
			{
				if (!isChoiceBrave)
				{
					parallelVfxPlayer.Register(InstantVfx.Create(delegate
					{
						_actingChoiceCard.BattleCardView.Transform.position = selectedChoiceCard.BattleCardView.Transform.position;
					}));
				}
				parallelVfxPlayer.Register(CreateHideChoiceCardsVfx(_actingChoiceCard, _choiceCards));
				CompleteChoiceDataIns.SetChoiceCardVfx(NullVfx.GetInstance());
			}
		}
		else if (isEvolve)
		{
			parallelVfxPlayer.Register(battleCardSelectVfx);
			parallelVfxPlayer.Register(CreateHideChoiceCardsVfx(_actingChoiceCard, _choiceCards));
			CompleteChoiceEvolve(selectedChoiceCardIdList);
		}
		else
		{
			CompleteChoiceDataIns.SetChoiceCardVfx(NullVfx.GetInstance());
		}
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		_battleMgr.VfxMgr.RegisterSequentialVfx(sequentialVfxPlayer);
	}

	public virtual void WatchSelectChoiceCards(List<int> selectedChoiceCardIds, bool isEvolve = false, bool isPlayer = true, bool isComplete = false)
	{
		for (int i = 0; i < _choiceCards.Count; i++)
		{
			if (selectedChoiceCardIds.Contains(_choiceCards[i].CardId) != _selectedChoiceCards.Contains(_choiceCards[i]))
			{
				SelectChoiceCard(_choiceCards[i].CardId, isEvolve, isPlayer, isComplete);
			}
		}
	}

	public virtual void CompleteChoiceCard(List<int> choiceIdList, bool isEvolveTargetSelect, bool isPlayer = true)
	{
		CompleteChoiceDataIns = new CompleteChoiceData();
		OverrideChoicecard(isPlayer);
	}

	protected abstract VfxBase CreateAfterSelectVfx(BattleCardBase actingCard, List<int> selectedChoiceCardIds, bool isPlayer = true, bool isChoiceBrave = false);

	protected VfxBase CreateStartSelectVfx(BattleCardBase actingCard, bool isEvolve = false, bool isChoice = false, Skill_transform transformSkill = null)
	{
		_actingCard = actingCard;
		BattleCardBase battleCardBase = actingCard;
		if (transformSkill != null)
		{
			battleCardBase = _battleMgr.CreateTransformCardRegisterVfx(transformSkill.SkillPrm.ownerCard, transformSkill.TransformId, transformSkill.SkillPrm.ownerCard.IsPlayer);
		}
		_selectSkills = GetSelectSkills(battleCardBase, isEvolve);
		_currentSkill = _selectSkills.First();
		_selectedCards.Clear();
		if (!isChoice)
		{
			_selectedChoiceCards.Clear();
		}
		_isBurialRiteSelect = _currentSkill.IsBurialRite;
		if (_isBurialRiteSelect && _currentSkill.IsUserSelectType)
		{
			_selectSkills.Add(_currentSkill);
		}
		List<BattleCardBase> selectableCards = GetSelectableCards(battleCardBase.IsPlayer);
		_isBurialRiteSelect = false;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(battleCardBase.IsPlayer);
		if (selectableCards.Count > 0)
		{
			sequentialVfxPlayer.Register(GetSelectAlertvfx(battlePlayer));
			sequentialVfxPlayer.Register(battlePlayer.BattleView.StartShowSelect(_battleMgr.GameMgr.IsReplayBattle ? battleCardBase : _actingCard, _currentSkill, selectableCards, isEvolve));
		}
		else
		{
			LocalLog.AccumulateTraceLog("702988 " + _actingCard.CardId);
		}
		return sequentialVfxPlayer;
	}

	protected VfxBase CreateSelectCardVfx(BattleCardBase selectedCard, bool isEvolve, bool isPlayer, bool isBurialRite, bool isChoiceBrave, bool isComplete)
	{
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isPlayer);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(_operateMgr.BattleCardSelect(_actingCard, selectedCard, isPlayer, registerEffectsDirectlyToVfxMgr: false, isTransformskill: false, isBurialRite, isComplete));
		_selectedCards.Add(selectedCard);
		_selectSkills.Remove(_currentSkill);
		SkillBase skillBase = _selectSkills.FirstOrDefault();
		SkillConditionCheckerOption skillConditionCheckerOption = null;
		if (_currentSkill != null)
		{
			skillConditionCheckerOption = new SkillConditionCheckerOption();
			skillConditionCheckerOption.SelectedCards.Add(new SkillConditionCheckerOption.SkillAndSelectTarget(selectedCard, _currentSkill));
		}
		if (skillBase != null)
		{
			_currentSkill = skillBase;
			int num = (isEvolve ? _actingCard.EvolutionSkills.Where((SkillBase s) => s.IsBurialRite).Count() : _actingCard.Skills.Where((SkillBase s) => s.IsBurialRite).Count());
			_isBurialRiteSelect = _currentSkill.IsBurialRite && _selectedCards.Count < num;
			List<BattleCardBase> selectableCards = GetSelectableCards(isPlayer, skillConditionCheckerOption, _selectedCards);
			sequentialVfxPlayer.Register(GetSelectAlertvfx(battlePlayer));
			sequentialVfxPlayer.Register(battlePlayer.BattleView.StartShowSelect(_actingCard, _currentSkill, selectableCards, isEvolve));
		}
		else
		{
			bool num2 = _selectedChoiceCards.Any();
			BattleCardBase actingCard = ((num2 && !isChoiceBrave) ? _actingChoiceCard : _actingCard);
			List<int> selectedChoiceCardIds = null;
			if (num2)
			{
				selectedChoiceCardIds = GetSelectedChoiceCardIds();
				BattleCardBase choiceCard = _selectedChoiceCards.First();
				ChoiceUtility.SetupActingChoiceCardToBePlayedFromQueue(actingCard, choiceCard, battlePlayer, isChoiceBrave);
			}
			sequentialVfxPlayer.Register(CreateAfterSelectVfx(actingCard, selectedChoiceCardIds, isPlayer, isChoiceBrave));
		}
		return sequentialVfxPlayer;
	}

	public virtual void SelectFusionIngredientCard(int cardIndex, bool isPlayer = true)
	{
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isPlayer);
		BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, battlePlayer, cardIndex);
		_selectableCards.IndexOf(indexToCardBase);
		int skillSelectCount = _currentSkill.GetSkillSelectCount();
		if (skillSelectCount < 8)
		{
			if (_selectedCards.Contains(indexToCardBase))
			{
				ChangeSelectFusionIngredientCard(battlePlayer, indexToCardBase, isSelect: false, skillSelectCount);
			}
			else if (_selectedCards.Count < skillSelectCount)
			{
				ChangeSelectFusionIngredientCard(battlePlayer, indexToCardBase, isSelect: true, skillSelectCount);
			}
			else if (_selectedCards.Count == skillSelectCount && skillSelectCount == 1)
			{
				_selectedCards.Clear();
				ChangeSelectFusionIngredientCard(battlePlayer, indexToCardBase, isSelect: true, skillSelectCount);
			}
		}
		else if (_selectedCards.Contains(indexToCardBase))
		{
			ChangeSelectFusionIngredientCard(battlePlayer, indexToCardBase, isSelect: false, skillSelectCount);
		}
		else
		{
			ChangeSelectFusionIngredientCard(battlePlayer, indexToCardBase, isSelect: true, skillSelectCount);
		}
	}

	public virtual void CompleteSelectFusionIngredientCard(bool isPlayer)
	{
		InstantVfx selectCardVfx = InstantVfx.Create(delegate
		{
			_battleMgr.GetBattlePlayer(isPlayer).ClassInformationUIController.SetIsSelect(isSelect: false);
		});
		_actingCard.SelfBattlePlayer.BattleView.StopFusionUI();
		CompleteSelectDataIns = new CompleteSelectData(selectCardVfx);
	}

	private void ChangeSelectFusionIngredientCard(BattlePlayerBase battlePlayer, BattleCardBase selectedCard, bool isSelect, int maxSelectCount)
	{
		int index = _selectableCards.IndexOf(selectedCard);
		if (isSelect)
		{
			_selectedCards.Add(selectedCard);
		}
		else
		{
			_selectedCards.Remove(selectedCard);
		}
		battlePlayer.BattleView.SelectedFusionIngredientCard(index, isSelect, maxSelectCount);
		if (_fusionMetamorphoseSkill != null)
		{
			_actingCard.BattleCardView.ShowFusionMetamorphoseFrameEffect(_fusionMetamorphoseSkill.IsShowFusionMetamorphoseFrameEffect(_selectedCards));
		}
	}

	private VfxBase GetSelectAlertvfx(BattlePlayerBase battlePlayer)
	{
		return InstantVfx.Create(delegate
		{
			string text = "";
			if (_battleMgr.GameMgr.IsWatchBattle)
			{
				SystemText systemText = Data.SystemText;
				text = (battlePlayer.IsPlayer ? systemText.Get("Battle_0501") : systemText.Get("Battle_0502"));
			}
			if (text != "")
			{
				battlePlayer.BattleView.ShowAlert(PanelMgr.BattleAlertType.SelectChoiceCard, isClass: false, text);
			}
		});
	}

	private BattleCardBase GetSelectedChoiceCard(int selectedChoiceCardId, List<BattleCardBase> choiceCards)
	{
		return choiceCards.Find((BattleCardBase card) => card.CardId == selectedChoiceCardId);
	}

	private List<int> GetSelectedChoiceCardIds()
	{
		return _selectedChoiceCards.Select((BattleCardBase card) => card.CardId).ToList();
	}

	private void ToggleSelectedCardButtonSprite(int selectedChoiceCardIndex, bool setActive, BattlePlayerBase battlePlayer, bool isFusion = false)
	{
		GameObject checkFromIndex = battlePlayer.BattleView.GetCheckFromIndex(selectedChoiceCardIndex);
		if (!(checkFromIndex == null))
		{
			int numberOfCardsToSelect = 0;
			if (!isFusion)
			{
				numberOfCardsToSelect = ChoiceUtility.GetNumberOfCardsToSelect(_choiceSkill);
			}
			ChoiceUtility.ToggleChoiceButtonSprite(null, checkFromIndex, setActive, numberOfCardsToSelect, isFusion);
		}
	}

	protected List<BattleCardBase> GetSelectableCards(bool isPlayer = true, SkillConditionCheckerOption option = null, List<BattleCardBase> selectedCards = null)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerInfoPair = _battleMgr.GetBattlePlayerInfoPair(isPlayer);
		List<BattleCardBase> list;
		if (_isBurialRiteSelect)
		{
			list = SkillPreprocessBurialRite.GetBurialRiteTarget(_actingCard.SelfBattlePlayer, _actingCard);
			if (selectedCards != null && selectedCards.Count > 0)
			{
				list.RemoveAll((BattleCardBase c) => selectedCards.Contains(c));
			}
		}
		else
		{
			IEnumerable<BattleCardBase> skillUserSelectableTargets = ActionProcessor.GetSkillUserSelectableTargets(_currentSkill, battlePlayerInfoPair, (option != null) ? option : new SkillConditionCheckerOption(), selectedCards);
			if (skillUserSelectableTargets != null)
			{
				list = skillUserSelectableTargets.ToList();
			}
			else
			{
				list = new List<BattleCardBase>();
				LocalLog.AccumulateTraceLog("702988 " + _actingCard.CardId);
			}
		}
		if (_currentSkill.OnWhenEvolveStart == 0 && list.Any((BattleCardBase c) => c == _actingCard) && !(_actingCard is ClassBattleCardBase))
		{
			list.Remove(_actingCard);
		}
		return list;
	}

	public virtual void RecordSelectStart(BattleCardBase receivedCard, BattleCardBase choiceTransformCard = null)
	{
	}

	public virtual void RecordSelectCard(BattleCardBase targetCard, bool isBurialRiteSelect)
	{
	}

	public virtual void RecordCompleteSelect(BattleCardBase targetCard, bool isBurialRiteSelect, bool isChoiceBraveSelect)
	{
	}

	public virtual void RecordCancelSelect()
	{
	}

	public virtual void RecordStartChoiceSelect(BattleCardBase playedCard)
	{
	}

	public virtual void RecordCompleteChoiceSelect(List<int> choiceIdList)
	{
	}

	public virtual void RecordCancelChoice()
	{
	}

	public virtual void RecordStartFusion(BattleCardBase fusionCard)
	{
	}

	public virtual void RecordSelectFusion(BattleCardBase targetCard)
	{
	}

	public void ClearData()
	{
		_actingCard = null;
		_selectSkills = new List<SkillBase>();
		_currentSkill = null;
		_isBurialRiteSelect = false;
		_selectedCards = new List<BattleCardBase>();
		_selectableCards = new List<BattleCardBase>();
	}
}
