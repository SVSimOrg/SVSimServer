using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wizard;

public class AIScriptArgumentExpressions
{
	protected List<AIPolishConvertedExpression> _exprList;

	private List<int> _referringIds;

	protected AIScriptTokenArgType[] LegalSelectTypes { get; set; }

	public AIScriptArgumentExpressions(string text)
	{
		CreateLegalSelectTypes();
		if (!(text == ""))
		{
			InitExpressions(text);
		}
	}

	protected virtual void InitExpressions(string text)
	{
		InitExprList(text);
	}

	protected void InitExprList(string text)
	{
		string[] array = text.Split(';');
		_exprList = new List<AIPolishConvertedExpression>();
		foreach (string expression in array)
		{
			AIPolishConvertedExpression aIPolishConvertedExpression = CreateArgumentExpression(expression);
			_exprList.Add(aIPolishConvertedExpression);
			List<int> referringIDLists = aIPolishConvertedExpression.GetReferringIDLists();
			if (referringIDLists != null)
			{
				if (_referringIds == null)
				{
					_referringIds = new List<int>();
				}
				_referringIds.AddRange(referringIDLists);
			}
		}
	}

	private AIPolishConvertedExpression CreateArgumentExpression(string expression)
	{
		string text = expression.TrimStart();
		if (text[0] == '*')
		{
			return new AIPolishConvertedExpression(text.Substring(1), isMultiplyMarked: true);
		}
		return new AIPolishConvertedExpression(expression);
	}

	public int EvalID(int index)
	{
		if (_exprList == null || index >= _exprList.Count)
		{
			return 0;
		}
		return _exprList[index].EvalID();
	}

	public List<int> EvalIDList(int startIndex)
	{
		List<int> list = new List<int>();
		if (_exprList != null && startIndex < _exprList.Count)
		{
			for (int i = startIndex; i < _exprList.Count; i++)
			{
				list.Add(_exprList[i].EvalID());
			}
		}
		return list;
	}

	public string EvalText(int index)
	{
		if (_exprList == null || index >= _exprList.Count)
		{
			return "";
		}
		return _exprList[index].EvalText();
	}

	public virtual List<int> GetReferringOtherInplayIds()
	{
		return _referringIds;
	}

	public float EvalArg(int index, AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		if (_exprList == null || index >= _exprList.Count)
		{
			return 0f;
		}
		return _exprList[index].EvalArg(tagOwner, playPtn, field, situation);
	}

	public bool IsHoldingEVAL()
	{
		if (_exprList != null)
		{
			return _exprList.Any((AIPolishConvertedExpression expr) => expr.IsHoldingEVAL());
		}
		return false;
	}

	protected string[] SplitTextToWords(string text)
	{
		string pattern = "\\s\\;\\s(?=\\{)";
		return Regex.Split(text, pattern);
	}

	protected List<AIScriptTokenBase> GetFilters(List<AIPolishConvertedExpression> exprs)
	{
		List<AIScriptTokenBase> list = new List<AIScriptTokenBase>();
		for (int i = 0; i < exprs.Count; i++)
		{
			AIPolishConvertedExpression aIPolishConvertedExpression = exprs[i];
			if (aIPolishConvertedExpression.TokenList == null)
			{
				continue;
			}
			if (aIPolishConvertedExpression.TokenList.Count > 1)
			{
				list.Add(aIPolishConvertedExpression.CreateCalculationToken());
				continue;
			}
			AIScriptTokenBase aIScriptTokenBase = aIPolishConvertedExpression.TokenList[0];
			if (!(aIScriptTokenBase is AIScriptArgumentToken { ArgumentType: AIScriptTokenArgType.FILTER_END }))
			{
				list.Add(aIScriptTokenBase);
			}
		}
		return list;
	}

	public virtual void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
	}

	public virtual void ExecuteWhenRemove(AIVirtualCard tagOwner, AIVirtualField field, AIPlayTag removingTag)
	{
	}

	protected AIScriptTokenArgType GetFirstTokenArgType(AIPolishConvertedExpression arg)
	{
		AIScriptTokenArgType result = AIScriptTokenArgType.NONE;
		if (arg.TokenList != null && arg.TokenList.Count > 0)
		{
			AIScriptTokenBase aIScriptTokenBase = arg.TokenList[0];
			if (aIScriptTokenBase is AIScriptArgumentToken)
			{
				result = ((AIScriptArgumentToken)aIScriptTokenBase).ArgumentType;
			}
		}
		return result;
	}

	public virtual AITokenIdCollection GetAllRegisterTokenPoolInfo(AIVirtualCard owner)
	{
		if (_exprList == null || _exprList.Count <= 0)
		{
			return null;
		}
		List<int> list = null;
		for (int i = 0; i < _exprList.Count; i++)
		{
			List<AIScriptTokenBase> tokenList = _exprList[i].TokenList;
			if (tokenList != null && tokenList.Count > 0 && tokenList.Count == 1 && tokenList[0] is AIScriptIDToken aIScriptIDToken)
			{
				list = AIParamQuery.AddElementToList(aIScriptIDToken.ID, list);
			}
		}
		if (list == null)
		{
			return null;
		}
		return CreateRegisterTokenPoolInfo(owner, list);
	}

	protected virtual AITokenIdCollection CreateRegisterTokenPoolInfo(AIVirtualCard owner, List<int> idList)
	{
		return AISummonTokenUtility.CreateTokenIdCollectionFromIdList(owner, AIScriptTokenArgType.ALLY, idList, AITokenType.Default);
	}

	protected bool IsSideTokenArgType(AIPolishConvertedExpression arg, out AIScriptTokenArgType dstTokenARgType)
	{
		dstTokenARgType = GetFirstTokenArgType(arg);
		AIScriptTokenArgType aIScriptTokenArgType = dstTokenARgType;
		if ((uint)(aIScriptTokenArgType - 84) <= 2u)
		{
			return true;
		}
		return false;
	}

	protected bool IsLegalSelectType(AIPolishConvertedExpression arg, out AIScriptTokenArgType selectType)
	{
		selectType = AIScriptTokenArgType.NONE;
		if (arg.TokenList != null && arg.TokenList.Count > 0 && arg.TokenList[0] is AIScriptArgumentToken { ArgumentType: var argumentType } && Array.IndexOf(LegalSelectTypes, argumentType) >= 0)
		{
			selectType = argumentType;
			return true;
		}
		return false;
	}

	protected virtual void CreateLegalSelectTypes()
	{
		LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}
}
