using System;
using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class DegreeMgr
{
	private List<Degree> _list = new List<Degree>();

	public bool IsContainsInMaster(int id)
	{
		return _list.Find((Degree x) => x._id == id) != null;
	}

	public Degree Get(int id)
	{
		Degree degree = _list.Find((Degree x) => x._id == id);
		if (degree == null)
		{
			degree = Get(Data.Load.data.DefaultDegreeId);
		}
		if (degree != null && !degree.IsEnableInCurrentLanguage)
		{
			degree = Get(Data.Load.data.DefaultDegreeId);
		}
		return degree;
	}

	public string GetResourcePath(int id)
	{
		return Get(id)._path;
	}

	public string GetMaskTexturePath(int id)
	{
		return $"degree_mask_{Get(id)._id}";
	}

	public void Acquired(int id)
	{
		Get(id).Acquired();
	}
}
