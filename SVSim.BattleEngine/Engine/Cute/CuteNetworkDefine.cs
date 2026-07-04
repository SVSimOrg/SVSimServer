using System.Collections.Generic;

namespace Cute;

public static class CuteNetworkDefine
{
	public enum ApiType
	{
		SignUp,
		GameStartCheck,
		CheckSpecialTitle,
		PaymentItemList,
		PaymentStart,
		PaymentCancel,
		PaymentFinish,
		PaymentSendLog,
		PaymentPCItemList,
		PaymentPCStart,
		PaymentPCCancel,
		PaymentPCFinish,
		SteamGetUserInfo,
		SteamMicroTxnInit,
		BirthUpdate,
		AccountMigration,
		GetGameDataBySocialAccount,
		GetTransitionCode,
		TransitionCodeMigration,
		GetGameDataByTransitionCode,
		GetFacebookNonce,
		CheckiCloudUser,
		MigrateiCloudUser
	}

	public enum ACCOUNT_TYPE
	{
		NONE	}

	public enum CONNECT_TYPE
	{
	}

	public static Dictionary<ApiType, string> ApiUrlList = new Dictionary<ApiType, string>
	{
		{
			ApiType.SignUp,
			"tool/signup"
		},
		{
			ApiType.CheckSpecialTitle,
			"check/special_title"
		},
		{
			ApiType.GameStartCheck,
			"check/game_start"
		},
		{
			ApiType.PaymentItemList,
			"payment/item_list"
		},
		{
			ApiType.PaymentStart,
			"payment/start"
		},
		{
			ApiType.PaymentCancel,
			"payment/cancel"
		},
		{
			ApiType.PaymentFinish,
			"payment/finish"
		},
		{
			ApiType.PaymentSendLog,
			"payment/send_log"
		},
		{
			ApiType.PaymentPCItemList,
			"payment_pc/item_list"
		},
		{
			ApiType.PaymentPCStart,
			"payment_pc/start"
		},
		{
			ApiType.PaymentPCCancel,
			"payment_pc/cancel"
		},
		{
			ApiType.PaymentPCFinish,
			"payment_pc/finish"
		},
		{
			ApiType.SteamGetUserInfo,
			"payment_pc/steam_get_user_info"
		},
		{
			ApiType.SteamMicroTxnInit,
			"payment_pc/steam_micro_txn_init"
		},
		{
			ApiType.BirthUpdate,
			"account/update_birth"
		},
		{
			ApiType.AccountMigration,
			"account/chain_by_social_account"
		},
		{
			ApiType.GetGameDataBySocialAccount,
			"account/get_by_social_account"
		},
		{
			ApiType.GetTransitionCode,
			"account/publish_transition_code"
		},
		{
			ApiType.TransitionCodeMigration,
			"account/chain_by_transition_code"
		},
		{
			ApiType.GetGameDataByTransitionCode,
			"account/get_by_transition_code"
		},
		{
			ApiType.GetFacebookNonce,
			"account/get_facebook_nonce"
		},
		{
			ApiType.CheckiCloudUser,
			"account/get_by_icloud_data"
		},
		{
			ApiType.MigrateiCloudUser,
			"account/chain_by_icloud_data"
		}
	};
}
