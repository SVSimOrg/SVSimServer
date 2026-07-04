using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class QuestCampaignDialog : MonoBehaviour
{
	[SerializeField]
	private UITexture _texture;

	private Action _onTweet;

	private List<string> _loadPathList = new List<string>();

	private string GetTexturePath(bool isFetch)
	{
		return Toolbox.ResourcesManager.GetAssetTypePath("quest_dialog_0013", ResourcesManager.AssetLoadPathType.UiOtherTexture, isFetch);
	}

	public static void Create(Action onFinish, Action onTweet = null)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("BossRush_0046"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_GrayBtn);
		dialogBase.SetButtonText(Data.SystemText.Get("Mission_0109"), Data.SystemText.Get("Common_0008"));
		dialogBase.SetSize(DialogBase.Size.M);
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/layoutParts/BossRush/QuestCampaignDialog")) as GameObject;
		dialogBase.SetObj(gameObject);
		QuestCampaignDialog component = gameObject.GetComponent<QuestCampaignDialog>();
		component.Initialize(dialogBase);
		component._onTweet = onTweet;
		dialogBase.OnClose = delegate
		{
			onFinish.Call();
		};
	}

	private void Initialize(DialogBase dialog)
	{
		dialog.onPushButton1 = (Action)Delegate.Combine(dialog.onPushButton1, (Action)delegate
		{
			OnClickTwitterShare();
		});
		Load(delegate
		{
			_texture.mainTexture = Toolbox.ResourcesManager.LoadObject(GetTexturePath(isFetch: true)) as Texture;
		});
	}

	private void Load(Action onFinish)
	{
		_loadPathList.Add(GetTexturePath(isFetch: false));
		StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(_loadPathList, delegate
		{
			onFinish.Call();
		}));
	}

	private void OnClickTwitterShare()
	{
		QuestTweetTask task = new QuestTweetTask();
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			_onTweet.Call();
		}));
	}
}
