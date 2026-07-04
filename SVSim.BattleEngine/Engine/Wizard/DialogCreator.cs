using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard.Lottery;
using Wizard.Scripts.Network.Data.TaskData.Arena;

namespace Wizard;

public static class DialogCreator
{
	public static DialogBase CreateRewardReceiveDialog(List<ReceivedReward> rewardList)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetLayer("Loading");
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetTitleLabel(Data.SystemText.Get("Mail_0021"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		RewardBase component = NGUITools.AddChild(dialogBase.gameObject, Resources.Load<GameObject>("UI/layoutParts/StoryRewardPanel")).GetComponent<RewardBase>();
		for (int i = 0; i < rewardList.Count; i++)
		{
			component.AddReward(rewardList[i]);
		}
		component.EndCreate();
		dialogBase.SetObj(component.gameObject);
		dialogBase.OnClose = delegate
		{
			List<string> cardListAssetPathList = Toolbox.ResourcesManager.CardListAssetPathList;
			if (cardListAssetPathList.Count > 0)
			{
				Toolbox.ResourcesManager.RemoveAssetGroup(cardListAssetPathList);
				cardListAssetPathList.Clear();
			}
		};
		return dialogBase;
	}

	public static CardDetailUI CreateCardDetailDialog(GameObject rootObject, string layerName)
	{
		CardDetailUI component = NGUITools.AddChild(rootObject, Resources.Load<GameObject>("UI/CardDetail")).GetComponent<CardDetailUI>();
		component.Initialize(LayerMask.NameToLayer(layerName), CardMaster.CardMasterId.Default);
		return component;
	}

	public static UICardList CreateDeckViewerDialog(GameObject rootObject)
	{
		CardDetailUI cardDetailUI = CreateCardDetailDialog(rootObject, "Detail");
		cardDetailUI.gameObject.SetActive(value: false);
		UICardList deckViewerDialog = NGUITools.AddChild(rootObject, Resources.Load<GameObject>("UI/UICardList")).GetComponent<UICardList>();
		deckViewerDialog.Init(rootObject, cardDetailUI, string.Empty, delegate
		{
			deckViewerDialog.SetActive(in_Active: false);
		}, "Detail", in_DetailCameraUse: true);
		deckViewerDialog.SetActive(in_Active: false);
		return deckViewerDialog;
	}

	public static DialogBase CreateStageSelectDialog()
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("Common_0021"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		dialogBase.SetObj(NGUITools.AddChild(dialogBase.gameObject, Resources.Load<GameObject>("UI/DeckList/StageSelect")));
		return dialogBase;
	}
}
