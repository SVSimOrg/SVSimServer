using System.Collections.Generic;

namespace Wizard;

public class AIVirtualFieldRollBackStackProcessor : AIVirtualFieldRollBackBasicProcessor
{
	private Stack<AIVirtualFieldRollBackRecord> _recordStack;

	public bool HasRecord => _recordStack.Count > 0;

	public AIVirtualFieldRollBackStackProcessor(AIVirtualField targetField)
		: base(targetField)
	{
		_recordStack = new Stack<AIVirtualFieldRollBackRecord>();
	}

	public void RegisterRecord()
	{
		_recordStack.Push(new AIVirtualFieldRollBackRecord(_field));
	}

	public override void ResetVirtualFieldToStart()
	{
		base.ResetVirtualFieldToStart();
		_recordStack.Clear();
	}

	public void RollBackFieldWithIteration(int iteration = 1)
	{
		for (int i = 0; i < iteration; i++)
		{
			if (!HasRecord)
			{
				break;
			}
			AIVirtualFieldRollBackRecord record = _recordStack.Pop();
			RollBackFromOneRecord(record);
		}
	}
}
