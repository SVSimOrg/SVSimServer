using System.Collections.Generic;

namespace Cute;

public class AssetErrorState
{
	public enum Code
	{
		NONE = 0,
		LOCAL_CAPACITY_OVER = 4,
		CANCELED = 8,
		FILE_READ_ERROR = 0x10	}

	public enum DialogDecision
	{
		UNDECIDED,
		RETRY,
		TERMINATE
	}

	private Dictionary<string, Code> errors = new Dictionary<string, Code>();

	public DialogDecision lastDialogDecision;

	public int errorFlag { get; private set; }

	public bool canceled { get; private set; }

	public bool HasError()
	{
		return errorFlag != 0;
	}

	public void SetCanceled()
	{
		canceled = true;
	}

	public AssetErrorState()
	{
		Reset();
	}

	public void Report(string filename, Code errorCode)
	{
		if (errorCode != Code.NONE)
		{
			errorFlag |= (int)errorCode;
			errors[filename] = errorCode;
		}
	}

	public void Reset()
	{
		errorFlag = 0;
		lastDialogDecision = DialogDecision.UNDECIDED;
		errors.Clear();
		canceled = false;
	}

	public List<string> GatherErrorFilenames()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Code> error in errors)
		{
			list.Add(error.Key);
		}
		return list;
	}
}
