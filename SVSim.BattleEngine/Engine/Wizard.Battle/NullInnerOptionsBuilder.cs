using Wizard.Battle.Card.InnerOptions;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.View;

namespace Wizard.Battle;

public class NullInnerOptionsBuilder : IInnerOptionsBuilder
{
	private static NullInnerOptionsBuilder m_instance;

	public static NullInnerOptionsBuilder GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new NullInnerOptionsBuilder();
		}
		return m_instance;
	}

	private NullInnerOptionsBuilder()
	{
	}

	public IPlayerEmotion CreatePlayerEmotion(IClassBattleCardView classCardView)
	{
		return new NullPlayerEmotion();
	}

	public IEmotion CreateEnemyEmotion(IClassBattleCardView classCardView)
	{
		return new NullEmotion();
	}

	public CardInnerOptionsBase CreateCardOptions()
	{
		return new CardInnerOptionsBase();
	}
}
