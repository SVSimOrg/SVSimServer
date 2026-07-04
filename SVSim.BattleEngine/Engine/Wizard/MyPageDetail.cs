using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Lottery;

namespace Wizard;

public class MyPageDetail
{
	public enum BGType
	{
		Deck,
		CustomBG,
		RandomBG
	}

	public MyPageBGInfo BGInfo { get; set; } = new MyPageBGInfo();
}
