using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using MessagePack;
using UnityEngine;
using UnityEngine.Networking;
using Wizard;
using Wizard.Battle.Phase;
using Wizard.Bingo;
using Wizard.Scripts.Network.Data.TaskData.BuildDeckPurchase;
using Wizard.Scripts.Network.Data.TaskData.ItemPurchase;
using Wizard.Scripts.Network.Data.TaskData.SkinPurchase;
using Wizard.Scripts.Network.Data.TaskData.SpotCardExchange;

namespace Cute;

public class NetworkManager : MonoBehaviour, IManager
{
	public const float TimeOut = 30f;

	protected NetworkTask lastRequestTask;

	public bool isConnect;

	public bool isTimeOut;

	public bool isError;

	private bool isEncrypt = true;

	private bool isUseJson;

	private bool _showLoadingIcon = true;

	private IEnumerator connectCoroutine;


	public IEnumerator Connect(NetworkTask task, Action<NetworkTask.ResultCode> callbackOnSuccess = null, Action<NetworkTask.ResultCode> callbackOnFailure = null, Action<int> callbackOnResultCodeError = null, bool encrypt = true, bool useJson = false, bool showLoadingIcon = true, bool showErrorDialog = true)
	{
		while (isConnect)
		{
			yield return 0;
		}
		isEncrypt = encrypt;
		isUseJson = useJson;
		_showLoadingIcon = showLoadingIcon;
		if (true)
		{
			if (IsBattle())
			{
				LocalLog.AccumulateLastTraceLog("taskStart " + task);
			}
			lastRequestTask = task;
			lastRequestTask.Initialize();
			lastRequestTask.CallbackOnSuccess = callbackOnSuccess;
			lastRequestTask.CallbackOnFailure = callbackOnFailure;
			lastRequestTask.CallbackOnResultCodeError = callbackOnResultCodeError;
			lastRequestTask.PrepareHeaders();
			lastRequestTask.PreparePostData(isEncrypt, isUseJson);
			connectCoroutine = Connect(showErrorDialog);
			yield return StartCoroutine(connectCoroutine);
		}
	}

