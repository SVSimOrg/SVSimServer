using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class SleeveMgr
{
	private List<Sleeve> _list = new List<Sleeve>();

	public List<Sleeve> GetAcquiredList()
	{
		return _list.FindAll((Sleeve x) => x.IsAcquired);
	}

	public IEnumerable<long> GetFavorites()
	{
		return from x in _list.FindAll((Sleeve x) => x.IsFavorite)
			select x.sleeve_id;
	}

	public bool IsContainsInMaster(long id)
	{
		return _list.Find((Sleeve x) => x.sleeve_id == id) != null;
	}

	public Sleeve Get(long id)
	{
		Sleeve sleeve = _list.Find((Sleeve x) => x.sleeve_id == id);
		if (sleeve == null)
		{
			sleeve = Get(3000011L);
		}
		if (!sleeve.IsEnableInCurrentLanguage)
		{
			sleeve = Get(3000011L);
		}
		return sleeve;
	}

	public string GetResourcePath(long id)
	{
		return Get(id).sleeve_path;
	}

	public void Acquired(long id)
	{
		Get(id).Acquired();
	}

	public void UnsetNew(long id)
	{
		Get(id).UnsetNew();
	}

	public void SetFavorite(long id, bool b)
	{
		Get(id).SetFavorite(b);
	}
}
