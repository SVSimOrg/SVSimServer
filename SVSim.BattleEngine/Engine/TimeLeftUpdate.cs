using System;
using UnityEngine;
using Wizard;

public class TimeLeftUpdate : MonoBehaviour
{
	[NonSerialized]
	public MailData mailData;

	[SerializeField]
	public UILabel timeLeft;

	public void UpdateTime()
	{
		if (mailData != null)
		{
			SystemText systemText = Data.SystemText;
			if (mailData.limit_type == 1)
			{
				timeLeft.text = Mail.GetTimeLeft(mailData.reward_limit_time);
			}
			else
			{
				timeLeft.text = systemText.Get("Mail_0030");
			}
		}
	}
}
