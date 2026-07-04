using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Wizard;

namespace Cute;

public class AssetHandle
{
	public enum AssetType
	{
		Manifests,
		AssetBundle,
		Sound,
		TemporarySound,
		Movie,
		Font
	}

	private enum RequestType
	{
		None,
		Download,
		Load
	}

	private AssetRequestContext requestContext;

	private static SHA1CryptoServiceProvider sha1;

	private static UTF8Encoding utf8;

	private string _cryptFilename;

	private byte _HandleAttribute;

	private Action phase;

	private RequestType requestType;

	public string manifestDataHash { get; set; }

	public string SmallDataHash { get; private set; }

	public float manifestDataSize { get; protected set; }

	public float ManifestSmallDataSize { get; private set; }

	private string SelectedHash
	{
		get
		{
			if (!CustomPreference.IsNormalResource)
			{
				return SmallDataHash;
			}
			return manifestDataHash;
		}
	}

	public AssetType assetType { get; private set; }

	public string directory { get; private set; }

	public string filename { get; private set; }

	public string AssetName { get; private set; }

	public string dataHash => Toolbox.AssetManager.GetLocalDatahash(directory + filename);

	public int reference { get; set; }

	public bool isPreDownload { get; set; }

	public bool isTutorialDownload { get; set; }

	public bool unloadCommon { get; set; }

	public bool unloadTemporary { get; set; }

	public string cryptFilename
	{
		get
		{
			if (_cryptFilename == null)
			{
				string extension = Path.GetExtension(filename);
				string name = ((!(extension != "")) ? "" : filename.Replace(extension, ""));
				_cryptFilename = GenCryptoAssetFileName(name);
			}
			return _cryptFilename;
		}
	}

	public bool useStreamingAsset { get; set; }

	public bool isMultipleHandleIgnorAsset
	{
		get
		{
			if ((_HandleAttribute & 2) != 2)
			{
				return false;
			}
			return true;
		}
		set
		{
			if (value)
			{
				_HandleAttribute |= 2;
			}
			else
			{
				_HandleAttribute = (byte)(_HandleAttribute & -3);
			}
		}
	}

	public bool isSubManifest
	{
		get
		{
			return (_HandleAttribute & 4) == 4;
		}
		set
		{
			if (value)
			{
				_HandleAttribute |= 4;
			}
			else
			{
				_HandleAttribute = (byte)(_HandleAttribute & -5);
			}
		}
	}

	public string GetHash(bool isNormalSizeResource)
	{
		if (!isNormalSizeResource)
		{
			return SmallDataHash;
		}
		return manifestDataHash;
	}

	public float GetSize(bool isNormalSizeResource)
	{
		if (!isNormalSizeResource)
		{
			return ManifestSmallDataSize;
		}
		return manifestDataSize;
	}

	public bool isReDownloadAsset(bool isNormalSizeResource)
	{
		bool flag = false;
		if (!flag && !dataHash.Equals(GetHash(isNormalSizeResource)))
		{
			flag = true;
		}
		return flag;
	}

	public string BuildURL()
	{
		if (assetType == AssetType.Manifests)
		{
			if (!filename.Contains("manifest"))
			{
				return CustomPreference.GetAssetBundleURL() + filename;
			}
			if (filename.Equals("soundmanifest"))
			{
				return CustomPreference.GetSubManifestURL() + filename;
			}
			if (filename.StartsWith("moviemanifest"))
			{
				return CustomPreference.GetManifestURL() + filename;
			}
			if (filename.StartsWith("fontmanifest"))
			{
				return CustomPreference.GetManifestURL() + filename;
			}
			if (!isSubManifest)
			{
				return CustomPreference.GetManifestURL() + filename;
			}
			return CustomPreference.GetSubManifestURL() + filename;
		}
		if (IsSound())
		{
			return CustomPreference.GetSoundResourceURL() + manifestDataHash;
		}
		if (assetType == AssetType.Movie)
		{
			return CustomPreference.GetMoiveResourceURL() + manifestDataHash;
		}
		return CustomPreference.GetAssetBundleURL() + SelectedHash;
	}

