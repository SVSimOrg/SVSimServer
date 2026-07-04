using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.ErrorDialog;

namespace Wizard.DeckCardEdit;

public class DeckSave
{
	public class DeckSaveCoroutineMonoBehaviour : MonoBehaviour
	{
		private DeckSaveCoroutineMonoBehaviour()
		{
		}
	}

	public class Option
	{
		public int[] CardIds;

		public int DeckId;

		public string DeckName;

		public int ClassType;

		public int SubClassType;

		public int RawSkinId;

		public bool IsRandomLeaderSkin;

		public List<int> LeaderSkinIdList;

		public long SleeveId;

		public bool IsNew;

		public Format Format;

		public string MyRotationId;

		public ConventionDeckList ConventionDeckList;

		public Action OnFinish;

		public Action<bool> OnSaveCompleteDialog;
	}

	private Action _onFinish;

	private int[] _deck;

	private int _id;

	private string _name;

	private string _defaultName;

	private int _class;

	private int _rawSkinId;

	private bool _isRandomLeaderSkin;

	private List<int> _leaderSkinIdList;

	private long _sleeveId;

	private Format _format;

	private ConventionDeckList _conventionDeckList;

	private bool _isNew;

	private string _myRotationId;

	private int _subClass;

	private MonoBehaviour _behaviour;

	private DialogBase _nameEditDialog;

	private IFormatBehavior _formatBehavior;

	private Action<bool> _saveCompleteDialogOverrideAction;

	private bool _isUsableDeck;

	public void Destroy()
	{
		if ((bool)_behaviour)
		{
			UnityEngine.Object.Destroy(_behaviour.gameObject);
		}
	}

