using System;

public class DocumentationLinkAttribute : Attribute
{
	public string DisplayName { get; private set; }

	public DocumentationLinkAttribute(string displayName)
	{
		DisplayName = displayName;
	}
}
