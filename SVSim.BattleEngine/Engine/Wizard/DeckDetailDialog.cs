using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.DeckCardEdit;
using Wizard.UI.Dialog.ImageSelection;

namespace Wizard;

public class DeckDetailDialog : MonoBehaviour
{
	private enum eTaskType
	{
		Normal,
		Convention,
		Gathering
	}

	[SerializeField]
	private UILabel _deckNameLabel;

	[SerializeField]
	private UIButton _deckNameEditButton;

	[SerializeField]
	private UIButton _deckCodeCreateButton;

	[SerializeField]
	private UITexture _skinTexture;

	[SerializeField]
	private ClassInfoParts _skinInfo;

	[SerializeField]
	private UIButton _skinChangeButton;

	[SerializeField]
	private UISprite _skinRandomIcon;

	[SerializeField]
	private UITexture[] _sleeveTextureList;

	[SerializeField]
	private UIButton _sleeveChangeButton;

	[SerializeField]
	private CostCurveUI _costCurve;

	[SerializeField]
	private ImageSelection _skinSelectionPrefab;

	[SerializeField]
	private FilteringSleeveSelection _sleeveSelectionPrefab;

	[SerializeField]
	private FlexibleGrid _classDisplayGrid;

	private Action _onDeckUpdateSuccess;

	private DeckData _deck;

	private ImageSelection _skinSelection;

	private List<string> _loadSkinPathList = new List<string>();

	private int _currentSkinId;

	private string _currentSkinPath;

	private List<int> _selectRandomSkinIdList;

	private List<long> _loadedSleeveId = new List<long>();

	private List<string> _loadedSleeveTexturePath = new List<string>();

	private ConventionDeckList _conventionDeckList;

	private List<ClassCharacterMasterData> _usableSkinList;

	private List<string> _loadedVoiceCueSheetList = new List<string>();

	private FilteringImageSelection _sleeveSelection;

	private bool _isChangingSleeve;

	private long _oldSleeveId = -1L;

	private Action<string> _startConnectDeckNameUpdateTask;

	private Action<int> _startConnectDeckSkinUpdateTask;

	private Action<List<int>> _startConnectDeckRandomSkinUpdateTask;

	private Action<long> _startConnectDeckSleeveUpdateTask;

	public DeckData Deck => _deck;

	public void Initialize(DeckData deck, Action onDeckUpdateSuccess, List<string> loadedVoiceList, ConventionDeckList conventionDeckList)
	{
		eTaskType taskType = ((conventionDeckList != null) ? eTaskType.Convention : eTaskType.Normal);
		InitializeCommon(deck, taskType, onDeckUpdateSuccess, loadedVoiceList, conventionDeckList);
	}

	private void InitializeCommon(DeckData deck, eTaskType taskType, Action onDeckUpdateSuccess, List<string> loadedVoiceList, ConventionDeckList conventionDeckList)
	{
		_deck = deck;
		_onDeckUpdateSuccess = onDeckUpdateSuccess;
		_loadedVoiceCueSheetList = loadedVoiceList;
		_conventionDeckList = conventionDeckList;
		CardMaster.CardMasterId cardMasterId = FormatBehaviorManager.Create(deck.Format, conventionDeckList).CardMasterId;
		UpdateDeckName();
		UpdateSkin();
		_costCurve.Initialize(cardMasterId);
		_costCurve.Refresh(_deck.GetCardIdList().ToArray());
		UIManager.SetObjectToGrey(_deckCodeCreateButton.gameObject, !_deck.GetDeckIsComplete());
		if (_deck.Format == Format.Avatar)
		{
			DisableEditDeck();
		}
		SetActionDeckUpdateTasks(taskType);
		UIEventListener.Get(_deckNameEditButton.gameObject).onClick = OnClickDeckNameEditButton;
		UIEventListener.Get(_deckCodeCreateButton.gameObject).onClick = OnClickDeckCodeCreateButton;
		UIEventListener.Get(_skinChangeButton.gameObject).onClick = OnClickSkinChangeButton;
		UIEventListener.Get(_sleeveChangeButton.gameObject).onClick = OnClickSleeveChangeButton;
		if (CustomPreference.GetTextLanguage() == Global.LANG_TYPE.Kor.ToString() || CustomPreference.GetTextLanguage() == Global.LANG_TYPE.Cht.ToString())
		{
			_skinInfo.SetCharacterNameHeight(31);
		}
		LoadSleeve(_deck.GetDeckSleeveID(), delegate
		{
			StartCoroutine(UpdateSleeveTexture());
		});
		InitSleeveSelection();
	}

