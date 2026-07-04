using System;
using System.Collections.Generic;

namespace Wizard;

public class SealedClassSelectLoadRequest
{
	public List<string> LoadAssetList { get; private set; } = new List<string>();

	public Action LoadEndCallback { get; set; }
}
