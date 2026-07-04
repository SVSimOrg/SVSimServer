using LitJson;
using Wizard;

public class UserRank : HeaderData
{
	public class GrandMasterData
	{
		public int[] id = new int[3];

		public int[] masterPoint = new int[3];

		public int targetMasterPoint;

		public int currentMasterPoint;
	}

	public int rank;

	public int master_point;

	public bool is_master_rank;

	public bool is_grand_master_rank;

	public GrandMasterData grandMasterData = new GrandMasterData();

	public UserPromotionMatch user_promotion_match = new UserPromotionMatch();

	public static bool IsGrandMasterAvailability { get; set; }
}
