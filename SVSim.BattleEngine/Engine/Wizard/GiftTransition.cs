using System.Collections.Generic;

namespace Wizard;

public class GiftTransition : Master.ReadFromCsv
{
	public class TransitionButton
	{
		public string _text;

		public SceneTransition.TransitionData _transitionData;
	}

	public int _rewardType;

	public int _rewardDetailId;

	public List<TransitionButton> _buttons = new List<TransitionButton>();

	public void ReadCsvColumns(string[] columns)
	{
		int num = 1;
		_rewardType = int.Parse(columns[num++]);
		int.TryParse(columns[num++], out _rewardDetailId);
		string text = columns[num++];
		while (!string.IsNullOrEmpty(text))
		{
			TransitionButton transitionButton = new TransitionButton();
			transitionButton._text = text;
			transitionButton._transitionData = new SceneTransition.TransitionData(columns[num++]);
			if (int.TryParse(columns[num++], out var result))
			{
				transitionButton._transitionData.Status = result;
			}
			_buttons.Add(transitionButton);
			text = ((num >= columns.Length) ? null : columns[num++]);
		}
	}
}
