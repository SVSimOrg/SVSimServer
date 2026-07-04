using System;
using System.Collections.Generic;
using Cute;
using Wizard.Story;
using Wizary.StorySelectionWorld;

namespace Wizard;

public class SceneTransition
{
	public class TransitionData
	{
		public struct SceneData
		{
			public UIManager.ViewScene _scene;

			public string textId;

			public SceneData(UIManager.ViewScene scene, string id)
			{
				_scene = scene;
				textId = id;
			}
		}

		private static readonly Dictionary<string, SceneData> _transitionDictScene = new Dictionary<string, SceneData>
		{
			{
				"decks_list",
				new SceneData(UIManager.ViewScene.DeckList, null)
			},
			{
				"cards",
				new SceneData(UIManager.ViewScene.CardAllList, null)
			},
			{
				"arena",
				new SceneData(UIManager.ViewScene.MyPage, null)
			},
			{
				"2pick",
				new SceneData(UIManager.ViewScene.MyPage, null)
			},
			{
				"card_pack",
				new SceneData(UIManager.ViewScene.Gacha, "Dia_BuyCard_001_Title")
			},
			{
				"profile",
				new SceneData(UIManager.ViewScene.Profile, null)
			},
			{
				"supplies",
				new SceneData(UIManager.ViewScene.MyPage, null)
			},
			{
				"deck_pack",
				new SceneData(UIManager.ViewScene.BuildDeckPurchasePage, "MyPage_0053")
			},
			{
				"mission",
				new SceneData(UIManager.ViewScene.Mission, null)
			},
			{
				"story_leader_select",
				new SceneData(UIManager.ViewScene.StorySelectPage, null)
			},
			{
				"story_section_select",
				new SceneData(UIManager.ViewScene.StorySelectionWorld, null)
			},
			{
				"leader_skin",
				new SceneData(UIManager.ViewScene.ClassSkinPurchasePage, "Common_0143")
			},
			{
				"limited_story",
				new SceneData(UIManager.ViewScene.StorySelectPage, null)
			},
			{
				"buy_cards",
				new SceneData(UIManager.ViewScene.MyPage, null)
			},
			{
				"sleeve",
				new SceneData(UIManager.ViewScene.CardSleevePurchasePage, "Mail_0034")
			},
			{
				"colosseum",
				new SceneData(UIManager.ViewScene.MyPage, null)
			},
			{
				"quest",
				new SceneData(UIManager.ViewScene.QuestSelectionPage, null)
			},
			{
				"crossover",
				new SceneData(UIManager.ViewScene.CrossoverPortal, null)
			},
			{
				"bingo",
				new SceneData(UIManager.ViewScene.Bingo, null)
			},
			{
				"card_vote",
				new SceneData(UIManager.ViewScene.NeutralPopularityVote, null)
			},
			{
				"leader_skin_vote",
				new SceneData(UIManager.ViewScene.LeaderPopularityVote, null)
			},
			{
				"mypage_bg",
				new SceneData(UIManager.ViewScene.MyPage, null)
			},
			{
				"free_match",
				new SceneData(UIManager.ViewScene.MyPage, null)
			}
		};

		public UIManager.ViewScene Scene { get; private set; }

		public UIManager.ChangeViewSceneParam Param { get; private set; }

		public int? Status { get; set; }

		public MyPageSub? Sub { get; private set; }

		public KeyValuePair<string, int>? PlayerPref { get; private set; }

		public int? UpdateFooter { get; private set; }

