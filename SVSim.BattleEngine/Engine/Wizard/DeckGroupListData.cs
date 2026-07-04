using System.Collections.Generic;
using System.Linq;
using LitJson;

namespace Wizard;

public class DeckGroupListData
{
	public List<DeckGroup> DeckGroupList { get; private set; } = new List<DeckGroup>();

	public bool VisiblePreRotation { get; private set; }

	public DeckGroupListData()
	{
	}

	public DeckGroupListData(JsonData jsonData, Format format)
	{
		Initialize(DeckListUtility.ParseDeckInfoResponceData(jsonData, format));
	}

	public DeckGroupListData(DeckGroup deckGroup)
	{
		Initialize(new List<DeckGroup> { deckGroup });
	}

	public DeckGroupListData(List<DeckGroup> deckGroupList)
	{
		Initialize(deckGroupList);
	}

	private void Initialize(List<DeckGroup> deckGroupList)
	{
		DeckGroupList = deckGroupList;
		VisiblePreRotation = Prerelease.Status == Prerelease.eStatus.PRE_ROTATION;
	}

	public List<DeckGroup> SelectDeckGroupList(Format format)
	{
		List<DeckGroup> list = new List<DeckGroup>();
		switch (format)
		{
		case Format.All:
			list = ((!VisiblePreRotation) ? DeckGroupList.Where((DeckGroup group) => group.DeckFormat != Format.PreRotation).ToList() : DeckGroupList);
			break;
		case Format.PreRotation:
			if (VisiblePreRotation)
			{
				list = DeckGroupList.Where((DeckGroup deckGroup) => deckGroup.DeckFormat == Format.PreRotation).ToList();
			}
			break;
		case Format.MyRotation:
			list = DeckGroupList.Where((DeckGroup deckGroup) => deckGroup.DeckFormat == format).ToList();
			list.RemoveAll((DeckGroup deckGroup) => deckGroup.AttributeType == DeckAttributeType.DefaultDeck);
			break;
		default:
			list = DeckGroupList.Where((DeckGroup deckGroup) => deckGroup.DeckFormat == format).ToList();
			break;
		}
		return list;
	}

	public List<DeckData> GetDeckListByAttribute(DeckAttributeType deckAttributeType, Format format = Format.All)
	{
		return (from deck in GetDeckListByFormat(format)
			where deck.DeckAttributeType == deckAttributeType
			select deck).ToList();
	}

	public DeckData GetDeckByAttribute(DeckAttributeType deckAttributeType, int deckId, Format format)
	{
		return GetDeckListByAttribute(deckAttributeType, format).FirstOrDefault((DeckData deckData) => deckData.GetDeckID() == deckId);
	}

	public List<DeckData> GetDeckListByFormat(Format format)
	{
		List<DeckData> list = new List<DeckData>();
		foreach (DeckGroup item in SelectDeckGroupList(format))
		{
			list.AddRange(item.DeckDataList);
		}
		return list;
	}

	public bool IsExistDeckListByFormat(Format format = Format.All)
	{
		foreach (DeckData item in GetDeckListByFormat(format))
		{
			if (!item.IsNoCard())
			{
				return true;
			}
		}
		return false;
	}
}
