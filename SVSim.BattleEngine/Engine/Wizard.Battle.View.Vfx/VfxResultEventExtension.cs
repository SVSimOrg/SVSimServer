using System;
// TODO(engine-cleanup-pass2): 4 of 6 methods unrun in baseline
//   Type: Wizard.Battle.View.Vfx.VfxResultEventExtension
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.View.Vfx;

public static class VfxResultEventExtension
{
	public static VfxBase GetAllFuncVfxResults(this Func<VfxBase> func)
	{
		return CallAllFunc(func, (Delegate f) => ((Func<VfxBase>)f)());
	}

	public static VfxBase GetAllFuncVfxResults<T1>(this Func<T1, VfxBase> func, T1 arg1)
	{
		return CallAllFunc(func, (Delegate f) => ((Func<T1, VfxBase>)f)(arg1));
	}

	public static VfxBase GetAllFuncVfxResults<T1, T2>(this Func<T1, T2, VfxBase> func, T1 arg1, T2 arg2)
	{
		return CallAllFunc(func, (Delegate f) => ((Func<T1, T2, VfxBase>)f)(arg1, arg2));
	}

	public static VfxBase GetAllFuncVfxResults<T1, T2, T3>(this Func<T1, T2, T3, VfxBase> func, T1 arg1, T2 arg2, T3 arg3)
	{
		return CallAllFunc(func, (Delegate f) => ((Func<T1, T2, T3, VfxBase>)f)(arg1, arg2, arg3));
	}

	public static VfxBase GetAllFuncVfxResults<T1, T2, T3, T4>(this Func<T1, T2, T3, T4, VfxBase> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		return CallAllFunc(func, (Delegate f) => ((Func<T1, T2, T3, T4, VfxBase>)f)(arg1, arg2, arg3, arg4));
	}

	private static VfxBase CallAllFunc(Delegate func, Func<Delegate, VfxBase> call)
	{
		if ((object)func == null)
		{
			return NullVfx.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		Delegate[] invocationList = func.GetInvocationList();
		foreach (Delegate arg in invocationList)
		{
			VfxBase vfx = call(arg);
			parallelVfxPlayer.Register(vfx);
		}
		return parallelVfxPlayer;
	}
}