	public string BuildLocalCachePath()
	{
		AssetManager assetManager = Toolbox.AssetManager;
		bool isCryptAssetFileName = AssetManager.isCryptAssetFileName;
		return assetType switch
		{
			AssetType.Manifests => assetManager.getAssetSavePath(assetType) + filename, 
			AssetType.Sound => assetManager.getAssetSavePath(assetType) + directory + filename, 
			AssetType.TemporarySound => assetManager.getAssetSavePath(assetType) + directory + filename, 
			AssetType.Movie => assetManager.getAssetSavePath(assetType) + directory + filename, 
			AssetType.Font => assetManager.getAssetSavePath(assetType) + directory + filename, 
			_ => assetManager.getAssetSavePath(assetType) + (isCryptAssetFileName ? cryptFilename : filename), 
		};
	}

	public AssetHandle(string _name, string expectedDataHash, string category, string size, string smallHash, string smallSize, bool isManifest = false, bool isUseStreaming = false)
	{
		directory = "";
		string[] array = _name.Split('/');
		for (int i = 0; i < array.Length - 1; i++)
		{
			directory = directory + array[i] + "/";
		}
		filename = Path.GetFileName(_name);
		AssetName = _name;
		useStreamingAsset = isUseStreaming;
		string extension = Path.GetExtension(filename);
		if (extension.Equals(".mp4") || extension.Equals(".ogg") || extension.Equals(".usm"))
		{
			assetType = AssetType.Movie;
		}
		else if (extension.Equals(".unity3d") || extension.Equals(".lz4"))
		{
			assetType = AssetType.AssetBundle;
		}
		else if (extension.Equals(".acb") || extension.Equals(".awb"))
		{
			assetType = (_name.Contains("/t/") ? AssetType.TemporarySound : AssetType.Sound);
		}
		else if (extension.Equals(".otf") || extension.Equals(".ttf") || extension.Equals(".TTF"))
		{
			assetType = AssetType.Font;
		}
		else
		{
			if (!isManifest)
			{
				Debug.LogError("error invalid asset name : " + filename);
				return;
			}
			assetType = AssetType.Manifests;
		}
		string.IsNullOrEmpty(expectedDataHash);
		isPreDownload = category != null && Toolbox.AssetManager.predownloadCategories.Contains(category);
		isTutorialDownload = category != null && Toolbox.AssetManager.tutorialdownloadCategories.Contains(category);
		phase = PhaseIdle;
		requestType = RequestType.None;
		unloadCommon = false;
		unloadTemporary = false;
		manifestDataHash = expectedDataHash;
		SmallDataHash = smallHash;
		if (string.IsNullOrEmpty(size))
		{
			manifestDataSize = 0f;
		}
		else
		{
			float result;
			bool flag = float.TryParse(size, out result);
			manifestDataSize = ((!flag) ? 0.1f : result);
		}
		if (string.IsNullOrEmpty(smallSize))
		{
			ManifestSmallDataSize = 0f;
			return;
		}
		float result2;
		bool flag2 = float.TryParse(smallSize, out result2);
		ManifestSmallDataSize = ((!flag2) ? 0.1f : result2);
	}

	private void PhaseIdle()
	{
		if (requestType == RequestType.Download)
		{
			phase = PhaseDownloading;
			Toolbox.AssetManager.AddDownloadJob(_Download(), _DownloadCancel);
		}
		if (requestType == RequestType.Load)
		{
			phase = PhaseLoading;
			Toolbox.AssetManager.AddLoadJob(_PlatformDependentLoad(), _LoadCancel);
		}
	}

	private IEnumerator _PlatformDependentLoad()
	{
		return _Load();
	}

	private void PhaseDownloading()
	{
	}

	private void PhaseLoading()
	{
	}

