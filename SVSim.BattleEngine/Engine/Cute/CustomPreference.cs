using Wizard;
// TODO(engine-cleanup-pass2): 37 of 39 methods unrun in baseline
//   Type: Cute.CustomPreference
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Cute;

public class CustomPreference
{
	public enum eSchemeType
	{
		Http,
		Https,
		File,
		Node,
		StreamingAssets
	}

	public enum PlatformType
	{
		NONE	}

	public enum SmallResourceStatus
	{
	}

	private static string nodeServerScheme = "ws://";

	private static string directoryRoot = "dl/";

	private static string _applicationServerUrl = "";

	private static string _resourceServerUrl = "";

	private static string _deckBuilderServerUrl = "";

	public static string _localePref = "Eng";

	private static string _languagePref = "Eng";

	private static string _languageSoundPref = "Eng";

	private static string _assetbundleurl = "";

	private static string _manifestsuburl = "";

	private static string _soundurl = "";

	private static string _movieurl = "";

	private static string _manifestUrl = "";

	private static eSchemeType _schemeType = eSchemeType.Http;

	private static eSchemeType _schemeCDNType = eSchemeType.Http;

	private static bool _isLocalAssetBundles = false;

	public static bool IsNormalResource => !IsSmallResource;

	public static bool IsSmallResource => PlayerPrefsCache.Instance.GetValue(PlayerPrefsWrapper.SMALL_RESOURCE_STATUS) == 2;

	public static string GetApplicationServerURL()
	{
		return GetScheme() + _applicationServerUrl;
	}

	public static string GetResourceServerURL()
	{
		return GetCDNScheme() + _resourceServerUrl;
	}

	public static string GetDeckBuilderServerURL()
	{
		return GetScheme() + _deckBuilderServerUrl;
	}

	public static int GetPlatform()
	{
		return 4;
	}

	public static string GetAssetBundleURL()
	{
		return _assetbundleurl;
	}

	public static string GetManifestURL()
	{
		return _manifestUrl;
	}

	public static string GetSubManifestURL()
	{
		if (_isLocalAssetBundles)
		{
			_manifestsuburl = GetResourceServerURL() + Utility.GetRuntimePlatform() + "/";
		}
		else
		{
			_manifestsuburl = GetResourceServerURL() + directoryRoot + "Manifest/" + GetVersionFolderName() + GetSoundMovieLanguageFolderName() + Utility.GetRuntimePlatform() + "/";
		}
		return _manifestsuburl;
	}

	public static string GetSoundResourceURL()
	{
		return _soundurl;
	}

	public static string GetMoiveResourceURL()
	{
		return _movieurl;
	}

	public static string GetScheme()
	{
		return _schemeType switch
		{
			eSchemeType.Http => "http://", 
			eSchemeType.Https => "https://", 
			eSchemeType.File => "file:///", 
			eSchemeType.Node => nodeServerScheme, 
			eSchemeType.StreamingAssets => "file:///", 
			_ => "http://", 
		};
	}

	public static string GetCDNScheme()
	{
		return _schemeCDNType switch
		{
			eSchemeType.Http => "http://", 
			eSchemeType.Https => "https://", 
			_ => "http://", 
		};
	}

	public static string GetVersionFolderName()
	{
		return Toolbox.SavedataManager.GetResourceVersion() + "/";
	}

	public static string GetTextLanguage()
	{
		return GetResourceLanguage();
	}

	public static string GetResourceLanguage()
	{
		return _languagePref;
	}

	public static string GetSoundMovieLanguageFolderName()
	{
		return _languageSoundPref + "/";
	}

	public static string GetSoundMovieLanguage()
	{
		return _languageSoundPref;
	}
}
