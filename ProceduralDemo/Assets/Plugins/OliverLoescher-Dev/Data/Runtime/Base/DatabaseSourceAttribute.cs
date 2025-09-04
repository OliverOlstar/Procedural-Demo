using System;

public class DatabaseSourceAttribute : Attribute
{
    public string FileName { get; }

	/// <summary> Add a shortcut to linked file </summary>
	/// <param name="fileName"> File name including file extention (ex. Data.xlsx) </param>
	public DatabaseSourceAttribute(string fileName)
    {
        FileName = fileName;
    }
}