	private void LoadManifestOfManifest(string text)
	{
		ArrayList arrayList = null;
		arrayList = Utility.ConvertCSV(text, removeTitle: false);
		for (int i = 0; i < arrayList.Count; i++)
		{
			string[] array = (string[])((ArrayList)arrayList[i]).ToArray(typeof(string));
			string text2 = array[0];
			string text3 = array[1];
			AssetHandle handle = new AssetHandle(text2, text3, array[2], null, text3, null, isManifest: true);
			Toolbox.AssetManager.RegistHandle(text2, handle);
		}
	}

	public void LoadMergeManifestOfManifest(string main_manifest, string overwrite_manifest)
	{
		if (string.IsNullOrEmpty(main_manifest) || string.IsNullOrEmpty(overwrite_manifest))
		{
			return;
		}
		ArrayList arrayList = null;
		ArrayList arrayList2 = null;
		arrayList = Utility.ConvertCSV(main_manifest, removeTitle: false);
		arrayList2 = Utility.ConvertCSV(overwrite_manifest, removeTitle: false);
		string text = Toolbox.AssetManager.MovieManifesHeadtName + "_" + CustomPreference.GetSoundMovieLanguage().ToLower();
		for (int i = 0; i < arrayList.Count; i++)
		{
			string[] array = (string[])((ArrayList)arrayList[i]).ToArray(typeof(string));
			ArrayList arrayList3 = null;
			if (Toolbox.AssetManager.SoundManifesHeadtName.Contains(array[0]))
			{
				int num = 0;
				for (num = 0; num < arrayList2.Count; num++)
				{
					arrayList3 = (ArrayList)arrayList2[num];
					if (arrayList3[0].ToString() == array[0].ToString())
					{
						array = (string[])arrayList3.ToArray(typeof(string));
						break;
					}
				}
			}
			if (!array[0].Contains(Toolbox.AssetManager.MovieManifesHeadtName) || !(text != array[0]))
			{
				string text2 = array[0];
				AssetHandle handle = new AssetHandle(text2, array[1], array[2], array[3], array[1], array[3], isManifest: true);
				Toolbox.AssetManager.RegistHandle(text2, handle);
			}
		}
	}

