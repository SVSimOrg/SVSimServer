using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Sqlite3Plugin;
using UnityEngine;
using Wizard;

namespace Cute;

public class AssetManager : MonoBehaviour, IManager
{
	public enum _tag_datamode
	{
	}

	private Dictionary<string, AssetHandle> handleDictionary = new Dictionary<string, AssetHandle>();

	private Dictionary<string, AssetBundleObject> objectDictionary = new Dictionary<string, AssetBundleObject>();

	private static bool _isCryptAssetFileName = false;

	private AsyncJob _asyncJobDownload;

	private AsyncJob _asyncJobLoad;

	private static string _savePath = null;

	private string manifestSavePath = "";

	private string bundleSavePath = "";

	private string soundSavePath = "";

	private string movieSavePath = "";

	private string fontSavePath = "";

	private int loadingReqCount;

	private int loadingCompCount;

	private int loadingManifestCompCount;

	public HashSet<string> predownloadCategories = new HashSet<string> { "common", "tutorial" };

	public HashSet<string> tutorialdownloadCategories = new HashSet<string> { "tutorial" };

	public List<string> NoUnloadAssetName = new List<string>();

	private float _downloadCompletedSize;

	private List<string> _temporaryVoiceNameList = new List<string>();

	public string MovieManifesHeadtName = "moviemanifest";

	public string SoundManifesHeadtName = "soundmanifest";

	private bool _isLocalDatahashStoreRecoveryMode;

	private ManifestDatahashKVS _localDatahashStore;

	public static bool isCryptAssetFileName => _isCryptAssetFileName;

	public bool IsResourceDataBaseError { get; private set; }

	public bool IsBackgroundDownload { get; private set; }

	public string manifestOfManifests { get; set; }

	public string manifestOfManifests_sub { get; set; }

	public void CancelDownloadAsyncJob()
	{
		_asyncJobDownload.Cancel();
	}

	private ManifestDatahashKVS GetLocalDatahashStore()
	{
		if (_localDatahashStore == null && !_isLocalDatahashStoreRecoveryMode)
		{
			string localDatahashStorePath = GetLocalDatahashStorePath();
			try
			{
				_localDatahashStore = new ManifestDatahashKVS(localDatahashStorePath);
			}
			catch (Exception ex)
			{
				HandleLocalDatahashException(ex);
			}
		}
		return _localDatahashStore;
	}

	public string GetLocalDatahashStorePath()
	{
		return GetAssetSaveRootPath() + "manifest.db";
	}

	private void HandleLocalDatahashException(Exception ex)
	{
		if (ex is DatabaseCorruptionException)
		{
			IsResourceDataBaseError = true;
			_isLocalDatahashStoreRecoveryMode = true;
			UnloadManifestHashDB();
			string localDatahashStorePath = GetLocalDatahashStorePath();
			if (File.Exists(localDatahashStorePath))
			{
				File.Delete(localDatahashStorePath);
			}
			UIManager instance = UIManager.GetInstance();
			if ((bool)instance)
			{
				instance.CreateConfirmationDialog(Data.SystemText.Get("System_0058")).OnClose = delegate
				{
					SoftwareReset.setAction();
					SoftwareReset.exec();
				};
			}
			else
			{
				SoftwareResetBase.SoftwareReset(null, null);
			}
			return;
		}
		throw ex;
	}

	public void SaveLocalDatahash(string name, string hash)
	{
		ManifestDatahashKVS localDatahashStore = GetLocalDatahashStore();
		if (localDatahashStore != null)
		{
			try
			{
				localDatahashStore.Set(name, hash);
			}
			catch (Exception ex)
			{
				HandleLocalDatahashException(ex);
			}
		}
	}

	public string GetLocalDatahash(string name)
	{
		ManifestDatahashKVS localDatahashStore = GetLocalDatahashStore();
		if (localDatahashStore != null)
		{
			try
			{
				return localDatahashStore.Get(name);
			}
			catch (Exception ex)
			{
				HandleLocalDatahashException(ex);
			}
		}
		return "";
	}

	public void UnloadManifestHashDB()
	{
		if (_localDatahashStore != null)
		{
			_localDatahashStore.Dispose();
			_localDatahashStore = null;
		}
	}

	public static string GetAssetSaveRootPath()
	{
		return _savePath;
	}

	public bool RegistHandle(string key, AssetHandle handle)
	{
		try
		{
			handleDictionary.Add(key, handle);
			if (handle.directory.StartsWith("v/t", StringComparison.Ordinal))
			{
				_temporaryVoiceNameList.Add(Path.GetFileNameWithoutExtension(handle.filename));
			}
		}
		catch (Exception ex)
		{
			AssetHandle assetHandle = GetAssetHandle(key);
			if (assetHandle != null && assetHandle.isMultipleHandleIgnorAsset)
			{
				assetHandle.CopyWithCatchException(handle);
				assetHandle.reference--;
				return true;
			}
			Debug.LogError(ex.Message);
			return false;
		}
		return true;
	}

