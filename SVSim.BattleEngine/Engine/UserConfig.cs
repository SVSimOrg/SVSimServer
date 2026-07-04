using Cute;
using LitJson;
using Wizard;

public class UserConfig : HeaderData
{
	public string create_time;

	public bool IsFoilPreferred { get; set; }

	public bool IsPrizePreferred { get; set; }
}
