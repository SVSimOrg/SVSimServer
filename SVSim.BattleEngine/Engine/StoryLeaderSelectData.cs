public class StoryLeaderSelectData
{
	public int CharaId { get; set; }

	public bool IsFinished { get; set; }

	public string CurrentChapter { get; set; }

	// Pre-Phase-5b: pulled class_id via DataMgr.GetCharaPrmByCharaId. Headless has no
	// chara master; return 0 as a headless-safe default.
	public int ClassId => 0;
}
