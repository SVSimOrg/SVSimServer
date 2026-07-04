using System.Collections.Generic;
// TODO(engine-cleanup-pass2): 4 of 7 methods unrun in baseline
//   Type: Wizard.Battle.View.Vfx.NullVfxWithLoading
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.View.Vfx;

public class NullVfxWithLoading : VfxWithLoading
{
	private static NullVfxWithLoading _instance;

	public override VfxBase LoadingVfx => NullVfx.GetInstance();

	public override VfxBase MainVfx => NullVfx.GetInstance();

	public override bool IsEnd => true;

	public static NullVfxWithLoading GetInstance()
	{
		if (_instance == null)
		{
			_instance = new NullVfxWithLoading();
		}
		return _instance;
	}

	public override void Play()
	{
	}

	public override void Update(float dt, List<IEffectVfx> effectVfxList)
	{
	}

	public override bool IsVfxNonEmpty()
	{
		return false;
	}
}
