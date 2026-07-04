using System.Collections;
using System.Collections.Generic;

namespace Wizard;

public class Mastershop
{
	public class Shop
	{
		private int _id;

		private string _store_product_id;

		private string _name;

		private string _text;

		private int _price;

		private int _charge_jewel;

		private int _free_jewel;

		public int id => _id;

		public string name => _name;

		public Shop(string[] record)
		{
			_id = int.Parse(record[0]);
			_store_product_id = record[1];
			_name = record[2];
			_text = record[3];
			_price = int.Parse(record[4]);
			_charge_jewel = int.Parse(record[5]);
			_free_jewel = int.Parse(record[6]);
		}
	}

	private Dictionary<int, Shop> _dictionary = new Dictionary<int, Shop>();

	public string[] ProductIdList { get; private set; }

	public Mastershop(ArrayList list)
	{
		ProductIdList = new string[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			string[] array = (string[])((ArrayList)list[i]).ToArray(typeof(string));
			int key = int.Parse(array[0]);
			_dictionary.Add(key, new Shop(array));
			ProductIdList[i] = array[1];
		}
	}

	public Mastershop()
	{
	}
}
