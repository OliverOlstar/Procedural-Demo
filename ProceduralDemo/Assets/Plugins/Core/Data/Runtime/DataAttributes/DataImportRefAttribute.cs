namespace Data.Import.Reference
{
	public class AssetAttribute : ReferenceAttribute
	{
		public AssetAttribute(bool allowBlanks = false) : base(allowBlanks) { }

		protected override UnityEngine.Object ImportReference(string columnName, System.Type columnType, string assetName, ref string importError)
		{
			if (!columnType.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				importError = $"Cannot use this attribute as this columns type {columnType.Name} is not a subclass of UnityEngine.Object";
				return null;
			}
#if UNITY_EDITOR
			UnityEngine.Object asset = Core.AssetDatabaseUtil.Load(assetName, columnType);
			if (asset != null)
			{
				return asset;
			}
#endif
			importError = "Asset named '" + assetName + "' of type " + columnType.Name + " doesn't exist in the Unity project";
			return null;
		}
	}
}
