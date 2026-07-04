namespace Wizard;

public static class AILeaderLifeEvaluationUtility
{
	private static float[] LEADER_LIFE_VALUE_20_TO_1 = new float[19]
	{
		0.55f, 0.627f, 0.704f, 0.781f, 0.858f, 0.935f, 1.012f, 1.088f, 1.165f, 1.242f,
		1.319f, 1.396f, 1.473f, 1.55f, 1.627f, 1.704f, 1.781f, 1.858f, 1.935f
	};

	private static float LEADER_LIFE_VALUE_OVER_20 = 0.5f;

	private static float LETHAL_VALUE_ALLY = -10000f;

	private static float LETHAL_VALUE_OPPONENT = 1000f;

	private static float OVER_KILL_VALUE_RATE = 10f;

	public static float Evaluate(int currentLife, int defaultLife, bool isAllyLeader, bool isAllyOwner)
	{
		float num = 0f;
		if (currentLife > 0)
		{
			int num2 = defaultLife;
			bool flag = currentLife < defaultLife;
			while (num2 != currentLife)
			{
				if (!flag)
				{
					num2++;
				}
				float num3 = 0f;
				num3 = ((num2 <= 20) ? (num3 + LEADER_LIFE_VALUE_20_TO_1[20 - num2]) : (num3 + LEADER_LIFE_VALUE_OVER_20));
				num += num3 * (flag ? (-1f) : 1f);
				if (flag)
				{
					num2--;
				}
			}
			num *= (isAllyLeader ? 1f : (-1f));
		}
		else
		{
			num += (isAllyLeader ? LETHAL_VALUE_ALLY : LETHAL_VALUE_OPPONENT);
			num += (isAllyLeader ? 1f : (-1f)) * OVER_KILL_VALUE_RATE * (float)currentLife;
		}
		return num * (isAllyOwner ? 1f : (-1f));
	}
}