	private IEnumerator Connect(bool showErrorDialog)
	{
		while (isConnect)
		{
			yield return 0;
		}
		isConnect = true;
		isTimeOut = false;
		isError = false;
		bool isLogTraceCheckUri = false;
		if (lastRequestTask is DoMatchingBase || lastRequestTask is FinishTaskBase)
		{
			isLogTraceCheckUri = true;
		}
		string url = lastRequestTask.Url;
		_ = lastRequestTask;
		if (isLogTraceCheckUri)
		{
			LogTraceCheck("1");
		}
		using UnityWebRequest unityWebRequest = GetUnityWebRequestInstance(url);
		yield return unityWebRequest.SendWebRequest();
		if (isLogTraceCheckUri)
		{
			LogTraceCheck("2");
		}
		float endTime = Time.realtimeSinceStartup + 30f;
		if (lastRequestTask.GetType().Equals(typeof(CheckSpecialTitleTask)))
		{
			endTime = Time.realtimeSinceStartup + 2f;
		}
		while (!unityWebRequest.isDone && Time.realtimeSinceStartup < endTime)
		{
			yield return 0;
		}
		if (isLogTraceCheckUri)
		{
			LogTraceCheck("3");
		}
		if (!unityWebRequest.isDone)
		{
			isTimeOut = true;
			LocalLog.AccumulateTraceLog("Connect is TimeOut");
			disposeUnityWebRequest(unityWebRequest);
			if (!lastRequestTask.isSkipCommonTimeOutPopUp())
			{
				if (lastRequestTask.GetType().Equals(typeof(PackOpenTask)) || lastRequestTask.GetType().Equals(typeof(BuildDeckBuyTask)) || lastRequestTask.GetType().Equals(typeof(SleeveBuyTask)) || lastRequestTask.GetType().Equals(typeof(SkinBuyMultiRewardTask)) || lastRequestTask.GetType().Equals(typeof(SkinBuyMultiTask)) || lastRequestTask.GetType().Equals(typeof(SkinBuySingleTask)) || lastRequestTask.GetType().Equals(typeof(ItemPurchaseBuyTask)) || lastRequestTask.GetType().Equals(typeof(SpotCardExchangeTask)) || lastRequestTask.GetType().Equals(typeof(CardCreateTask)) || lastRequestTask.GetType().Equals(typeof(CardDestructTask)) || lastRequestTask.GetType().Equals(typeof(StoryFinishTask)) || lastRequestTask.GetType().Equals(typeof(PracticeFinishTask)) || lastRequestTask.GetType().Equals(typeof(BingoDrawTask)) || lastRequestTask.GetType().Equals(typeof(MypageTreasureBoxCpOpenTask)) || lastRequestTask.GetType().Equals(typeof(MypageReceiveSpecialTreasureTask)) || lastRequestTask.GetType().Equals(typeof(FreeCardPackCampaignFinishTask)))
				{

				}
				else
				{

				}
			}
			if (lastRequestTask.CallbackOnFailure != null)
			{
				lastRequestTask.CallbackOnFailure(NetworkTask.ResultCode.TimeOut);
			}
			Toolbox.DeviceManager.ClearIpAddress();
		}
		else if (!string.IsNullOrEmpty(unityWebRequest.error))
		{
			LocalLog.AccumulateTraceLog("Connect is Error!" + unityWebRequest.error + " responseCode:" + unityWebRequest.responseCode);
			isError = true;
			if (showErrorDialog && !lastRequestTask.isSkipCommonHttpStatusErrorPopUp())
			{
				if (lastRequestTask.GetType().Equals(typeof(PackOpenTask)))
				{

				}
				else
				{

				}
			}
			disposeUnityWebRequest(unityWebRequest);
			if (lastRequestTask.CallbackOnFailure != null)
			{
				lastRequestTask.CallbackOnFailure(NetworkTask.ResultCode.Error);
			}
			Toolbox.DeviceManager.ClearIpAddress();
		}
		else if (unityWebRequest.isDone)
		{
			if (lastRequestTask.CallbackOnUnityWebRequestDone != null)
			{
				lastRequestTask.CallbackOnUnityWebRequestDone(unityWebRequest);
			}
			else if (unityWebRequest.downloadHandler.text != null && unityWebRequest.downloadHandler.text != "")
			{
				try
				{
					byte[] bytes = ((!isEncrypt) ? Convert.FromBase64String(unityWebRequest.downloadHandler.text) : CryptAES.decrypt(unityWebRequest.downloadHandler.text));
					string json = (isUseJson ? MessagePackSerializer.ToJson(bytes) : MessagePackSerializer.ToJson(bytes));
					lastRequestTask.SetResponseData(JsonMapper.ToObject(json));
				}
				catch (Exception ex)
				{
					string text = unityWebRequest.downloadHandler.text;
					disposeUnityWebRequest(unityWebRequest);
					if (!lastRequestTask.GetType().Equals(typeof(CheckSpecialTitleTask)))
					{
						if (!isEncrypt)
						{
							LocalLog.AccumulateTraceLog(ex.ToString());
							throw ex;
						}
						Debug.LogError(text);
						Debug.LogError(ex.Message);
						Debug.LogError(ex.StackTrace);
						if (text.Contains("php"))
						{
							if (text.Length > 1800)
							{
								throw new Exception(text.Substring(1, 1800));
							}
							throw new Exception(text);
						}
						HandleDeserializeException(ex);
					}
				}
				try
				{
					if (lastRequestTask != null)
					{
						if (lastRequestTask.GetType().Equals(typeof(CheckSpecialTitleTask)))
						{
							((CheckSpecialTitleTask)lastRequestTask).ParseTitleCheckData();
						}
						else
						{
							NetworkTask.ERROR_CODE_STATUS num = lastRequestTask.CheckResultCodeToPopupCreate_ReturnStatus();
							if (num == NetworkTask.ERROR_CODE_STATUS.ERROR)
							{
								isError = true;
							}
							if (num == NetworkTask.ERROR_CODE_STATUS.ERROR_TO_MAINTENANCE_POPUP && lastRequestTask.CallbackOnFailure != null)
							{
								lastRequestTask.CallbackOnFailure(NetworkTask.ResultCode.Maintenance);
							}
							if (num == NetworkTask.ERROR_CODE_STATUS.ERROR && lastRequestTask.CallbackOnFailure != null)
							{
								lastRequestTask.CallbackOnFailure(NetworkTask.ResultCode.Title);
							}
						}
					}
				}
				catch (Exception ex2)
				{
					disposeUnityWebRequest(unityWebRequest);
					if (!lastRequestTask.GetType().Equals(typeof(CheckSpecialTitleTask)))
					{
						LocalLog.AccumulateTraceLog("NetworkManager Connect Error 2：" + ex2);
						throw ex2;
					}
				}
			}
			else
			{
				LocalLog.AccumulateTraceLog("NetworkManager Connect Error 3");
			}
		}
		ClearLastRequestTask();
		disposeUnityWebRequest(unityWebRequest);
		isConnect = false;
	}

	private void LogTraceCheck(string logMsg)
	{
		LocalLog.AccumulateLastTraceLog("NetworkTrace msg " + logMsg);
		LocalLog.SubmitAccumulateLastTraceLog();
	}

	private bool IsBattle()
	{
		if (false /* Pre-Phase-5b: RTA + MainPhase gate headless-false */)
		{
			return true;
		}
		return false;
	}

	private UnityWebRequest GetUnityWebRequestInstance(string serverUrl)
	{
		try
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(serverUrl, "POST");
			unityWebRequest.uploadHandler = new UploadHandlerRaw(lastRequestTask.Body);
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			foreach (KeyValuePair<string, string> item in lastRequestTask.Header)
			{
				unityWebRequest.SetRequestHeader(item.Key, item.Value);
			}
			return unityWebRequest;
		}
		catch (Exception ex)
		{
			string text = "";
			foreach (KeyValuePair<string, string> item2 in lastRequestTask.Header)
			{
				text += string.Format("header==={0} : {1}" + Environment.NewLine, item2.Key, item2.Value);
			}
			Debug.LogError(ex?.ToString() + "：" + text);
			return null;
		}
	}

	private void HandleDeserializeException(Exception e)
	{
		SoftwareReset.exec();
		throw new Exception("復号化に失敗しました。" + e);
	}

	public void ClearLastRequestTask()
	{
		if (lastRequestTask.isServerResultCodeOK())
		{
			if (IsBattle())
			{
				LocalLog.AccumulateLastTraceLog("ClearLastRequestTask " + lastRequestTask);
			}
			lastRequestTask = null;
		}
	}

	private void disposeUnityWebRequest(UnityWebRequest unityWebRequest)
	{
		unityWebRequest.Dispose();
	}
}
