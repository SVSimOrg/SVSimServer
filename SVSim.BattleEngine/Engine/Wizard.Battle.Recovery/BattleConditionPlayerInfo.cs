using System.Collections.Generic;
using System.Linq;
using LitJson;
using Wizard.AutoTest;

namespace Wizard.Battle.Recovery;

public class BattleConditionPlayerInfo
{
	public int? CemeteryCount { get; private set; }

	public int Pp { get; private set; }

	public int ClassId { get; private set; }

	public int SubClassId { get; private set; }

	public string MyRotationId { get; private set; }

	public int CharaId { get; private set; }

	public long SleeveId { get; private set; }

	public IEnumerable<InPlayCardInfo> InPlayCardInfos { get; private set; }

	public IEnumerable<HandCardInfo> HandCardInfos { get; private set; }

	public IEnumerable<DeckCardInfo> DeckCardInfos { get; private set; }

	public IEnumerable<CemeteryCardInfo> CemeteryCardInfos { get; private set; }

	public BattleConditionPlayerInfo(JsonData jsonData, bool useDefaultInPlayCardValue)
	{
		ClassId = jsonData.ToIntOrDefault("clan_type", 1);
		SubClassId = jsonData.ToIntOrDefault("sub_class_type", 1);
		MyRotationId = jsonData.ToStringOrDefault("my_rotation_id", "");
		CharaId = jsonData.ToIntOrDefault("chara_id", 1);
		SleeveId = jsonData.ToLongOrDefault("sleeve_id", 1);
		CemeteryCount = jsonData.ToIntOrNull("cemetery_count");
		Pp = jsonData.ToIntOrDefault("pp", 0);
		InPlayCardInfos = from d in jsonData.ToJsonDataCollection("inplay")
			select new InPlayCardInfo(d, useDefaultInPlayCardValue);
		HandCardInfos = from d in jsonData.ToJsonDataCollection("hand")
			select new HandCardInfo(d, useDefaultInPlayCardValue);
		DeckCardInfos = from d in jsonData.ToJsonDataCollection("deck")
			select new DeckCardInfo(d);
		CemeteryCardInfos = from d in jsonData.ToJsonDataCollection("cemetery")
			select new CemeteryCardInfo(d);
	}
}
