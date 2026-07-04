using System;
using System.Collections.Generic;

namespace Wizard;

public class ArenaCommonLobbyLoadRequest
{
	public List<string> LoadAssetList { get; set; }

	public Action LoadEndCallback { get; set; }

	public ArenaCommonLobbyLoadRequest()
	{
		LoadAssetList = new List<string>();
	}
}
