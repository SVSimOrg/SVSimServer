using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class Effect2dCreateParam
{
	public GameObject Parent { get; set; }

	public string EffectName { get; set; }

	public eColorCodeId? ColorCode { get; set; }

	public bool InitActive { get; set; }

	public Action MaterialSetEndCallback { get; set; }

	public List<string> UnloadAssetList { get; set; }
}
