using System.Collections.Generic;

namespace Wizard;

public class AIVariableResultContainer
{
	private Dictionary<AIScriptTokenVariableType, Dictionary<ulong, float>> _variableResultDic;

	public AIVariableResultContainer()
	{
		_variableResultDic = new Dictionary<AIScriptTokenVariableType, Dictionary<ulong, float>>();
	}

	public void Clear()
	{
		foreach (AIScriptTokenVariableType key in _variableResultDic.Keys)
		{
			_variableResultDic[key].Clear();
		}
		_variableResultDic.Clear();
	}

	private void AddRecord(AIScriptTokenVariableType valType, ulong hash, float result)
	{
		if (!_variableResultDic.TryGetValue(valType, out var value))
		{
			value = new Dictionary<ulong, float>();
			_variableResultDic.Add(valType, value);
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

	public bool GetContainsResultValue(AIScriptTokenVariableType valType, ulong hash, out float getResult)
	{
		if (_variableResultDic.TryGetValue(valType, out var value))
		{
			return value.TryGetValue(hash, out getResult);
		}
		getResult = 0f;
		return false;
	}

	public void CheckDuplicateAndAddRecord(AIScriptTokenVariableType type, ulong hash, float result, string errorMessage)
	{
		if (GetContainsResultValue(type, hash, out var getResult))
		{
			if (errorMessage != null && errorMessage != "" && getResult != result)
			{
				AIConsoleUtility.LogError(errorMessage);
			}
		}
		else
		{
			AddRecord(type, hash, result);
		}
	}
}
