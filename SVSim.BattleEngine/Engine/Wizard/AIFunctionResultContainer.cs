using System.Collections.Generic;

namespace Wizard;

public class AIFunctionResultContainer
{
	private Dictionary<AIScriptTokenFuncType, Dictionary<ulong, float>> _functionResultDic;

	public AIFunctionResultContainer()
	{
		_functionResultDic = new Dictionary<AIScriptTokenFuncType, Dictionary<ulong, float>>();
	}

	public void Clear()
	{
		foreach (AIScriptTokenFuncType key in _functionResultDic.Keys)
		{
			_functionResultDic[key].Clear();
		}
		_functionResultDic.Clear();
	}

	public void AddRecord(AIScriptTokenFuncType funcType, ulong hash, float result)
	{
		if (!_functionResultDic.TryGetValue(funcType, out var value))
		{
			value = new Dictionary<ulong, float>();
			_functionResultDic.Add(funcType, value);
		}
		if (value.ContainsKey(hash))
		{
			AIConsoleUtility.LogError($"AIFunctionResultContainer.AddRecord() error!! conflict key = {hash}");
		}
		else
		{
			value.Add(hash, result);
		}
	}

	public bool GetContainsResultValue(AIScriptTokenFuncType funcType, ulong hash, out float getResult)
	{
		if (_functionResultDic.TryGetValue(funcType, out var value))
		{
			return value.TryGetValue(hash, out getResult);
		}
		getResult = 0f;
		return false;
	}
}