	private void SetActionDeckUpdateTasks(eTaskType type)
	{
		switch (type)
		{
		case eTaskType.Normal:
			_startConnectDeckNameUpdateTask = delegate(string newDeckName)
			{
				DeckNameUpdateTask deckNameUpdateTask = new DeckNameUpdateTask();
				deckNameUpdateTask.SetParameter(_deck.GetDeckID(), newDeckName, _deck.Format);
				StartCoroutine(Toolbox.NetworkManager.Connect(deckNameUpdateTask, delegate
				{
					OnSuccessDeckNameUpdate(newDeckName);
				}));
			};
			_startConnectDeckSkinUpdateTask = delegate(int skinId)
			{
				DeckLeaderSkinUpdateTask deckLeaderSkinUpdateTask = new DeckLeaderSkinUpdateTask();
				deckLeaderSkinUpdateTask.SetParameter(_deck.GetDeckID(), skinId, _deck.Format);
				StartCoroutine(Toolbox.NetworkManager.Connect(deckLeaderSkinUpdateTask, delegate
				{
					OnSuccessChangeSkin(skinId);
				}));
			};
			_startConnectDeckRandomSkinUpdateTask = delegate(List<int> skinIdList)
			{
				DeckRandomLeaderSkinUpdateTask task = new DeckRandomLeaderSkinUpdateTask();
				task.SetParameter(_deck.Format, _deck.GetDeckID(), skinIdList.ToArray());
				StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
				{
					OnSuccessChangeRandomSkin(task.SelectedSkinId, skinIdList);
				}));
			};
			_startConnectDeckSleeveUpdateTask = delegate(long sleeveId)
			{
				DeckUpdateSleeveTask deckUpdateSleeveTask = new DeckUpdateSleeveTask();
				deckUpdateSleeveTask.SetParameter(_deck.GetDeckID(), sleeveId, _deck.Format);
				StartCoroutine(Toolbox.NetworkManager.Connect(deckUpdateSleeveTask, delegate
				{
					OnSuccessChangeSleeve(sleeveId);
				}));
			};
			break;
		case eTaskType.Convention:
			_startConnectDeckNameUpdateTask = delegate(string newDeckName)
			{
				DeckConventionNameUpdateTask deckConventionNameUpdateTask = new DeckConventionNameUpdateTask();
				deckConventionNameUpdateTask.SetParameter(_deck.GetDeckID(), newDeckName, _conventionDeckList);
				StartCoroutine(Toolbox.NetworkManager.Connect(deckConventionNameUpdateTask, delegate
				{
					OnSuccessDeckNameUpdate(newDeckName);
				}));
			};
			_startConnectDeckSkinUpdateTask = delegate(int skinId)
			{
				DeckConventionLeaderSkinUpdateTask deckConventionLeaderSkinUpdateTask = new DeckConventionLeaderSkinUpdateTask();
				deckConventionLeaderSkinUpdateTask.SetParameter(_deck.GetDeckID(), skinId, _conventionDeckList);
				StartCoroutine(Toolbox.NetworkManager.Connect(deckConventionLeaderSkinUpdateTask, delegate
				{
					OnSuccessChangeSkin(skinId);
				}));
			};
			_startConnectDeckRandomSkinUpdateTask = delegate(List<int> skinIdList)
			{
				DeckConventionRandomLeaderSkinUpdateTask task = new DeckConventionRandomLeaderSkinUpdateTask();
				task.SetParameter(_deck.GetDeckID(), skinIdList.ToArray(), _conventionDeckList);
				StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
				{
					OnSuccessChangeRandomSkin(task.SelectedSkinId, skinIdList);
				}));
			};
			_startConnectDeckSleeveUpdateTask = delegate(long sleeveId)
			{
				DeckConventionUpdateSleeveTask deckConventionUpdateSleeveTask = new DeckConventionUpdateSleeveTask();
				deckConventionUpdateSleeveTask.SetParameter(_deck.GetDeckID(), sleeveId, _conventionDeckList);
				StartCoroutine(Toolbox.NetworkManager.Connect(deckConventionUpdateSleeveTask, delegate
				{
					OnSuccessChangeSleeve(sleeveId);
				}));
			};
			break;
		case eTaskType.Gathering:
			_startConnectDeckNameUpdateTask = delegate(string newDeckName)
			{
				GatheringUpdateDeckName gatheringUpdateDeckName = new GatheringUpdateDeckName();
				gatheringUpdateDeckName.SetParameter(_deck.GetDeckID(), newDeckName);
				StartCoroutine(Toolbox.NetworkManager.Connect(gatheringUpdateDeckName, delegate
				{
					OnSuccessDeckNameUpdate(newDeckName);
				}));
			};
			_startConnectDeckSkinUpdateTask = delegate(int skinId)
			{
				GatheringUpdateDeckLeaderSkin gatheringUpdateDeckLeaderSkin = new GatheringUpdateDeckLeaderSkin();
				gatheringUpdateDeckLeaderSkin.SetParameter(_deck.GetDeckID(), skinId);
				StartCoroutine(Toolbox.NetworkManager.Connect(gatheringUpdateDeckLeaderSkin, delegate
				{
					OnSuccessChangeSkin(skinId);
				}));
			};
			_startConnectDeckRandomSkinUpdateTask = delegate(List<int> skinIdList)
			{
				GatheringUpdateDeckRandomLeaderSkinTask task = new GatheringUpdateDeckRandomLeaderSkinTask();
				task.SetParameter(_deck.GetDeckID(), skinIdList.ToArray());
				StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
				{
					OnSuccessChangeRandomSkin(task.SelectedSkinId, skinIdList);
				}));
			};
			_startConnectDeckSleeveUpdateTask = delegate(long sleeveId)
			{
				GatheringUpdateDeckSleeve gatheringUpdateDeckSleeve = new GatheringUpdateDeckSleeve();
				gatheringUpdateDeckSleeve.SetParameter(_deck.GetDeckID(), sleeveId);
				StartCoroutine(Toolbox.NetworkManager.Connect(gatheringUpdateDeckSleeve, delegate
				{
					OnSuccessChangeSleeve(sleeveId);
				}));
			};
			break;
		}
	}

	public void SetDeck(DeckData deck)
	{
		_deck = deck;
	}

	public int GetDeckId()
	{
		return _deck.GetDeckID();
	}

	public void Final()
	{
		if (_currentSkinPath != null)
		{
			Toolbox.ResourcesManager.RemoveAsset(_currentSkinPath);
			_currentSkinPath = null;
		}
		Toolbox.ResourcesManager.RemoveAssetGroup(_loadSkinPathList);
		_loadSkinPathList.Clear();
		UnloadAllSleeves();
	}

	private void OnClickDeckNameEditButton(GameObject g)
	{

		string oldDeckName = _deck.GetDeckName();
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = InputDialog.Create(30, 24);
		dialogBase.SetTitleLabel(systemText.Get("Card_0011"));
		UILabel[] labels = dialogBase.InputAreaObjs.labels;
		dialogBase.onPushButton1 = delegate
		{
			string text = labels[0].text;
			if (string.IsNullOrEmpty(text))
			{
				text = oldDeckName;
			}
			if (oldDeckName != text)
			{
				_startConnectDeckNameUpdateTask.Call(text);
			}
		};
		dialogBase.SetPanelDepth(400);
		labels[0].text = oldDeckName;
		labels[2].text = systemText.Get("Card_0012");
		labels[3].text = systemText.Get("Common_0401", 24.ToString());
	}

	private void OnSuccessDeckNameUpdate(string deckName)
	{
		_deck.SetDeckName(deckName);
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_003_Title"));
		dialogBase.SetText(systemText.Get("Card_0014"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		dialogBase.SetPanelDepth(400);
		SaveLastEditDeck();
		_onDeckUpdateSuccess.Call();
		DeckCardEditUI.CurrentDeckName = _deck.GetDeckName();
		UpdateDeckName();
	}

	public void UpdateDeckName()
	{
		_deckNameLabel.text = _deck.GetDeckName();
	}

	private void OnClickDeckCodeCreateButton(GameObject g)
	{

		GenerateDeckCodeTask generateDeckCodeTask = new GenerateDeckCodeTask();
		SetGenerateDeckCodeTask(generateDeckCodeTask);
		StartCoroutine(Toolbox.NetworkManager.Connect(generateDeckCodeTask, OnSuccessCreateDeckCode, null, null, encrypt: false));
	}

	private void SetGenerateDeckCodeTask(GenerateDeckCodeTask task)
	{
		IFormatBehavior defaultBehaviour = FormatBehaviorManager.GetDefaultBehaviour(_deck.Format);
		if (defaultBehaviour.UseSubClass)
		{
			task.SetParameter(_deck.GetDeckClassID(), _deck.GetDeckSubClassID(), defaultBehaviour.DeckCodeType, _deck.GetCardIdList().ToArray());
		}
		else if (_deck.Format == Format.MyRotation)
		{
			task.SetParameterMyRotation(_deck, defaultBehaviour.DeckCodeType);
		}
		else
		{
			task.SetParameter(_deck.GetDeckClassID(), GenerateDeckCodeTask.SubmitDeckType.NORMAL, _deck.GetCardIdList().ToArray());
		}
	}

	private void OnSuccessCreateDeckCode(NetworkTask.ResultCode code)
	{
		string deckCode = Data.GenerateDeckCode.deck_code;
		SystemText text = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetPanelDepth(400);
		dialogBase.SetTitleLabel(text.Get("Card_0120"));
		dialogBase.SetText(text.Get("Card_0128", deckCode));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_GrayBtn);
		dialogBase.SetButtonText(text.Get("Card_0133"), text.Get("Common_0008"));
		dialogBase.onPushButton1 = delegate
		{
			NativePluginWrapper.SetStringToClipboard(deckCode);
			UIManager.GetInstance().CreateConfirmationDialog(text.Get("Card_0132", deckCode)).SetPanelDepth(400);
		};
	}

	private void OnClickSkinChangeButton(GameObject g)
	{

		StartCoroutine(OpenSkinSelectionDialog());
	}

	private IEnumerator OpenSkinSelectionDialog()
	{
		yield return StartCoroutine(CreateSkinSelection());
		string key = (_deck.IsSkinRandom ? "skin_random" : _deck.GetRawSkinId().ToString());
		int displayPage = _skinSelection.SelectItemWithKey(key);
		_skinSelection.SetDisplayPage(displayPage);
		DialogBase dialogBase = DialogBase.CreateImageSelectionDialog(_skinSelection, "Profile_0029", DialogBase.Size.L);
		dialogBase.SetPanelDepth(400);
		dialogBase.onPushButton1 = (Action)Delegate.Combine(dialogBase.onPushButton1, (Action)delegate
		{
			string selectedItemKey = _skinSelection.GetSelectedItemKey();
			if (selectedItemKey == "skin_random")
			{
				if (!_deck.IsSkinRandom || !_deck.SelectRandomSkinIdList.SequenceEqual(_selectRandomSkinIdList))
				{
					_startConnectDeckRandomSkinUpdateTask.Call(_selectRandomSkinIdList);
				}
			}
			else
			{
				int num = int.Parse(selectedItemKey);
				if (_deck.IsSkinRandom || (!string.IsNullOrEmpty(selectedItemKey) && _deck.GetRawSkinId() != num))
				{
					_startConnectDeckSkinUpdateTask.Call(num);
				}
			}
		});
		List<int> initSelectRandomSkinIdList = new List<int>(_selectRandomSkinIdList);
		dialogBase.onCloseWithoutSelect = (Action)Delegate.Combine(dialogBase.onCloseWithoutSelect, (Action)delegate
		{
			_selectRandomSkinIdList = initSelectRandomSkinIdList;
		});
	}

	private IEnumerator CreateSkinSelection()
	{
		if (!(_skinSelection != null))
		{
			CreateSkinSelectionMain();
			UIManager uiMgr = UIManager.GetInstance();
			uiMgr.createInSceneCenterLoading();
			yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(_loadSkinPathList, null));
			uiMgr.closeInSceneCenterLoading();
		}
	}

	private void CreateSkinSelectionMain()
	{
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		ResourcesManager.AssetLoadPathType type = ResourcesManager.AssetLoadPathType.ClassCharaSkinThumbnail;
		SystemText systemText = Data.SystemText;
		_skinSelection = NGUITools.AddChild(base.gameObject, _skinSelectionPrefab.gameObject).GetComponent<ImageSelection>();
		_skinSelection.gameObject.SetActive(value: false);
		_skinSelection.Create(400);
		_skinSelection.AddItem(0.ToString(), null, isSelectable: true, null, null, null, isDisplaySprite: true, string.Empty, new string[2]
		{
			systemText.Get("Profile_0017"),
			systemText.Get("Card_0182")
		}, null);
		_selectRandomSkinIdList = new List<int>(_deck.SelectRandomSkinIdList);
		if (_selectRandomSkinIdList.Contains(0))
		{
			_selectRandomSkinIdList.Remove(0);
			// Pre-Phase-5b: chara master lookup dropped; headless has no default skin to add
				_selectRandomSkinIdList.Sort();
			}
			_skinSelection.AddItem("skin_random", null, isSelectable: true, null, null, null, isDisplaySprite: true, string.Empty, new string[2]
			{
				systemText.Get("Card_0256"),
				systemText.Get("Card_0257")
			}, null, delegate
			{
				CreateRandomSkinSelectDialog();
			});
			int classId = _deck.GetDeckClassID();
			_usableSkinList = Data.Master.ClassCharacterList.Where((ClassCharacterMasterData x) => x.is_usable && x.IsAcquired && x.class_id == classId).ToList();
			for (int num = 0; num < _usableSkinList.Count; num++)
			{
				ClassCharacterMasterData charaPrm = _usableSkinList[num];
				string text = charaPrm.skin_id.ToString();
				_skinSelection.AddItem(text, null, isSelectable: true, () => charaPrm.IsNew, null, resourcesManager.GetAssetTypePath(text, type, isfetch: true), isDisplaySprite: false, charaPrm.chara_name, null, delegate
				{
					charaPrm.UnsetNew();
				});
				_loadSkinPathList.Add(resourcesManager.GetAssetTypePath(text, type));
			}
		}

		private void CreateRandomSkinSelectDialog()
		{
			SelectRandomSkinDialog.Create(_usableSkinList.Select((ClassCharacterMasterData x) => x.skin_id).ToList(), _selectRandomSkinIdList, delegate(List<int> idList)
			{
				_selectRandomSkinIdList = idList;
			}).SetPanelDepth(600);
		}

		private void OnSuccessChangeSkin(int skinId)
		{
			_deck.SetSkinId(skinId);
			_deck.IsSkinRandom = false;
			SuccessChangeSkin();
		}

		private void OnSuccessChangeRandomSkin(int skinId, List<int> skinRandomIdList)
		{
			_deck.SetSkinId(skinId);
			_deck.IsSkinRandom = true;
			_deck.SelectRandomSkinIdList = skinRandomIdList;
			SuccessChangeSkin();
		}

		private void SuccessChangeSkin()
		{
			SaveLastEditDeck();
			_onDeckUpdateSuccess.Call();
			UpdateSkin();
			DeckCardEditUI.CurrentDeckData = _deck;
			StopVoice();

		}

		private void StopVoice()
		{
		}

		private void UpdateSkin()
		{
			int currentSkinId = _currentSkinId;
			int newSkinId = _deck.GetSkinId();
			/* Pre-Phase-5b: class prm read dropped */
			_skinRandomIcon.gameObject.SetActive(_deck.IsVisibleRandomIcon());
			if (newSkinId != currentSkinId)
			{
				ResourcesManager resMgr = Toolbox.ResourcesManager;
				if (_currentSkinPath != null)
				{
					resMgr.RemoveAsset(_currentSkinPath);
				}
				ResourcesManager.AssetLoadPathType skinLoadType = ResourcesManager.AssetLoadPathType.ClassCharaSkinThumbnail;
				_currentSkinId = newSkinId;
				_currentSkinPath = resMgr.GetAssetTypePath(newSkinId.ToString(), skinLoadType);
				_skinInfo.InitClassByDeckData(_deck);
				if (FormatBehaviorManager.GetDefaultBehaviour(_deck.Format).UseSubClass)
				{
					_skinInfo.SetSubClass((CardBasePrm.ClanType)_deck.GetDeckSubClassID());
				}
				UIUtil.AdjustClassInfoPartsSize(_skinInfo, _classDisplayGrid, 375);
				UIManager uiMgr = UIManager.GetInstance();
				uiMgr.createInSceneCenterLoading();
				_skinTexture.mainTexture = null;
				StartCoroutine(resMgr.LoadAssetGroupAsync(new List<string> { _currentSkinPath }, delegate
				{
					uiMgr.closeInSceneCenterLoading();
					_skinTexture.mainTexture = resMgr.LoadObject(resMgr.GetAssetTypePath(newSkinId.ToString(), skinLoadType, isfetch: true)) as Texture;
				}));
			}
		}

		private void OnClickSleeveChangeButton(GameObject g)
		{

			UIManager.GetInstance().createInSceneCenterLoading();
			OpenSleeveSelectionDialog();
		}

		private void LoadSleeve(long sleeveId, Action onFinish = null)
		{
			List<string> first = CollectSleeveResourcePaths(sleeveId);
			List<string> loadPathList = first.Except(_loadedSleeveTexturePath).ToList();
			if (loadPathList.Count == 0)
			{
				onFinish.Call();
				return;
			}
			StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(loadPathList, delegate
			{
				_loadedSleeveId.Add(sleeveId);
				_loadedSleeveTexturePath.AddRange(loadPathList);
				onFinish.Call();
			}));
		}

		private void UnloadSleeve(long sleeveId)
		{
			List<string> list = CollectSleeveResourcePaths(sleeveId);
			_loadedSleeveId.Remove(sleeveId);
			_loadedSleeveTexturePath = _loadedSleeveTexturePath.Except(list).ToList();
			Toolbox.ResourcesManager.RemoveAssetGroup(list);
		}

		private void UnloadAllSleeves()
		{
			Toolbox.ResourcesManager.RemoveAssetGroup(_loadedSleeveTexturePath);
			_loadedSleeveId.Clear();
			_loadedSleeveTexturePath.Clear();
		}

		private List<string> CollectSleeveResourcePaths(long sleeveId)
		{
			sleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(sleeveId);
			string path = sleeveId.ToString();
			List<string> loadPath = new List<string>();
			loadPath.Add(Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.SleeveTexture));
			Sleeve sleeve = Data.Master.SleeveMgr.Get(sleeveId);
			if (sleeve.IsPremiumSleeve)
			{
				UIManager.GetInstance().getUIBase_CardManager().AddPremireSleevePath(ref loadPath, sleeve);
			}
			return loadPath;
		}

		private void InitSleeveSelection()
		{
			_sleeveSelection = NGUITools.AddChild(base.gameObject, _sleeveSelectionPrefab.gameObject).GetComponent<FilteringImageSelection>();
			_sleeveSelection.gameObject.SetActive(value: false);
			ResourcesManager resourcesManager = Toolbox.ResourcesManager;
			List<Sleeve> acquiredList = Data.Master.SleeveMgr.GetAcquiredList();
			List<SleeveCategory> list = Data.Master.SleeveCategoryIdDic.Values.OrderBy((SleeveCategory category) => category.Id).ToList();
			_sleeveSelection.Initialize(acquiredList.Count, list.Count);
			foreach (SleeveCategory item in list)
			{
				_sleeveSelection.AddSeries(item.Id, item.Name);
			}
			foreach (Sleeve sleeve in acquiredList)
			{
				string key = sleeve.sleeve_id.ToString();
				long existingSleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(sleeve.sleeve_id);
				Sleeve sleeve2 = Data.Master.SleeveMgr.Get(existingSleeveId);
				List<string> loadPath = new List<string>();
				loadPath.Add(resourcesManager.GetAssetTypePath(existingSleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTexture));
				if (sleeve2.IsPremiumSleeve)
				{
					UIManager.GetInstance().getUIBase_CardManager().AddPremireSleevePath(ref loadPath, sleeve2);
				}
				_sleeveSelection.AddItem(key, sleeve._categoryId, isSelectable: true, loadPath, null, isDisplaySprite: false, sleeve.sleeve_name, null, () => sleeve.IsNew, delegate
				{
					Data.Master.SleeveMgr.UnsetNew(sleeve.sleeve_id);
				}, null, null, sleeve.IsFavorite);
			}
			_sleeveSelection.SelectItemWithKey(_deck.GetDeckSleeveID().ToString());
		}

		private void OpenSleeveSelectionDialog()
		{
			_isChangingSleeve = false;
			DialogBase dialogBase = DialogBase.CreateFilteringImageSelectionDialog(_sleeveSelection, "Card_0146");
			dialogBase.SetPanelDepth(400);
			dialogBase.onPushButton1 = (Action)Delegate.Combine(dialogBase.onPushButton1, (Action)delegate
			{
				if (long.TryParse(_sleeveSelection.GetSelectedItemKey(), out var result))
				{
					_oldSleeveId = _deck.GetDeckSleeveID();
					if (result != _oldSleeveId)
					{
						LoadSleeve(result);
						_startConnectDeckSleeveUpdateTask.Call(result);
						_isChangingSleeve = true;
					}
				}
			});
			dialogBase.OnClose = (Action)Delegate.Combine(dialogBase.OnClose, (Action)delegate
			{
				if (!_isChangingSleeve)
				{
					OnCloseSleeveSelection();
				}
			});
		}

		private void OnSuccessChangeSleeve(long sleeveId)
		{
			_deck.SetDeckSleeveID(sleeveId);
			_isChangingSleeve = false;
			MyPageMenu.SetEnableReloadCard();
			SaveLastEditDeck();
			_onDeckUpdateSuccess.Call();
			StartCoroutine(UpdateSleeveTexture());
			DeckCardEditUI.CurrentDeckData = _deck;
		}

		private void OnCloseSleeveSelection()
		{
			_sleeveSelection.SelectItemWithKey(_deck.GetDeckSleeveID().ToString());
		}

		private IEnumerator UpdateSleeveTexture()
		{
			long sleeveId = _deck.GetDeckSleeveID();
			while (!_loadedSleeveId.Contains(sleeveId))
			{
				yield return null;
			}
			for (int i = 0; i < _sleeveTextureList.Length; i++)
			{
				UIManager.GetInstance().getUIBase_CardManager().SetSleeveTexture(_sleeveTextureList[i], sleeveId);
			}
			if (_oldSleeveId != sleeveId && _oldSleeveId != -1)
			{
				UnloadSleeve(_oldSleeveId);
			}
		}

		private void SaveLastEditDeck()
		{
			DeckListUtility.SaveLastSelectDeck(_deck.GetDeckID(), isDefaultDeck: false, isTrialDeck: false, _deck.Format);
		}

		private void DisableEditDeck()
		{
			UIManager.SetObjectToGrey(_deckCodeCreateButton.gameObject, b: true);
			UIManager.SetObjectToGrey(_deckNameEditButton.gameObject, b: true);
			UIManager.SetObjectToGrey(_sleeveChangeButton.gameObject, b: true);
			UIManager.SetObjectToGrey(_skinChangeButton.gameObject, b: true);
		}
	}