	public void Start(Option option)
	{
		IFormatBehavior formatBehavior = FormatBehaviorManager.Create(option.Format, option.ConventionDeckList);
		_deck = option.CardIds;
		_name = option.DeckName;
		_defaultName = option.DeckName;
		_id = option.DeckId;
		_format = option.Format;
		_isNew = option.IsNew;
		_class = option.ClassType;
		_subClass = option.SubClassType;
		_rawSkinId = option.RawSkinId;
		_isRandomLeaderSkin = option.IsRandomLeaderSkin;
		_leaderSkinIdList = option.LeaderSkinIdList;
		_sleeveId = option.SleeveId;
		_conventionDeckList = option.ConventionDeckList;
		_myRotationId = option.MyRotationId;
		_onFinish = option.OnFinish;
		_saveCompleteDialogOverrideAction = option.OnSaveCompleteDialog;
		MyRotationInfo myRotationInfo = null;
		if (!string.IsNullOrEmpty(_myRotationId))
		{
			myRotationInfo = Data.MyRotationAllInfo.Get(_myRotationId);
		}
		_formatBehavior = formatBehavior;
		if (_deck.Length < 40)
		{
			CreateSaveConfirmDialog(Data.SystemText.Get("Card_0050", 40.ToString()));
			return;
		}
		if (_deck.Length > 50)
		{
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetTitleLabel(Data.SystemText.Get("Dia_DeckEdit_006_Title"));
			dialogBase.SetText(Data.SystemText.Get("Card_0107", 40.ToString()));
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			dialogBase.OnCloseStart = Destroy;
			return;
		}
		if (_deck.Length > 40)
		{
			CreateSaveConfirmDialog(Data.SystemText.Get("Card_0138", 41.ToString()));
			return;
		}
		if (_format == Format.Crossover)
		{
			CardMaster cardMaster = CardMaster.GetInstance(formatBehavior.CardMasterId);
			List<int> source = _deck.ToList();
			CardBasePrm.ClanType mainClass = (CardBasePrm.ClanType)_class;
			if (source.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == mainClass) < 24)
			{
				CreateSaveConfirmDialog(Data.SystemText.Get("Card_0279", 24.ToString()));
				return;
			}
			CardBasePrm.ClanType subClass = (CardBasePrm.ClanType)_subClass;
			if (source.Count((int cardId) => cardMaster.GetCardParameterFromId(cardId).Clan == subClass) < 9)
			{
				CreateSaveConfirmDialog(Data.SystemText.Get("Card_0280", 9.ToString()));
				return;
			}
		}
		if (DeckCardEditUI.IsNotAvailableCardInIds(new List<int>(_deck), _format, formatBehavior, myRotationInfo))
		{
			CreateSaveConfirmDialog(Data.SystemText.Get("Card_0191"));
			return;
		}
		if (DeckData.ContainsNonPossessionCard(_deck, formatBehavior))
		{
			CreateSaveConfirmDialog(Data.SystemText.Get("Card_0252"));
			return;
		}
		_isUsableDeck = true;
		CheckNewDeckBeforeSave();
	}

	private void CreateSaveConfirmDialog(string messageText)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("Dia_DeckEdit_005_Title"));
		dialogBase.SetText(messageText);
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(Data.SystemText.Get("Dia_DeckEdit_005_Button"), Data.SystemText.Get("Dia_DeckEdit_005_Button_2"));
		dialogBase.onPushButton1 = CheckNewDeckBeforeSave;
	}

	private void CheckNewDeckBeforeSave()
	{
		if (_isNew)
		{
			_nameEditDialog = InputDialog.Create(30, 24);
			_nameEditDialog.InputAreaObjs.labels[0].text = _name;
			_nameEditDialog.InputAreaObjs.labels[1].text = _id.ToString();
			_nameEditDialog.InputAreaObjs.labels[2].text = Data.SystemText.Get("Card_0012");
			_nameEditDialog.InputAreaObjs.labels[3].text = Data.SystemText.Get("Common_0401", 24.ToString());
			_nameEditDialog.SetTitleLabel(Data.SystemText.Get("Card_0011"));
			_nameEditDialog.onPushButton1 = NameSetEnd;
			_nameEditDialog.onCloseWithoutSelect = Destroy;
			_nameEditDialog.SetPanelDepth(2000);
		}
		else
		{
			SaveRequest();
		}
	}

	private void NameSetEnd()
	{
		_name = _nameEditDialog.InputAreaObjs.labels[0].text;
		if (string.IsNullOrEmpty(_name))
		{
			_name = _defaultName;
		}
		SaveRequest();
	}

	private void SaveRequest()
	{
		if (_deck.Length > 50)
		{
			return;
		}
		GameObject gameObject = new GameObject();
		_behaviour = gameObject.AddComponent<DeckSaveCoroutineMonoBehaviour>();
		if (_formatBehavior.IsConventionMode)
		{
			DeckConventionUpdateTask deckConventionUpdateTask = new DeckConventionUpdateTask();
			if (_formatBehavior.UseSubClass)
			{
				deckConventionUpdateTask.SetParameterWithSubClass(_id, _class, _subClass, _rawSkinId, _isRandomLeaderSkin, _leaderSkinIdList.ToArray(), _sleeveId, _name, is_delete: false, _deck, _conventionDeckList);
			}
			else
			{
				deckConventionUpdateTask.SetParameter(_id, _class, _rawSkinId, _isRandomLeaderSkin, _leaderSkinIdList.ToArray(), _sleeveId, _name, is_delete: false, _deck, _myRotationId, _conventionDeckList);
			}
			deckConventionUpdateTask.SkipAllCuteResultCodeCheckErrorPopup();
			_behaviour.StartCoroutine(Toolbox.NetworkManager.Connect(deckConventionUpdateTask, OnFinishSaveRequest, null, OnFailedSaveRequest));
		}
		else
		{
			DeckUpdateTask deckUpdateTask = null; // Pre-Phase-5b: no wire task headless
			if (_formatBehavior.UseSubClass)
			{
				deckUpdateTask.SetParameterWithSubClass(_id, _class, _subClass, _rawSkinId, _isRandomLeaderSkin, _leaderSkinIdList.ToArray(), _sleeveId, _name, isDelete: false, _deck, _format);
			}
			else
			{
				deckUpdateTask.SetParameter(_id, _class, _rawSkinId, _isRandomLeaderSkin, _leaderSkinIdList.ToArray(), _sleeveId, _name, is_delete: false, _deck, _format, _myRotationId);
			}
			deckUpdateTask.SkipAllCuteResultCodeCheckErrorPopup();
			_behaviour.StartCoroutine(Toolbox.NetworkManager.Connect(deckUpdateTask, OnFinishSaveRequest, null, OnFailedSaveRequest));
		}
	}

	private void OnFinishSaveRequest(NetworkTask.ResultCode error)
	{
		if (_saveCompleteDialogOverrideAction != null)
		{
			_saveCompleteDialogOverrideAction(_isUsableDeck);
		}
		else
		{
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetTitleLabel(Data.SystemText.Get("Dia_DeckEdit_007_Title"));
			dialogBase.SetText(Data.SystemText.Get("Card_0019"));
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			dialogBase.OnCloseStart = _onFinish;
		}
		Destroy();
	}

	private void OnFailedSaveRequest(int code)
	{
		DialogBase dialogBase = Wizard.ErrorDialog.Dialog.Create(code);
		if (_isNew)
		{
			dialogBase.OnCloseStart = CheckNewDeckBeforeSave;
		}
	}
}
