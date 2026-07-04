using System.Collections.Generic;
// TODO(engine-cleanup-pass2): 2 of 4 methods unrun in baseline
//   Type: Wizard.FormatBehaviorManager
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public static class FormatBehaviorManager
{
	private static Dictionary<Format, IFormatBehavior> _behaviorDic = new Dictionary<Format, IFormatBehavior>();

	public static IFormatBehavior Create(Format format, ConventionDeckList conventionDeckList)
	{
		if (conventionDeckList != null)
		{
			switch (format)
			{
			case Format.Rotation:
				return new ConventionRotationFormatBehavior(conventionDeckList);
			case Format.Unlimited:
				return new ConventionUnlimitedFormatBehavior(conventionDeckList);
			case Format.Crossover:
				return new ConventionCrossoverFormatBehavior(conventionDeckList);
			case Format.MyRotation:
				return new ConventionMyRotationFormatBehavior(conventionDeckList);
			}
		}
		return GetDefaultBehaviour(format);
	}

	public static IFormatBehavior GetDefaultBehaviour(Format format)
	{
		if (_behaviorDic.TryGetValue(format, out var value))
		{
			return value;
		}
		value = CreateDefaultBehaviour(format);
		_behaviorDic.Add(format, value);
		return value;
	}

	private static IFormatBehavior CreateDefaultBehaviour(Format format)
	{
		return format switch
		{
			Format.Rotation => new RotationFormatBehavior(), 
			Format.Unlimited => new UnlimitedFormatBehavior(), 
			Format.Sealed => new SealedFormatBehavior(), 
			Format.PreRotation => new PreRotationFormatBehavior(), 
			Format.Hof => new HofFormatBehavior(), 
			Format.Crossover => new CrossoverFormatBehavior(), 
			Format.MyRotation => new MyRotationFormatBehavior(), 
			Format.Avatar => new AvatarFormatBehavior(), 
			_ => new NoneFormatBehavior(), 
		};
	}
}
