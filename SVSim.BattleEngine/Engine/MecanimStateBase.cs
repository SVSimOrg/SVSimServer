using UnityEngine;

public class MecanimStateBase : MonoBehaviour
{
	[SerializeField]
	private string m_openStateName;

	[SerializeField]
	private string m_closeStateName;

	[SerializeField]
	private MecanimStateBase m_autoNextState;

	public string OpenStateName
	{
		get
		{
			return m_openStateName;
		}
		set
		{
			m_openStateName = value;
		}
	}

	public string CloseStateName
	{
		get
		{
			return m_closeStateName;
		}
		set
		{
			m_closeStateName = value;
		}
	}

	public MecanimStateBase NextState
	{
		get
		{
			return m_autoNextState;
		}
		set
		{
			m_autoNextState = value;
		}
	}

	public bool DontMove { get; private set; }

	public virtual bool onOpenRequest(MecanimStateBase prev, bool isSkip)
	{
		return true;
	}

	public virtual void onOpen()
	{
		DontMove = false;
	}

	public virtual void onUpdateOpenAnim()
	{
	}

	public virtual void onFinishOpenAnim()
	{
	}

	public virtual bool onCloseRequest(MecanimStateBase next, bool isSkip)
	{
		return true;
	}

	public virtual void onClose()
	{
		DontMove = true;
	}

	public virtual void onUpdateCloseAnim()
	{
	}

	public virtual void onFinishCloseAnim()
	{
	}

	public virtual void onMove()
	{
	}

	public virtual void onNotify(int value)
	{
	}
}
