using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using UnityEngine;
using Wizard;

namespace Cute;

public class DeviceManager : MonoBehaviour, IManager
{
	public enum TextureCompression
	{
		DXT	}

	public enum DeviceType
	{
		NONE	}

	private string strBuildVersionName = "9.9.9";

	private TextureCompression textureCommpression;

	private IPAddress _ipAddress;

	private string _winOsVersion;

	private string _getIpAddressWithFamilyTypeLog = "";

	private void Awake()
	{
		CheckTextureCompression();
	}

	private void CheckTextureCompression()
	{
		textureCommpression = TextureCompression.DXT;
	}

	public string GetOsVersion()
	{
		if (string.IsNullOrEmpty(_winOsVersion))
		{
			try
			{
				string operatingSystem = SystemInfo.operatingSystem;
				string value = Environment.OSVersion.Version.ToString();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(operatingSystem.Substring(0, operatingSystem.IndexOf('(') + 1));
				stringBuilder.Append(value);
				stringBuilder.Append(operatingSystem.Substring(operatingSystem.IndexOf(')')));
				_winOsVersion = stringBuilder.ToString();
			}
			catch (Exception)
			{
				_winOsVersion = SystemInfo.operatingSystem;
			}
		}
		return _winOsVersion;
	}

	public int GetDeviceType()
	{
		return 3;
	}

	public string GetAppVersionName()
	{
		return strBuildVersionName;
	}

	public string GetLocale()
	{
		return CustomPreference._localePref;
	}

	public string GetDeviceUniqueIdentifier()
	{
		string text = "";
		text = SystemInfo.deviceUniqueIdentifier;
		if (string.IsNullOrEmpty(text))
		{
			text = "";
		}
		return text;
	}

	public string GetDeviceName()
	{
		return SystemInfo.deviceModel;
	}

	public string GetGraphicsDeviceName(bool textureCheck = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(SystemInfo.graphicsDeviceName);
		if (textureCheck)
		{
			if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGB) && SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGBA8))
			{
				stringBuilder.Append("[ETC2=1]");
			}
			else
			{
				stringBuilder.Append("[ETC2=0]");
			}
			if (SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_6x6) && SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_6x6))
			{
				stringBuilder.Append("[ASTC=1]");
			}
			else
			{
				stringBuilder.Append("[ASTC=0]");
			}
		}
		return stringBuilder.ToString();
	}

	private IPAddress GetIpAddressWithFamilyType(AddressFamily family = AddressFamily.InterNetwork)
	{
		_getIpAddressWithFamilyTypeLog = "";
		_getIpAddressWithFamilyTypeLog += "GetIpAddressWithFamilyType ";
		if (family == AddressFamily.InterNetworkV6 && !Socket.OSSupportsIPv6)
		{
			return null;
		}
		UnicastIPAddressInformation unicastIPAddressInformation = null;
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		_getIpAddressWithFamilyTypeLog += "GetIpAddressWithFamilyType2 ";
		NetworkInterface[] array = allNetworkInterfaces;
		foreach (NetworkInterface networkInterface in array)
		{
			if (networkInterface.OperationalStatus != OperationalStatus.Up)
			{
				_getIpAddressWithFamilyTypeLog = _getIpAddressWithFamilyTypeLog + " OperationalStatus" + networkInterface.OperationalStatus;
				continue;
			}
			NetworkInterfaceType networkInterfaceType = networkInterface.NetworkInterfaceType;
			if (networkInterfaceType != NetworkInterfaceType.Wireless80211 && networkInterfaceType != NetworkInterfaceType.Ethernet)
			{
				_getIpAddressWithFamilyTypeLog = _getIpAddressWithFamilyTypeLog + " Type " + networkInterfaceType;
				continue;
			}
			IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
			if (iPProperties.GatewayAddresses.Count == 0)
			{
				_getIpAddressWithFamilyTypeLog += " GatewayAddresses.Count 0 ";
				continue;
			}
			foreach (UnicastIPAddressInformation unicastAddress in iPProperties.UnicastAddresses)
			{
				if (unicastAddress.Address.AddressFamily != family)
				{
					continue;
				}
				if (IPAddress.IsLoopback(unicastAddress.Address))
				{
					_getIpAddressWithFamilyTypeLog += " IsLoopback ";
					continue;
				}
				if (!unicastAddress.IsDnsEligible)
				{
					if (unicastIPAddressInformation == null)
					{
						unicastIPAddressInformation = unicastAddress;
					}
					_getIpAddressWithFamilyTypeLog += " ip.IsDnsEligible ";
					continue;
				}
				return unicastAddress.Address;
			}
		}
		_getIpAddressWithFamilyTypeLog += "GetIpAddressWithFamilyType3 ";
		return unicastIPAddressInformation?.Address;
	}

	public string GetIpAddress()
	{
		if (_ipAddress != null)
		{
			return _ipAddress.ToString();
		}
		LocalLog.AccumulateTraceInquiryLog("GetIpAddress " + StackTraceUtility.ExtractStackTrace());
		_ipAddress = GetIpAddressWithFamilyType();
		if (_ipAddress == null)
		{
			LocalLog.AccumulateTraceInquiryLog("GetIpAddress Empty " + _getIpAddressWithFamilyTypeLog + " " + StackTraceUtility.ExtractStackTrace());
			return string.Empty;
		}
		return _ipAddress.ToString();
	}

	public void ClearIpAddress()
	{
		_ipAddress = null;
		LocalLog.AccumulateTraceInquiryLog("ClearIpAddress " + StackTraceUtility.ExtractStackTrace());
	}
}
