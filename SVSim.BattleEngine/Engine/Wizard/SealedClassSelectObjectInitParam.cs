using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class SealedClassSelectObjectInitParam
{
	public ClassCharacterMasterData CharaParam { get; set; }

	public List<CardListTemplate> CardObjectList { get; set; }

	public Action SelectButtonClickCallback { get; set; }

	public GameObject EffectRoot { get; set; }

	public List<string> UnloadAssetList { get; set; }
}
