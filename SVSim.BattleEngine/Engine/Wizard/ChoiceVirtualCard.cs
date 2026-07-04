namespace Wizard;

public class ChoiceVirtualCard : AIVirtualCard
{
	public BattleCardBase VirtualBattleCard { get; private set; }

	public ChoiceVirtualCard(BattleCardBase card, bool isAlly, AIVirtualField field)
	{
		VirtualBattleCard = card;
		base.IsAlly = isAlly;
		_field = field;
		InitializeFromBattleCardBase(card);
	}

	protected override void InitializeFromBattleCardBase(BattleCardBase origin)
	{
		InitializeFromBattleCardBaseBasic(origin);
		Cost = origin.Cost;
		base.CardIndex = origin.BaseParameter.BaseCardId;
		IsPlayer = origin.IsPlayer;
		base.TagCollectionContainer = new AITagCollectionContainer();
		BarrierInfoCollection = new AIBarrierInfoCollection();
		if (origin.Tribe != null && origin.Tribe.Count > 0)
		{
			for (int i = 0; i < origin.Tribe.Count; i++)
			{
				AppendTribe(origin.Tribe[i]);
			}
		}
	}

	public override ulong GetHash()
	{
		ulong num = 0uL;
		num += (ulong)((long)base.Attack * 6337L);
		num += (ulong)((long)base.Life * 11383L);
		num += (ulong)((long)base.DefLife * 173L);
		num += (ulong)((long)base.EvolutionAttack * 1488017L);
		num += (ulong)((long)base.EvolutionLife * 937477L);
		num += (ulong)((long)Cost * 14401L);
		num += PRIME_NUMBERS_FOR_CLAN[(int)base.Clan % PRIME_NUMBERS_FOR_CLAN.Length];
		if (_tribeList != null)
		{
			for (int i = 0; i < _tribeList.Count; i++)
			{
				num += PRIME_NUMBERS_FOR_TRIBE[(int)_tribeList[i] % PRIME_NUMBERS_FOR_TRIBE.Length];
			}
		}
		return num;
	}
}
