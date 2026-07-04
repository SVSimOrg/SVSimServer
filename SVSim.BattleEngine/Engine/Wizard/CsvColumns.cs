namespace Wizard;

public class CsvColumns
{
	private int _index = -1;

	private string[] _columnArray;

	private string _fileName;

	public string NextValue
	{
		get
		{
			_index++;
			return _columnArray[_index];
		}
	}

	public CsvColumns(string[] columns, string fileName)
	{
		_columnArray = columns;
		_fileName = fileName;
	}
}
