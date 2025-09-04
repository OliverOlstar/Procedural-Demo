
namespace ODev.Data
{
    public class DataBin : IDataDictItem
    {
        protected string m_ID = string.Empty;
        // Often data with a custom server json format will require members to opt into serialization
        public string ID => m_ID;

        public virtual System.Type Type => GetType();
    }
}