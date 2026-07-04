using UnityEngine;

namespace Wizard;

public class TutorialAreaSelect
{
	public string ChapterId { get; }

	public int TutorialStep { get; }

	public Vector2 MapIconPos { get; }

	public int ChapterButtonBgId { get; }

	public bool ExistsStoryMovie { get; }

	public TutorialAreaSelect(string[] columns)
	{
		int num = 0;
		ChapterId = columns[num++];
		TutorialStep = int.Parse(columns[num++]);
		float x = float.Parse(columns[num++]);
		float y = float.Parse(columns[num++]);
		MapIconPos = new Vector2(x, y);
		ChapterButtonBgId = int.Parse(columns[num++]);
		ExistsStoryMovie = int.Parse(columns[num++]) == 1;
	}
}
