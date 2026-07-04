using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using Wizard;
using Wizard.UI.LoginBonus;

public class LoadDetail
{

	public int RotationLatestCardPackId;

	public UserInfo _userInfo = new UserInfo();

	public UserCrystalCount _userCrystalCount = new UserCrystalCount();

	public UserTutorial _userTutorial = new UserTutorial();

	private List<long> _acquiredEmblemList = new List<long>();

	private List<long> _favoriteEmblemList = new List<long>();

	private List<int> _acquiredDegreeList = new List<int>();

	private List<int> _favoriteDegreeList = new List<int>();

	private List<long> _acquiredSleeveList = new List<long>();

	private List<long> _favoriteSleeveList = new List<long>();

	public UserConfig _userConfig = new UserConfig();

	public Dictionary<int, UserRank> _userRank = new Dictionary<int, UserRank>();

	public Dictionary<int, int> _userItemDict = new Dictionary<int, int>();

	public DateTime _masterResetNextTime;

	public int _receiveInviteCount;

	public List<UnlimitedRestrictedCard> UnlimitedRestrictedCardList { get; private set; } = new List<UnlimitedRestrictedCard>();

	public List<int> RotationCardSetList { get; private set; } = new List<int>();

	public Dictionary<string, int> OverwriteGetRedEtherDict { get; private set; } = new Dictionary<string, int>();

	public Dictionary<string, int> OverwriteUseRedEtherDict { get; private set; } = new Dictionary<string, int>();

	public List<string> AcquiredMyPageBGList { get; private set; } = new List<string>();

	public long DefaultEmblemId { get; private set; }

	public int DefaultDegreeId { get; private set; }

	public List<int> ReprintedBaseCardIds { get; private set; } = new List<int>();

	public List<string> OpenBattleFieldIdList { get; private set; }

	public bool IsAcquiredSleeve(long sleeveId)
	{
		return _acquiredSleeveList.Contains(sleeveId);
	}

	public bool IsFavoriteSleeve(long sleeveId)
	{
		return _favoriteSleeveList.Contains(sleeveId);
	}

	public bool IsAcquiredEmblem(long emblemId)
	{
		return _acquiredEmblemList.Contains(emblemId);
	}

	public bool IsFavoriteEmblem(long emblemId)
	{
		return _favoriteEmblemList.Contains(emblemId);
	}

	public bool IsAcquiredDegree(int degreeId)
	{
		return _acquiredDegreeList.Contains(degreeId);
	}

	public bool IsFavoriteDegree(int degreeId)
	{
		return _favoriteDegreeList.Contains(degreeId);
	}
}
