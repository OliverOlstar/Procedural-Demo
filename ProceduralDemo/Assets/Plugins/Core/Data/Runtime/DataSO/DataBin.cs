using Newtonsoft.Json;
using UnityEngine;

public class DataBin : Data.IDataDictItem
{
    [SerializeField]
    protected string m_ID = string.Empty;
    [JsonProperty] // Often data with a custom server json format will require members to opt into serialization
    public string ID => m_ID;

    public virtual System.Type Type => GetType();
}
