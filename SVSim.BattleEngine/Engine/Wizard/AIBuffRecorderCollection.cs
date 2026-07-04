using System.Collections.Generic;

namespace Wizard;

public class AIBuffRecorderCollection
{
	public class AIBuffRecorder : BuffCountInfo
	{
		public int SimulateAtkBuffValue;

		public int SimulateLifeBuffValue;

		public AIBuffRecorder(int turn, bool isSelfTurn, int atkBuff = 0, int lifeBuff = 0)
			: base(turn, isSelfTurn)
		{
			SimulateAtkBuffValue = atkBuff;
			SimulateLifeBuffValue = lifeBuff;
		}
	}

	public List<AIBuffRecorder> RecorderList { get; private set; }

	public int Count
	{
		get
		{
			if (RecorderList != null)
			{
				return RecorderList.Count;
			}
			return 0;
		}
	}

	public AIBuffRecorderCollection()
	{
		RecorderList = null;
	}

	public AIBuffRecorderCollection(List<BuffCountInfo> turnBuffCountList)
	{
		RecorderList = null;
		CreateRecorderListFromTurnBuffCountList(turnBuffCountList);
	}

	private AIBuffRecorderCollection(AIBuffRecorderCollection original)
	{
		RecorderList = AIParamQuery.CloneList(original.RecorderList);
	}

	public AIBuffRecorderCollection Clone()
	{
		return new AIBuffRecorderCollection(this);
	}

	public void AddBuffRecord(int turn, bool isSelfTurn, int atkBuff = 0, int lifeBuff = 0)
	{
		AIBuffRecorder element = new AIBuffRecorder(turn, isSelfTurn, atkBuff, lifeBuff);
		RecorderList = AIParamQuery.AddElementToList(element, RecorderList);
	}

	public int GetTurnBuffCount(int turn, bool isSelfTurn)
	{
		if (Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < RecorderList.Count; i++)
		{
			AIBuffRecorder aIBuffRecorder = RecorderList[i];
			if (aIBuffRecorder.Turn == turn && aIBuffRecorder.IsSelfTurn == isSelfTurn)
			{
				num++;
			}
		}
		return num;
	}

	public (int atkSum, int lifeSum) GetSimulateBuff()
	{
		int num = 0;
		int num2 = 0;
		if (0 < Count)
		{
			for (int i = 0; i < RecorderList.Count; i++)
			{
				AIBuffRecorder aIBuffRecorder = RecorderList[i];
				num += aIBuffRecorder.SimulateAtkBuffValue;
				num2 += aIBuffRecorder.SimulateLifeBuffValue;
			}
		}
		return (atkSum: num, lifeSum: num2);
	}

	public void CreateRecorderListFromTurnBuffCountList(List<BuffCountInfo> turnBuffCountList)
	{
		if (turnBuffCountList != null && turnBuffCountList.Count > 0)
		{
			RecorderList = new List<AIBuffRecorder>();
			for (int i = 0; i < turnBuffCountList.Count; i++)
			{
				BuffCountInfo buffCountInfo = turnBuffCountList[i];
				AIBuffRecorder item = new AIBuffRecorder(buffCountInfo.Turn, !buffCountInfo.IsSelfTurn);
				RecorderList.Add(item);
			}
		}
	}
}
