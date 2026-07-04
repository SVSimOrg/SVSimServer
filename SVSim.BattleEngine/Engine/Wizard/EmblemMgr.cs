using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class EmblemMgr
{
	private List<Emblem> _list = new List<Emblem>();

	public bool IsContainsInMaster(long id)
	{
		return _list.Find((Emblem x) => x._id == id) != null;
	}

	public Emblem Get(long id)
	{
		Emblem emblem = _list.Find((Emblem x) => x._id == id);
		if (emblem == null)
		{
			emblem = Get(Data.Load.data.DefaultEmblemId);
		}
		if (emblem != null && !emblem.IsEnableInCurrentLanguage)
		{
			emblem = Get(Data.Load.data.DefaultEmblemId);
		}
		return emblem;
	}

	public string GetResourcePath(long id)
	{
		return Get(id)._path;
	}

	public void Acquired(long id)
	{
		Get(id).Acquired();
	}
}
