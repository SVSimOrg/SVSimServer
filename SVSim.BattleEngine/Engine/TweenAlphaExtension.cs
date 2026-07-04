using AnimationOrTween;

public static class TweenAlphaExtension
{
	public static void PlayPingPong(this TweenAlpha tweenAlpha, bool isIncreaseAlpha)
	{
		float num = tweenAlpha.from;
		float num2 = tweenAlpha.to;
		if ((isIncreaseAlpha && num > num2) || (!isIncreaseAlpha && num < num2))
		{
			float num3 = num;
			num = num2;
			num2 = num3;
		}
		bool flag = tweenAlpha.direction == Direction.Forward;
		tweenAlpha.from = (flag ? num : num2);
		tweenAlpha.to = (flag ? num2 : num);
		tweenAlpha.ResetToBeginning();
		tweenAlpha.PlayForward();
	}
}