		public TransitionData(string s)
		{
			Scene = _transitionDictScene[s]._scene;
			Param = new UIManager.ChangeViewSceneParam();
			if (s == null)
			{
				return;
			}
			switch (s)
			{
			case "free_match":
				Param = CreateMyPageChangeParam(2, isCutCard: true);
				Param.OnFinishChangeView = delegate
				{
					MyPageMenu.Instance.GoToFreeMatch();
				};
				break;
			case "arena":
				Param = CreateMyPageChangeParam(3, isCutCard: true);
				break;
			case "2pick":
				Param = CreateMyPageChangeParam(3, isCutCard: true);
				Param.OnFinishChangeView = delegate
				{
					MyPageMenu.Instance.GoToChallengeMenu();
				};
				break;
			case "colosseum":
				Sub = MyPageSub.Colosseum;
				Param = CreateMyPageChangeParam(3, isCutCard: true);
				Param.OnFinishChangeView = delegate
				{
					MyPageMenu.Instance.GoToColosseum();
				};
				break;
			case "supplies":
				Sub = MyPageSub.Supply;
				Param = CreateMyPageChangeParam(5, isCutCard: true);
				Param.OnFinishChangeView = delegate
				{
					MyPageMenu.Instance.GoToShopSupply();
				};
				break;
			case "buy_cards":
				Sub = MyPageSub.Gacha;
				Param = CreateMyPageChangeParam(5, isCutCard: true);
				Param.OnFinishChangeView = delegate
				{
					MyPageMenu.Instance.GoToShopCard();
				};
				break;
			case "mypage_bg":
				Param = CreateMyPageChangeParam(0, isCutCard: true);
				Param.OnFinishChangeView = delegate
				{
					MyPageBGCustomDialog.Create(MyPageMenu.Instance.HomeMenu.OnDecideMyPageBG);
				};
				break;
			case "leader_skin":
				UpdateFooter = 5;
				PlayerPref = PlayerPrefsWrapper.SCENE_TRANSITION_VIEW_SKIN_SERIES_ID;
				break;
			case "deck_pack":
				UpdateFooter = 5;
				PlayerPref = PlayerPrefsWrapper.SCENE_TRANSITION_VIEW_DECK_SERIES_ID;
				break;
			case "card_pack":
				UpdateFooter = 5;
				break;
			case "sleeve":
				UpdateFooter = 5;
				PlayerPref = PlayerPrefsWrapper.SCENE_TRANSITION_VIEW_SLEEVE_SERIES_ID;
				break;
			case "limited_story":
				Data.SelectedStoryInfo = new SelectedStoryInfo(StoryEntranceType.LimitedStory);
				break;
			case "story_section_select":
				Data.SelectedStoryInfo = new SelectedStoryInfo(StoryEntranceType.AllStory);
				break;
			}
		}
	}

	public enum MyPageSub
	{
		Colosseum,
		Supply,
		Gacha
	}

