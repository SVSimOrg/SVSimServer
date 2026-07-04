using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Wizard;

public class AchievementImpl : MonoBehaviour
{
	public enum eAchievementID
	{
		ELF10,
		ELF25,
		ELF40,
		RYL10,
		RYL25,
		RYL40,
		WCH10,
		WCH25,
		WCH40,
		DGN10,
		DGN25,
		DGN40,
		NCR10,
		NCR25,
		NCR40,
		VMP10,
		VMP25,
		VMP40,
		BSP10,
		BSP25,
		BSP40,
		RANK_D,
		RANK_C,
		RANK_B,
		RANK_A,
		NEM10,
		NEM25,
		NEM40
	}

	public class AchievementCallbackImpl : IAchievementCallback
	{
		private bool _bShowAchievementAfterSignIn;

		public void OnSignIn(bool success)
		{
			if (success)
			{
				instance.mIsSignIn = true;
				if (_bShowAchievementAfterSignIn)
				{
					AchievementManager.ShowAchievementsUI();
				}
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.SOCIAL_ACHIEVEMENT_SIGNIN, flag: true);
			}
			else
			{
				PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.SOCIAL_ACHIEVEMENT_SIGNIN, flag: false);
			}
			_bShowAchievementAfterSignIn = false;
		}

		public void OnSignOut()
		{
			instance.mIsSignIn = false;
			PlayerPrefsWrapper.SetBool(PlayerPrefsWrapper.SOCIAL_ACHIEVEMENT_SIGNIN, flag: false);
		}

		public void OnReleaseAchievement(bool success)
		{
		}

		public void OnProceedAchievement(bool success)
		{
		}

		public void OnLoadAchievements(IAchievement[] achievements)
		{
		}

		public void OnLoadAchievementDescriptions(IAchievementDescription[] descriptions)
		{
		}
	}

	private readonly string[] mszAchievementID = new string[36]
	{
		"CgkIz8_azvQHEAIQAQ", "CgkIz8_azvQHEAIQAg", "CgkIz8_azvQHEAIQAw", "CgkIz8_azvQHEAIQBA", "CgkIz8_azvQHEAIQBQ", "CgkIz8_azvQHEAIQBg", "CgkIz8_azvQHEAIQBw", "CgkIz8_azvQHEAIQCA", "CgkIz8_azvQHEAIQCQ", "CgkIz8_azvQHEAIQCg",
		"CgkIz8_azvQHEAIQCw", "CgkIz8_azvQHEAIQDA", "CgkIz8_azvQHEAIQDQ", "CgkIz8_azvQHEAIQDg", "CgkIz8_azvQHEAIQDw", "CgkIz8_azvQHEAIQEA", "CgkIz8_azvQHEAIQEQ", "CgkIz8_azvQHEAIQEg", "CgkIz8_azvQHEAIQEw", "CgkIz8_azvQHEAIQFA",
		"CgkIz8_azvQHEAIQFQ", "CgkIz8_azvQHEAIQFg", "CgkIz8_azvQHEAIQFw", "CgkIz8_azvQHEAIQGA", "CgkIz8_azvQHEAIQGQ", "CgkIz8_azvQHEAIQGg", "CgkIz8_azvQHEAIQGw", "CgkIz8_azvQHEAIQHA", "CgkIz8_azvQHEAIQHQ", "CgkIz8_azvQHEAIQHg",
		"CgkIz8_azvQHEAIQHw", "CgkIz8_azvQHEAIQIA", "CgkIz8_azvQHEAIQIQ", "CgkIz8_azvQHEAIQJA", "CgkIz8_azvQHEAIQJQ", "CgkIz8_azvQHEAIQJg"
	};

	private bool mIsSignIn;

	private AchievementCallbackImpl mCallbackImpl;

	public static AchievementImpl instance;

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (mCallbackImpl == null)
		{
			mCallbackImpl = new AchievementCallbackImpl();
		}
		AchievementManager.Initialize(mCallbackImpl);
		instance = this;
	}

	public void ReleaseAchievement(eAchievementID id)
	{
		if (mIsSignIn)
		{
			AchievementManager.ReleaseAchievement(mszAchievementID[(int)id]);
		}
	}

	public void ReleaseAchievement(string id)
	{
		if (mIsSignIn)
		{
			AchievementManager.ReleaseAchievement(id);
			ReleaseAchievementShouldBeReleased();
		}
	}

	private void ReleaseAchievementShouldBeReleased()
	{
		for (int i = 1; i < 9; i++)
		{
			ClassCharaPrm classPrm = null; // Pre-Phase-5b: no class prm headless
			Dictionary<int, eAchievementID> achievementIDsByClass = getAchievementIDsByClass((CardBasePrm.ClanType)i);
			int classCharaLv = classPrm.GetClassCharaLv();
			if (classCharaLv >= 10)
			{
				ReleaseAchievement(achievementIDsByClass[10]);
			}
			if (classCharaLv >= 25)
			{
				ReleaseAchievement(achievementIDsByClass[25]);
			}
			if (classCharaLv >= 40)
			{
				ReleaseAchievement(achievementIDsByClass[40]);
			}
		}
		for (int j = 0; j < 2; j++)
		{
			int num = PlayerStaticData.UserRank((Format)j);
			if (num >= 5)
			{
				ReleaseAchievement(eAchievementID.RANK_D);
			}
			if (num >= 9)
			{
				ReleaseAchievement(eAchievementID.RANK_C);
			}
			if (num >= 13)
			{
				ReleaseAchievement(eAchievementID.RANK_B);
			}
			if (num >= 17)
			{
				ReleaseAchievement(eAchievementID.RANK_A);
			}
		}
	}

	private Dictionary<int, eAchievementID> getAchievementIDsByClass(CardBasePrm.ClanType type)
	{
		return type switch
		{
			CardBasePrm.ClanType.MIN => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.ELF10
				},
				{
					25,
					eAchievementID.ELF25
				},
				{
					40,
					eAchievementID.ELF40
				}
			}, 
			CardBasePrm.ClanType.ROYAL => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.RYL10
				},
				{
					25,
					eAchievementID.RYL25
				},
				{
					40,
					eAchievementID.RYL40
				}
			}, 
			CardBasePrm.ClanType.WITCH => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.WCH10
				},
				{
					25,
					eAchievementID.WCH25
				},
				{
					40,
					eAchievementID.WCH40
				}
			}, 
			CardBasePrm.ClanType.DRAGON => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.DGN10
				},
				{
					25,
					eAchievementID.DGN25
				},
				{
					40,
					eAchievementID.DGN40
				}
			}, 
			CardBasePrm.ClanType.NECRO => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.NCR10
				},
				{
					25,
					eAchievementID.NCR25
				},
				{
					40,
					eAchievementID.NCR40
				}
			}, 
			CardBasePrm.ClanType.VAMPIRE => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.VMP10
				},
				{
					25,
					eAchievementID.VMP25
				},
				{
					40,
					eAchievementID.VMP40
				}
			}, 
			CardBasePrm.ClanType.BISHOP => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.BSP10
				},
				{
					25,
					eAchievementID.BSP25
				},
				{
					40,
					eAchievementID.BSP40
				}
			}, 
			CardBasePrm.ClanType.NEMESIS => new Dictionary<int, eAchievementID>
			{
				{
					10,
					eAchievementID.NEM10
				},
				{
					25,
					eAchievementID.NEM25
				},
				{
					40,
					eAchievementID.NEM40
				}
			}, 
			_ => throw new Exception("bad ClanType: " + type), 
		};
	}
}