	private void LoadManifest(string text)
	{
		List<string[]> list = Utility.ConvertCSV_Array(text, removeTitle: false);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			string[] array = list[i];
			string text2 = array[0];
			if (!(text2 == "master_card_master.unity3d"))
			{
				bool isUseStreaming = false;
				AssetHandle handle = new AssetHandle(text2, array[1], array[2], array[3], array[4], array[5], isManifest: false, isUseStreaming);
				if (!Toolbox.AssetManager.RegistHandle(text2, handle))
				{
					handle = null;
				}
			}
		}
	}

	private IEnumerator _Download()
	{
		if (requestContext != null)
		{
			Utility.LeanSemaphore semaphore = requestContext.semaphore;
			if (semaphore != null)
			{
				AssetErrorState errorState = requestContext.errorState;
				while (!semaphore.TryWait())
				{
					yield return 0;
				}
				if (errorState != null && errorState.canceled)
				{
					errorState.Report(filename, AssetErrorState.Code.CANCELED);
					Fin();
					yield break;
				}
			}
		}
		string url = BuildURL();
		int QuickRetryCount_Cache = 0;
		int QuickRetryCount_Hash = 0;
		while (true)
		{
			string errorMessage = "";
			string errorCode = "";
			if (!isReDownloadAsset(CustomPreference.IsNormalResource))
			{
				break;
			}
			using (UnityWebRequest www = UnityWebRequest.Get(url))
			{
				if (www == null)
				{
					break;
				}
				yield return www.SendWebRequest();
				bool isTimeOut = false;
				float noProgressTime = Time.realtimeSinceStartup;
				float oldProgress = 0f;
				float timeOut = 30f;
				while (!www.isDone)
				{
					float downloadProgress = www.downloadProgress;
					if (downloadProgress <= oldProgress)
					{
						oldProgress = downloadProgress;
						if (Time.realtimeSinceStartup - noProgressTime > timeOut)
						{
							isTimeOut = true;
							break;
						}
					}
					else
					{
						oldProgress = downloadProgress;
						noProgressTime = Time.realtimeSinceStartup;
					}
					yield return null;
				}
				if (!string.IsNullOrEmpty(www.error) || isTimeOut)
				{
					if (isTimeOut)
					{
						Debug.LogError("download timeout error:" + url);
						errorMessage = Data.SystemText.Get("System_0004");
					}
					else
					{
						if (www.GetResponseHeaders() == null || !www.GetResponseHeaders().TryGetValue("STATUS", out var _))
						{
						}
						Debug.LogError($"download error: {www.error} ({url}) ({filename})");
						if (www.error.StartsWith("Failed to initialize cache for the AssetBundle") && QuickRetryCount_Cache < 5)
						{
							QuickRetryCount_Cache++;
							continue;
						}
						if (www.error.StartsWith("Failed to decompress data for the AssetBundle") && QuickRetryCount_Cache < 5)
						{
							QuickRetryCount_Cache++;
							continue;
						}
						yield return new WaitForSeconds(0.1f);
						errorMessage = Data.SystemText.Get("System_0005");
					}
					UIManager.GetInstance().isErrorProc = false;
					if (!Toolbox.AssetManager.IsBackgroundDownload)
					{
						string titleLabel = Data.SystemText.Get("System_0003");
						string text = errorMessage;
						QuickRetryCount_Cache = 0;
						DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose(isSystem: true);
						if (dialogBase != null)
						{
							dialogBase.SetFadeButtonEnabled(flag: false);
							dialogBase.SetSize(DialogBase.Size.M);
							dialogBase.SetTitleLabel(titleLabel);
							dialogBase.SetText((text == "") ? Data.SystemText.Get("Battle_0300") : (text + Environment.NewLine + Data.SystemText.Get("Battle_0300")));
							dialogBase.SetReturnMsg(UIManager.GetInstance().gameObject, "CommonRetry", "CommonResetGame");
							dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_GrayBtn);
							dialogBase.SetButtonText(Data.SystemText.Get("Battle_0301"), Data.SystemText.Get("System_0006"));
							dialogBase.ClickSe_Btn2 = 0;
							dialogBase.SetPanelDepth(6000);
						}
						UIManager.GetInstance().isNoAvailMemory = true;
					}
					UIManager.GetInstance().isRetryProc = false;
					if (Toolbox.AssetManager.IsBackgroundDownload)
					{
						yield return new WaitForSeconds(5f);
						UIManager.GetInstance().CommonRetry();
					}
					while (!UIManager.GetInstance().isErrorProc)
					{
						yield return 0;
					}
					UIManager.GetInstance().isNoAvailMemory = false;
					if (UIManager.GetInstance().isRetryProc)
					{
						disposeWebRequest(www);
						yield return 0;
						continue;
					}
					disposeWebRequest(www);
					yield break;
				}
				Toolbox.AssetManager.AddDownloadCompletedSize(GetSize(CustomPreference.IsNormalResource));
				string localCachePath = BuildLocalCachePath();
				if (assetType == AssetType.Manifests)
				{
					try
					{
						string text2 = www.downloadHandler.text;
						if (filename == "manifest_assetmanifest")
						{
							if (!isSubManifest)
							{
								Toolbox.AssetManager.manifestOfManifests = text2;
							}
							else
							{
								Toolbox.AssetManager.manifestOfManifests_sub = text2;
							}
							goto IL_0684;
						}
						string text3 = Utility.CreateHash(text2).ToString();
						if (!string.IsNullOrEmpty(manifestDataHash) && !text3.Equals(manifestDataHash) && QuickRetryCount_Hash < 5)
						{
							QuickRetryCount_Hash++;
							disposeWebRequest(www);
							continue;
						}
						manifestDataHash = text3;
						File.WriteAllText(localCachePath, text2, Encoding.UTF8);
						Toolbox.AssetManager.SaveLocalDatahash(filename, text3);
						goto IL_0684;
						IL_0684:
						Toolbox.AssetManager.AddManifestCount();
						goto IL_08bd;
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
						Debug.LogError(errorMessage);
						errorCode = "ERROR_CAPACITY_OVER";
						goto IL_08bd;
					}
				}
				Exception e = null;
				byte[] bytes = www.downloadHandler.data;
				string dataHash = null;
				ParallelJob threadJob = ParallelJob.Dispatch(delegate
				{
					dataHash = Utility.CreateHash(bytes).ToString();
				});
				while (!threadJob.isDone)
				{
					yield return null;
				}
				if (!string.IsNullOrEmpty(SelectedHash) && !dataHash.Equals(SelectedHash) && QuickRetryCount_Hash < 5)
				{
					QuickRetryCount_Hash++;
					disposeWebRequest(www);
					continue;
				}
				if (CustomPreference.IsNormalResource)
				{
					manifestDataHash = dataHash;
				}
				else
				{
					SmallDataHash = dataHash;
				}
				int num = 0;
				while (num < 5)
				{
					e = TryWriteAllBytes(localCachePath, bytes);
					if (e == null)
					{
						break;
					}
					num++;
					Debug.LogError("error System.IO.File.WriteAllBytes ct " + num);
				}
				while (!threadJob.isDone)
				{
					yield return null;
				}
				if (e == null)
				{
					Toolbox.AssetManager.SaveLocalDatahash(directory + filename, dataHash.ToString());
				}
				else
				{
					manifestDataHash = (dataHash = null);
					errorMessage = e.Message;
					Debug.LogError(errorMessage);
					errorCode = "ERROR_CAPACITY_OVER";
				}
				goto IL_08bd;
				IL_08bd:
				QuickRetryCount_Hash = 0;
				if (errorCode.Contains("ERROR"))
				{
					Debug.LogError("download error:" + errorMessage + " : " + filename);
					if (requestContext != null && requestContext.errorState != null)
					{
						AssetErrorState errorState2 = requestContext.errorState;
						errorState2.Report(filename, AssetErrorState.Code.LOCAL_CAPACITY_OVER);
						errorState2.lastDialogDecision = AssetErrorState.DialogDecision.TERMINATE;
					}
					LocalLog.AccumulateTraceLog("Not enough available storage.");
					UIManager.GetInstance().isErrorProc = false;
					string titleLabel2 = Data.SystemText.Get("System_0020");
					DialogBase dialogBase2 = UIManager.GetInstance().CreateDialogClose(isSystem: true);
					if (dialogBase2 != null)
					{
						dialogBase2.SetFadeButtonEnabled(flag: false);
						dialogBase2.SetTitleLabel(titleLabel2);
						dialogBase2.SetText(Data.SystemText.Get("System_0021"));
						dialogBase2.SetReturnMsg(UIManager.GetInstance().gameObject, "CommonResetGame", "CommonResetGame", "CommonResetGame", "CommonResetGame");
						dialogBase2.SetButtonLayout(DialogBase.ButtonLayout.GrayBtn);
						dialogBase2.SetButtonText(Data.SystemText.Get("System_0006"));
						dialogBase2.ClickSe_Btn1 = 0;
						dialogBase2.SetPanelDepth(6000);
					}
					UIManager.GetInstance().isNoAvailMemory = true;
					UIManager.GetInstance().isRetryProc = false;
					while (!UIManager.GetInstance().isErrorProc)
					{
						yield return 0;
					}
					UIManager.GetInstance().isNoAvailMemory = false;
					UIManager.GetInstance().isNoAvailMemory = false;
					if (!UIManager.GetInstance().isRetryProc)
					{
						SoftwareResetBase.SoftwareReset(null, null);
					}
				}
				disposeWebRequest(www);
			}
			break;
		}
		Fin();
	}

	private Exception TryWriteAllBytes(string localCachePath, byte[] bytes)
	{
		try
		{
			File.WriteAllBytes(localCachePath, bytes);
			return null;
		}
		catch (Exception ex)
		{
			Debug.LogError("error System.IO.File.WriteAllBytes " + localCachePath + "," + ex.GetType().FullName + "," + ex.Message);
			return ex;
		}
	}

	private void _DownloadCancel()
	{
		phase = PhaseIdle;
		requestType = RequestType.None;
		Fin();
	}

	private void _LoadPostProcess()
	{
		switch (assetType)
		{
		case AssetType.AssetBundle:
			Toolbox.AssetManager.AddLoadingCurrentCount(filename);
			break;
		case AssetType.Sound:
		case AssetType.TemporarySound:
			if (filename.Substring(filename.Length - 4).Equals(".awb"))
			{
			}
			else
			{
			}
			Toolbox.AssetManager.AddLoadingCurrentCount(filename);
			break;
		case AssetType.Movie:
			Toolbox.AssetManager.AddLoadingCurrentCount(filename);
			break;
		case AssetType.Font:
			Toolbox.AssetManager.AddLoadingCurrentCount(filename);
			break;
		case AssetType.Manifests:
			if (filename == "manifest_assetmanifest")
			{
				string manifestOfManifests = Toolbox.AssetManager.manifestOfManifests;
				if (manifestOfManifests == null)
				{
					Debug.LogError("Failed to load manifest of manifests");
				}
				else if (Toolbox.AssetManager.manifestOfManifests_sub != null)
				{
					LoadMergeManifestOfManifest(manifestOfManifests, Toolbox.AssetManager.manifestOfManifests_sub);
				}
				else
				{
					LoadManifestOfManifest(manifestOfManifests);
				}
			}
			else
			{
				string text = File.ReadAllText(BuildLocalCachePath(), Encoding.UTF8);
				LoadManifest(text);
			}
			break;
		}
	}

	private IEnumerator _Load()
	{
		if (requestContext != null)
		{
			Utility.LeanSemaphore semaphore = requestContext.semaphore;
			if (semaphore != null)
			{
				AssetErrorState errorState = requestContext.errorState;
				while (!semaphore.TryWait())
				{
					yield return 0;
				}
				if (errorState != null && errorState.canceled)
				{
					errorState.Report(filename, AssetErrorState.Code.CANCELED);
					Fin();
					yield break;
				}
			}
		}
		if (assetType == AssetType.AssetBundle && ++reference == 1)
		{
			string errorMessage = "";
			string localCachePath = BuildLocalCachePath();
			AssetBundle unityAssetBundle;
			if (requestContext != null && requestContext.preferSynchronousLoad)
			{
				unityAssetBundle = AssetBundle.LoadFromFile(localCachePath);
			}
			else
			{
				AssetBundleCreateRequest acr = AssetBundle.LoadFromFileAsync(localCachePath);
				yield return acr;
				unityAssetBundle = acr.assetBundle;
			}
			if (unityAssetBundle == null)
			{
				string text = errorMessage + " : " + localCachePath;
				Debug.LogError("_DownloadCancel load error:" + text);
				if (requestContext != null && requestContext.errorState != null)
				{
					requestContext.errorState.Report(filename, AssetErrorState.Code.FILE_READ_ERROR);
				}
				Toolbox.AssetManager.AddLoadingCurrentCount(filename);
				Fin();
				if (UIManager.GetInstance() != null)
				{
					UIManager.GetInstance().CreateAssetFileErrorDialog();
				}
				yield break;
			}
			Toolbox.AssetManager.SetAssetBundle(filename, unityAssetBundle, isMultipleHandleIgnorAsset);
			string[] pathlist = unityAssetBundle.GetAllAssetNames();
			AssetBundleRequest request = unityAssetBundle.LoadAllAssetsAsync();
			yield return request;
			UnityEngine.Object[] allAssets = request.allAssets;
			bool flag = false;
			for (int i = 0; i < Toolbox.AssetManager.NoUnloadAssetName.Count; i++)
			{
				if (filename.StartsWith(Toolbox.AssetManager.NoUnloadAssetName[i]))
				{
					flag = true;
				}
			}
			if (!unloadCommon && !flag && unityAssetBundle != null)
			{
				unityAssetBundle.Unload(unloadAllLoadedObjects: false);
			}
			int num = pathlist.Length;
			int num2 = allAssets.Length;
			List<AssetObject> list = new List<AssetObject>();
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < num; k++)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathlist[k]);
					if (allAssets[j].name.ToLower().Equals(fileNameWithoutExtension.ToLower()))
					{
						string path = Path.ChangeExtension(pathlist[k], ".any");
						list.Add(new AssetObject(path, allAssets[j]));
						break;
					}
				}
			}
			Toolbox.AssetManager.SetObjectList(filename, list);
		}
		_LoadPostProcess();
		Fin();
	}

	private void _LoadCancel()
	{
	}

	private bool _Unload()
	{
		if (--reference == 0)
		{
			if (assetType == AssetType.AssetBundle)
			{
				if (!unloadCommon && !unloadTemporary)
				{
					Toolbox.AssetManager.UnloadAssetBundle(filename);
				}
				else
				{
					int num = reference + 1;
					reference = num;
				}
			}
		}
		else if (reference < 0)
		{
			reference = 0;
		}
		if (IsSound())
		{
		}
		return true;
	}

	private void Fin()
	{
		Action<AssetHandle> action = null;
		if (requestContext != null)
		{
			requestContext.semaphore?.Post();
			action = requestContext.callback;
		}
		phase = PhaseIdle;
		requestContext = null;
		action?.Invoke(this);
	}

	public void Unload()
	{
		if (_Unload())
		{
			phase = PhaseIdle;
		}
		requestType = RequestType.None;
	}

	public bool Load(AssetRequestContext requestContext)
	{
		requestType = RequestType.Load;
		this.requestContext = requestContext;
		phase();
		return true;
	}

	public bool QuickLoadIfPossible()
	{
		if (phase != new Action(PhaseIdle))
		{
			return false;
		}
		bool flag = false;
		switch (assetType)
		{
		case AssetType.Manifests:
			flag = false;
			break;
		case AssetType.AssetBundle:
			flag = reference > 0 && Toolbox.AssetManager.HasObjectList(filename);
			if (flag)
			{
				int num = reference + 1;
				reference = num;
			}
			break;
		case AssetType.Movie:
			flag = !isReDownloadAsset(CustomPreference.IsNormalResource);
			break;
		case AssetType.Font:
			flag = !isReDownloadAsset(CustomPreference.IsNormalResource);
			break;
		case AssetType.Sound:
		case AssetType.TemporarySound:
			flag = !isReDownloadAsset(CustomPreference.IsNormalResource);
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			_LoadPostProcess();
		}
		return flag;
	}

	public bool IsSound()
	{
		if (assetType != AssetType.Sound && assetType != AssetType.TemporarySound)
		{
			return false;
		}
		return true;
	}

	public static string GenCryptoAssetFileName(string name)
	{
		if (sha1 == null)
		{
			sha1 = new SHA1CryptoServiceProvider();
		}
		if (utf8 == null)
		{
			utf8 = new UTF8Encoding();
		}
		byte[] bytes = utf8.GetBytes(name);
		byte[] array = sha1.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(Convert.ToString(array[i], 16).PadLeft(2, '0'));
		}
		return stringBuilder.ToString();
	}

	private void disposeWebRequest(UnityWebRequest www)
	{
		www.Dispose();
	}

	public void CopyWithCatchException(AssetHandle src)
	{
		manifestDataHash = src.manifestDataHash;
		SmallDataHash = src.SmallDataHash;
		isPreDownload = src.isPreDownload;
	}
}