	private static void GoToMission(int status)
	{
		MissionInfoTask missionInfoTask = null; // Pre-Phase-5b: headless has no MissionInfoTask
		missionInfoTask.SetParameter();
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(missionInfoTask, delegate
		{
			Mission.ChangeViewSceneTransitionMissionViewTab((Mission.MissionViewTab)status);
		}));
	}

	public static bool IsMaintenance(TransitionData data)
	{
		NetworkDefine.MAINTENANCE_TYPE? mAINTENANCE_TYPE = null;
		switch (data.Scene)
		{
		case UIManager.ViewScene.Gacha:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.SHOP_CARDPACK_MAINTENANCE;
			break;
		case UIManager.ViewScene.Profile:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.PROFILE_MAINTENANCE;
			break;
		case UIManager.ViewScene.CardAllList:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.CARD_MAINTENANCE;
			break;
		case UIManager.ViewScene.DeckList:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.DECK_MAINTENANCE;
			break;
		case UIManager.ViewScene.MyPage:
			if (!data.Sub.HasValue)
			{
				break;
			}
			switch (data.Sub.Value)
			{
			case MyPageSub.Supply:
				if (IsMaintenance(NetworkDefine.MAINTENANCE_TYPE.SHOP_SLEEVE_MAINTENANCE) && IsMaintenance(NetworkDefine.MAINTENANCE_TYPE.SHOP_SKIN_MAINTENANCE))
				{
					return IsMaintenance(NetworkDefine.MAINTENANCE_TYPE.SHOP_ITEM_MAINTENANCE);
				}
				return false;
			case MyPageSub.Colosseum:
				mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.COLOSSEUM;
				break;
			case MyPageSub.Gacha:
				mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.SHOP_CARDPACK_MAINTENANCE;
				break;
			}
			break;
		case UIManager.ViewScene.BuildDeckPurchasePage:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.SHOP_BUILDDECK_MAINTENANCE;
			break;
		case UIManager.ViewScene.Mission:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.MISSION_MAINTENANCE;
			break;
		case UIManager.ViewScene.StorySelectPage:
		case UIManager.ViewScene.StorySelectionWorld:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.STORY_MAINTENANCE;
			break;
		case UIManager.ViewScene.ClassSkinPurchasePage:
			mAINTENANCE_TYPE = NetworkDefine.MAINTENANCE_TYPE.SHOP_SKIN_MAINTENANCE;
			break;
		}
		if (mAINTENANCE_TYPE.HasValue)
		{
			return IsMaintenance(mAINTENANCE_TYPE.Value);
		}
		return false;
	}

	public static bool IsMaintenance(NetworkDefine.MAINTENANCE_TYPE type)
	{
		if (Data.MaintenanceCodeList == null)
		{
			return false;
		}
		return Data.MaintenanceCodeList.Contains(type);
	}

	public static void ChangeScene(TransitionData data, Action onChange)
	{
		object sceneParam = null;
		data.Param.OnChange = onChange;
		data.Param.IsUpdateFooterMenuTexture = true;
		if (data.Scene == UIManager.ViewScene.DeckList)
		{
			ChangeSceneToDeckList(data);
			return;
		}
		if (data.UpdateFooter.HasValue)
		{
			data.Param.OnFinishChangeView = delegate
			{
				UIManager.GetInstance()._Footer.UpdateCurrentIndex(data.UpdateFooter.Value);
			};
		}
		if (data.PlayerPref.HasValue && data.Status.HasValue)
		{
			PlayerPrefsWrapper.SetValue(data.PlayerPref.Value, data.Status.Value);
		}
		if (data.Scene == UIManager.ViewScene.Mission)
		{
			if (data.Status.HasValue)
			{
				GoToMission(data.Status.Value);
			}
			return;
		}
		if (data.Scene == UIManager.ViewScene.StorySelectPage)
		{
			// StorySelectPage removed (DEAD-COLD engine cleanup Task 12)
		}
		else if (data.Scene == UIManager.ViewScene.StorySelectionWorld && data.Status.HasValue)
		{
			StorySelectionWorldScene.RedirectSectionId = data.Status.Value;
		}
		if (data.Scene == UIManager.ViewScene.Gacha)
		{
			GachaUIParam gachaUIParam = new GachaUIParam();
			if (data.Status.HasValue)
			{
				gachaUIParam.SetDefaultPackId(data.Status.Value);
			}
			sceneParam = gachaUIParam;
		}
		if (data.Scene == UIManager.ViewScene.CrossoverPortal)
		{
			sceneParam = CrossoverPortalParam.CreateParam(data.Status);
		}
		if (data.Scene == UIManager.ViewScene.ClassSkinPurchasePage && data.Status.HasValue)
		{
			ClassSkinPurchasePage.SetFirstDisplaySeries(data.Status.Value);
		}
		if (UIManager.GetInstance().GetCurrentScene() == UIManager.ViewScene.Battle)
		{
			Action<UIManager.ChangeViewSceneParam> paramCustomize = delegate(UIManager.ChangeViewSceneParam param)
			{
				if (onChange != null)
				{
					param.OnChange = (Action)Delegate.Combine(param.OnChange, onChange);
				}
				param.IsUpdateFooterMenuTexture = true;
			};
			/* Pre-Phase-5b: BattleCtrl.BattleEnd dropped (BattleControl stubbed chunk 7) */
		}
		else
		{
			UIManager.GetInstance().ChangeViewScene(data.Scene, data.Param, sceneParam);
		}
	}

	private static void ChangeSceneToDeckList(TransitionData data)
	{
		DeckInfoTask task = new DeckInfoTask();
		task.SetParameter(Format.All);
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			Format format = Format.Rotation;
			if (!task.DeckGroupListData.IsExistDeckListByFormat(Format.Rotation) && task.DeckGroupListData.IsExistDeckListByFormat(Format.Unlimited))
			{
				format = Format.Unlimited;
			}
			DeckListUI.ChangeSceneToDeckList(format, data.Param);
		}));
	}

	private static UIManager.ChangeViewSceneParam CreateMyPageChangeParam(int index, bool isCutCard)
	{
		return new UIManager.ChangeViewSceneParam
		{
			MyPageMenuIndex = index,
			IsCutCardMotion = isCutCard,
			IsUpdateFooterMenuTexture = true
		};
	}
}
