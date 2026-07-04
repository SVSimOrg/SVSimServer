using System;
// TODO(engine-cleanup-pass2): 20 of 21 methods unrun in baseline
//   Type: Cute.EventExtension
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Cute;

public static class EventExtension
{
	public static void Call(this Action action)
	{
		action?.Invoke();
	}

	public static void Call<T1>(this Action<T1> action, T1 arg1)
	{
		action?.Invoke(arg1);
	}

	public static void Call<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
	{
		action?.Invoke(arg1, arg2);
	}

	public static void Call<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
	{
		action?.Invoke(arg1, arg2, arg3);
	}

	public static void Call<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		action?.Invoke(arg1, arg2, arg3, arg4);
	}

	public static void Call<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		action?.Invoke(arg1, arg2, arg3, arg4, arg5);
	}

	public static void Call<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		action?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public static void Call<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		action?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public static void Call<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		action?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
	{
		action?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public static TR Call<TR>(this Func<TR> func)
	{
		if (func == null)
		{
			return default(TR);
		}
		return func();
	}

	public static TR Call<T1, TR>(this Func<T1, TR> func, T1 arg1)
	{
		if (func == null)
		{
			return default(TR);
		}
		return func(arg1);
	}

	public static TR Call<T1, T2, TR>(this Func<T1, T2, TR> func, T1 arg1, T2 arg2)
	{
		if (func == null)
		{
			return default(TR);
		}
		return func(arg1, arg2);
	}

	public static TR Call<T1, T2, T3, TR>(this Func<T1, T2, T3, TR> func, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func == null)
		{
			return default(TR);
		}
		return func(arg1, arg2, arg3);
	}

	public static TR Call<T1, T2, T3, T4, TR>(this Func<T1, T2, T3, T4, TR> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func == null)
		{
			return default(TR);
		}
		return func(arg1, arg2, arg3, arg4);
	}

	public static TR Call<T1, T2, T3, T4, T5, TR>(this Func<T1, T2, T3, T4, T5, TR> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func == null)
		{
			return default(TR);
		}
		return func(arg1, arg2, arg3, arg4, arg5);
	}

	public static TR[] GetAllFuncCallResults<TR>(this Func<TR> func)
	{
		if (func == null)
		{
			return new TR[0];
		}
		return CallAllFunc(func, (Delegate f) => ((Func<TR>)f)());
	}

	public static TR[] GetAllFuncCallResults<T1, TR>(this Func<T1, TR> func, T1 arg1)
	{
		if (func == null)
		{
			return new TR[0];
		}
		return CallAllFunc(func, (Delegate f) => ((Func<T1, TR>)f)(arg1));
	}

	public static TR[] GetAllFuncCallResults<T1, T2, TR>(this Func<T1, T2, TR> func, T1 arg1, T2 arg2)
	{
		if (func == null)
		{
			return new TR[0];
		}
		return CallAllFunc(func, (Delegate f) => ((Func<T1, T2, TR>)f)(arg1, arg2));
	}

	public static TR[] GetAllFuncCallResults<T1, T2, T3, TR>(this Func<T1, T2, T3, TR> func, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func == null)
		{
			return new TR[0];
		}
		return CallAllFunc(func, (Delegate f) => ((Func<T1, T2, T3, TR>)f)(arg1, arg2, arg3));
	}

	private static TR[] CallAllFunc<TR>(Delegate func, Func<Delegate, TR> call)
	{
		Delegate[] invocationList = func.GetInvocationList();
		int num = invocationList.Length;
		TR[] array = new TR[num];
		for (int i = 0; i < num; i++)
		{
			TR val = call(invocationList[i]);
			array[i] = val;
		}
		return array;
	}
}