	public void SetAssetBundle(string filename, AssetBundle assetbundle, bool isMultipleHandleIgnorAsset = false)
	{
		if (objectDictionary.TryGetValue(filename, out var value))
		{
			if (!isMultipleHandleIgnorAsset)
			{
				for (int i = 0; i < value.objectArray.Count; i++)
				{
					UnityEngine.Object.DestroyImmediate(value.objectArray[i].baseObject, allowDestroyingAssets: true);
				}
			}
			value.objectArray.Clear();
			if (value.assetBundle != null)
			{
				value.assetBundle.Unload(unloadAllLoadedObjects: true);
			}
			value.assetBundle = assetbundle;
		}
		else
		{
			value = new AssetBundleObject();
			value.assetBundle = assetbundle;
			objectDictionary.Add(filename, value);
		}
	}

	public void SetObjectList(string filename, List<AssetObject> objectList)
	{
		if (objectDictionary.TryGetValue(filename, out var value))
		{
			value.objectArray = objectList;
		}
	}

	public bool HasObjectList(string filename)
	{
		return objectDictionary.ContainsKey(filename);
	}

	public void UnloadAssetBundle(string assetName)
	{
		if (objectDictionary.TryGetValue(assetName, out var value))
		{
			for (int i = 0; i < value.objectArray.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(value.objectArray[i].baseObject, allowDestroyingAssets: true);
			}
			value.objectArray.Clear();
			if (value.assetBundle != null)
			{
				value.assetBundle.Unload(unloadAllLoadedObjects: true);
				value.assetBundle = null;
			}
			objectDictionary.Remove(assetName);
		}
	}

	public void UnloadAsset(string assetName)
	{
		if (!string.IsNullOrEmpty(assetName) && handleDictionary.TryGetValue(assetName, out var value) && value.assetType != AssetHandle.AssetType.Movie)
		{
			value.Unload();
		}
	}

	public void CacheAsset(string assetName, AssetRequestContext requestContext)
	{
		AssetHandle value = null;
		if (handleDictionary.TryGetValue(assetName, out value) && !value.useStreamingAsset)
		{
			value.Load(requestContext);
		}
		else if (requestContext != null && requestContext.callback != null)
		{
			requestContext.callback(value);
		}
	}

	public UnityEngine.Object LoadObject(string objectName, Type type, bool isIfFindLoad = false)
	{
		string value = objectName.ToLower() + ".";
		foreach (AssetBundleObject value2 in objectDictionary.Values)
		{
			int count = value2.objectArray.Count;
			for (int i = 0; i < count; i++)
			{
				AssetObject assetObject = value2.objectArray[i];
				if (assetObject.baseObject != null && (assetObject.baseObject.GetType() == type || assetObject.baseObject.GetType().IsSubclassOf(type)) && assetObject.basePath.IndexOf(value, StringComparison.Ordinal) != -1)
				{
					return assetObject.baseObject;
				}
			}
		}
		return null;
	}

	public void AddDownloadJob(IEnumerator enumerator, Action cancelAction)
	{
		_asyncJobDownload.Add(enumerator, cancelAction);
	}

	public void AddLoadJob(IEnumerator enumerator, Action cancelAction)
	{
		_asyncJobLoad.Add(enumerator, cancelAction);
	}

	public AssetHandle GetAssetHandle(string assetName, bool isWarning = true)
	{
		AssetHandle value = null;
		handleDictionary.TryGetValue(assetName, out value);
		return value;
	}

	public string getAssetSavePath(AssetHandle.AssetType _assetType)
	{
		switch (_assetType)
		{
		case AssetHandle.AssetType.Manifests:
			return manifestSavePath;
		case AssetHandle.AssetType.AssetBundle:
			return bundleSavePath;
		case AssetHandle.AssetType.Sound:
		case AssetHandle.AssetType.TemporarySound:
			return soundSavePath;
		case AssetHandle.AssetType.Movie:
			return movieSavePath;
		case AssetHandle.AssetType.Font:
			return fontSavePath;
		default:
			return bundleSavePath;
		}
	}

	public void AddManifestCount()
	{
		loadingManifestCompCount++;
	}

	public void AddDownloadCompletedSize(float size)
	{
		_downloadCompletedSize += size;
	}

	public void AddLoadingMaxCount(int cnt = -1)
	{
		if (cnt == -1)
		{
			loadingReqCount++;
		}
		else
		{
			loadingReqCount += cnt;
		}
	}

	public void AddLoadingCurrentCount(string strFileName)
	{
		loadingCompCount++;
	}
}
