using UnityEditor;

namespace Core
{
	public class DefaultAssetPostprocessor : AssetPostprocessor
	{
		void OnPreprocessModel()
		{
			ModelImporter modelImporter = (ModelImporter)assetImporter;
			modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
#if UNITY_2020_1_OR_NEWER
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
#else
			modelImporter.importMaterials = false;          
#endif
			modelImporter.importTangents = ModelImporterTangents.CalculateMikk;
			if (modelImporter.meshCompression == ModelImporterMeshCompression.Off && !modelImporter.assetPath.Contains("Uncompressed"))
			{
				modelImporter.meshCompression = ModelImporterMeshCompression.Low;
			}
		}
	}
}

