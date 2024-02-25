using Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Data
{
	public class ImportData
	{
		public delegate void ExternalVariantSelectionAction(IReadOnlyDictionary<string, DataVariant> existingVariants, HashSet<DataVariant> importVariants);

		public static void ImportForEditor(params string[] ignoreSheetNames)
		{
			if (Application.isPlaying)
			{
				Application.Quit();
			}
			ImportInternal(null, ignoreSheetNames);
			DataValidationUtil.FinalizeReport();
			DataValidationUtil.ShowResultPopup();
		}

		public static void ImportForBuild(out string errors, ExternalVariantSelectionAction externalVariantSelection = null)
		{
			DebugOptions.IncludeVariants.SetString(string.Empty); // Build should always import all variants
			DebugOptions.IncludeVariants.Set();
			TypeUtility.DebugLogAllTypes();
			ImportInternal(externalVariantSelection);
			DataValidationUtil.FinalizeReport();
			if (DataValidationUtil.TryGetReport(out _, out errors))
			{
				LogError(errors);
			}
		}

		public static void ImportInternal(ExternalVariantSelectionAction externalVariantSelection = null, params string[] ignoreSheetNames)
		{
			s_Logger.OnStart("DataImport");
			DataValidationUtil.Flush();
			if (!StaticDataConfig.TryGet(out StaticDataConfig dataConfig))
			{
				DataValidationUtil.Raise("Unable to import data please create a data config file");
				return;
			}
			int count = 0;
			HashSet<DataVariant> importVariants = new HashSet<DataVariant> { DataVariant.Base };
			try
			{
				//////////////////////////////////////////////
				// DETECT VARIANT FOLDERS AND GENERATE TREE
				//////////////////////////////////////////////
				Dictionary<string, DataVariant> dataVariants = new Dictionary<string, DataVariant>();
				if (DebugOptions.IncludeVariants.IsSet())
				{
					if (DebugOptions.IncludeVariants.IsStringSet(out string variant))
					{
						dataVariants = dataConfig.BuildVariantsDictionary(variant);
						if (dataVariants.Count > 0 || variant == DataVariant.Base.Name)
						{
							Log($"Found specific {variant} with {dataVariants.Count} variants");
						}
						else
						{
							DataValidationUtil.Raise($"Could not find {variant} specified in {nameof(DebugOptions)}.{nameof(DebugOptions.IncludeVariants)}.\n" +
								$"Ensure {variant} is spelled correctly or a variant folder named {variant} exist somewhere in source directory.\n" +
								$"If wanting to import all variants, ensure {nameof(DebugOptions)}.{nameof(DebugOptions.IncludeVariants)}' string is left empty.\n");
							return; // Exit out so user has to fix error
						}
					}
					else
					{
						dataVariants = dataConfig.BuildVariantsDictionary();
						Log($"Found {dataVariants.Count} variants");
					}
				}

				//////////////////////////////////////////////
				// DETECT VALID VARIANTS AND LOAD EXCEL TABLES
				//////////////////////////////////////////////
				Dictionary<DataVariant, List<DataTable>> variantTables = new Dictionary<DataVariant, List<DataTable>>();
				if (DebugOptions.IncludeVariants.IsSet())
				{
					DebugOptions.IncludeVariants.IsStringSet(out string specificVariant);
					foreach (DataVariant variant in dataVariants.Values)
					{
						string variantPath = dataConfig.GetDataVariantPath(variant, dataVariants);
						List<DataTable> dataTables = ImportExcel.ParseDirectoryToDataTables(variantPath);
						if (dataTables.Count == 0)
						{
							continue;
						}
						variantTables.Add(variant, dataTables);
						if (string.IsNullOrEmpty(specificVariant) || variant.Name == specificVariant)
						{
							importVariants.Add(variant);
						}
					}
					Log($"Load Excel tables for {variantTables.Count} variants");
				}

				//////////////////////////////////////////////
				// EXTERNAL VARIANT SELECTION
				//////////////////////////////////////////////
				if (externalVariantSelection != null)
				{
					Log($"External Variant Selection Before - {importVariants.Count} variants to import");
					externalVariantSelection?.Invoke(dataVariants, importVariants); // Allow external source to modify 'importVariants'
					Log($"External Variant Selection After - {importVariants.Count} variants remaining to import");
				}

				count = importVariants.Count;
				if (!importVariants.Contains(DataVariant.Base))
				{
					count++; // We will still be importing Base even if it was removed, it just won't be added to the __directory.json
				}
				if (count == 0)
				{
					s_Logger.OnComplete($"Imported {count} data variants");
					return;
				}

				//////////////////////////////////////////////
				// LOAD BASE EXCEL TABLES
				//////////////////////////////////////////////
				string sourcePath = dataConfig.GetRootPath(StaticDataConfig.RootFolder.Source);
				string progressTitle = $"Parse Excel";
				List<DataTable> sourceTables = ImportExcel.ParseDirectoryToDataTables(sourcePath, progressTitle);
				Log($"Load base Excel tables");

				//////////////////////////////////////////////
				// DETECT INVALID VARIANTS AND DELETE THEM
				//////////////////////////////////////////////
				string clientVariantPath = dataConfig.GetVariantsPath(StaticDataConfig.RootFolder.Client);
				TryCleanVariantDirectory(clientVariantPath, dataVariants, importVariants);
				string serverVariantPath = dataConfig.GetVariantsPath(StaticDataConfig.RootFolder.Server);
				TryCleanVariantDirectory(serverVariantPath, dataVariants, importVariants);
				Log($"Removed no longer existant variant client & server data");

				//////////////////////////////////////////////
				// IMPORT BASE
				//////////////////////////////////////////////
				int num = 1;
				progressTitle = $"{DataVariant.Base}({num}/{count}) - ";
				ImportVariant(dataConfig, DataVariant.Base, dataVariants, variantTables, sourceTables, progressTitle, ignoreSheetNames);
				num++;

				//////////////////////////////////////////////
				// PREP VARIANT ASSETS
				//////////////////////////////////////////////
				// Importing data that doesn't exist is very expensive as we need to create and import ~150 assets,
				// instead when a variant doesn't exist copy base to create that variant
				// We also want to delete variants that aren't being used otherwise their assets can get stale and the
				// next time we try to import that variant the data can become unstable
				try
				{
					AssetDatabase.StartAssetEditing();
					string basePath = dataConfig.GetPath(StaticDataConfig.RootFolder.Client, DataVariant.Base);
					float i = 1;
					int totalVariants = dataVariants.Count;
					foreach (DataVariant variant in dataVariants.Values)
					{
						float progress = i / totalVariants;
						i++;

						string variantPath = dataConfig.GetPath(StaticDataConfig.RootFolder.Client, variant);
						bool variantExists = AssetDatabase.IsValidFolder(variantPath);
						bool variantIsValid = importVariants.Contains(variant);
						if (variantIsValid && !variantExists)
						{
							DisplayProgressBar("Prep Variant Assets", $"{variant} copy from {DataVariant.Base}", progress);
							AssetDatabase.CopyAsset(basePath, variantPath);
						}
						else if (!variantIsValid && variantExists)
						{
							DisplayProgressBar("Prep Variant Assets", $"{variant} deleting invalid variant data", progress);
							AssetDatabase.DeleteAsset(variantPath);
						}
					}
				}
				finally
				{
					EditorUtility.ClearProgressBar();
					AssetDatabase.StopAssetEditing(); // Must always match AssetDatabase.StartAssetEditing() or things might be horribly broken
					AssetDatabase.Refresh();
					Log($"Preped Variant Assets");
				}

				//////////////////////////////////////////////
				// IMPORT VARIANTS
				//////////////////////////////////////////////
				foreach (DataVariant variant in dataVariants.Values)
				{
					if (importVariants.Contains(variant))
					{
						progressTitle = $"{variant}({num}/{count}) - ";
						ImportVariant(dataConfig, variant, dataVariants, variantTables, sourceTables, progressTitle, ignoreSheetNames);
						num++;
					}
				}

				//////////////////////////////////////////////
				// SET ASSETBUNDLE TAGS
				//////////////////////////////////////////////
				SetAssetBundleTag(DataVariant.Base, dataConfig, importVariants);
				foreach (DataVariant variant in dataVariants.Values)
				{
					SetAssetBundleTag(variant, dataConfig, importVariants);
				}

				//////////////////////////////////////////////
				// BUILD __directory.json
				//////////////////////////////////////////////
				// Build this json so the server knows which variants exist
				// We shouldn't update __directory if we're not importing variants, but if this json doesn't exist we better create it
				// IMPORTANT: Server uses first variant in json as base
				string serverPath = dataConfig.GetRootPath(StaticDataConfig.RootFolder.Server);
				GenerateDirectoryJson(serverPath, importVariants, out string output);
				Log($"Update {output}");
				
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
			}
			catch (System.Exception e)
			{
				EditorUtility.ClearProgressBar();
				DataValidationUtil.Raise(e.ToString());
			}
			DBManager.SetVariantAndClear(DataVariant.Base.Name);
			s_Logger.OnComplete($"Imported {count} data variants");
			s_ForceValidateAndRegenerate = false;
		}

		private static void SetAssetBundleTag(DataVariant variant, StaticDataConfig dataConfig, HashSet<DataVariant> importVariants)
		{
			string clientPath = dataConfig.GetPath(StaticDataConfig.RootFolder.Client, variant);
			if (Directory.Exists(clientPath))
			{
				// Decided if this variant should be included in bundles
				string bundleName = string.Empty;
				if (variant == DataVariant.Base || // Always tag the Base
					(importVariants.Contains(variant) && Data.DebugOptions.IncludeVariants.IsSet()))
				{
					bundleName = StaticDataConfig.GetVariantAssetBundleName(variant);
				}

				// Stomp asset bundle tag on the folder, we just need to tag the master
				AssetImporter.GetAtPath(clientPath).SetAssetBundleNameAndVariant("", "");

				// Tag master asset
				string masterPath = dataConfig.GetMasterAssetPath(variant);
				AssetImporter masterImporter = AssetImporter.GetAtPath(masterPath);
				if (masterImporter != null)
				{
					masterImporter.SetAssetBundleNameAndVariant(bundleName, "");
				}
			}
		}

		public static void GenerateDirectoryJson(string directory, HashSet<DataVariant> importVariants, out string output)
		{
			output = string.Empty;
			string variantsJsonPath = $"{directory}/__directory.json";
			if (Data.DebugOptions.IncludeVariants.IsSet() || !File.Exists(variantsJsonPath))
			{
				List<string> variantStrings = new List<string>();
				foreach (DataVariant v in importVariants)
				{
					variantStrings.Add(StaticDataConfig.GetShortVariantsPath(v));
				}
				string variantsJson = JsonConvert.SerializeObject(variantStrings, Formatting.Indented);
				File.WriteAllText(variantsJsonPath, variantsJson);
				output = $"{variantsJsonPath}\n{variantsJson}";
			}
		}

		public static bool IsDerivedFrom(System.Type type, System.Type baseType)
		{
			return !type.IsAbstract && !type.IsGenericType && type.IsSubclassOf(baseType);
		}

		private static bool s_ForceValidateAndRegenerate = false;
		public static void SetForceValidateAndRegenerateNextImport() => s_ForceValidateAndRegenerate = true;
		private static DBBase s_Current = null;
		private static HashSet<System.Type> s_Ignore = new HashSet<System.Type>();

		private static void OnDBAccess(System.Type type)
		{
			if (s_Ignore.Contains(type))
			{
				DataValidationUtil.Raise(s_Current.GetType().Name, $"{s_Current.GetType().Name} is dependent on {type.Name} " +
					$"but it is not registered as a dependency\n{StackTraceUtility.ExtractStackTrace()}");
			}
		}

		private static void ImportVariant(
			StaticDataConfig dataConfig,
			DataVariant variant,
			Dictionary<string, DataVariant> dataVariants,
			Dictionary<DataVariant, List<DataTable>> variantTables,
			List<DataTable> sourceTables,
			string progressTitlePrefix,
			string[] ignoreSheetNames)
		{
			float firstTs = Time.realtimeSinceStartup;
			ImportVariantInternal(dataConfig, variant, dataVariants, variantTables, sourceTables, progressTitlePrefix, ignoreSheetNames);
			// Wrapper is to guarenteed these things happen if the Import early out's
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
			EditorUtility.ClearProgressBar();
			Log($"Save duration", variant);
			Log(progressTitlePrefix + $"Completed in {Time.realtimeSinceStartup - firstTs:0.0}s", variant);
		}

		private static void ApplyVariant(List<DataTable> variantTables, Dictionary<string, Raw.Sheet> sheets)
		{
			Dictionary<string, Raw.Sheet> variantSheets = ImportExcel.ConvertDataTablesToRawSheets(variantTables);
			foreach (KeyValuePair<string, Raw.Sheet> modifiedSheet in variantSheets)
			{
				if (sheets.TryGetValue(modifiedSheet.Key, out Raw.Sheet sheet))
				{
					sheet.ApplyVariant(modifiedSheet.Value);
				}
			}
		}

		private static void PreImportCleanUp(
			StaticDataConfig dataConfig,
			DataVariant variant,
			HashSet<System.Type> types,
			HashSet<System.Type> sourceTypes)
		{
			// Make sure destination folders exist
			string clientPath = dataConfig.GetPath(StaticDataConfig.RootFolder.Client, variant);
			Directory.CreateDirectory(clientPath);
			string serverPath = dataConfig.GetPath(StaticDataConfig.RootFolder.Server, variant);
			Directory.CreateDirectory(serverPath);

			// Delete unused assets, these can get corrupted over time and cause mysterious errors
			HashSet<string> typeNames = new HashSet<string>();
			foreach (System.Type type in types)
			{
				typeNames.Add(type.Name);
			}
			foreach (System.Type type in sourceTypes)
			{
				typeNames.Add(type.Name);
			}
			string master = StaticDataConfig.GetVariantDBMasterName(variant);
			typeNames.Add(master);
			foreach (string path in Directory.GetFiles(clientPath, "*.asset"))
			{
				string name = Path.GetFileNameWithoutExtension(path);
				if (!typeNames.Contains(name))
				{
					LogError("Delete unused asset " + path, variant);
					AssetDatabase.DeleteAsset(path);
				}
			}

			// Delete unused jsons
			HashSet<string> jsonNames = new HashSet<string>(dataConfig.ServerDataSheetNames);
			foreach (string path in Directory.GetFiles(serverPath, "*.json"))
			{
				string name = Path.GetFileNameWithoutExtension(path);
				if (!jsonNames.Contains(name))
				{
					LogError($"Delete unused json {path}", variant);
					AssetDatabase.DeleteAsset(path);
				}
			}
		}

		private static void CalculateDirty(
			DBMasterSO masterSO,
			Dictionary<System.Type, DBBase> dbList,
			DBHashes hashes,
			HashSet<string> dirty)
		{
			if (s_ForceValidateAndRegenerate)
			{
				foreach (DBBase db in dbList.Values)
				{
					dirty.Add(db.GetType().Name);
				}
				return;
			}
			Dictionary<string, DBHash> masterHashes = Core.DictionaryPool<string, DBHash>.Request();
			foreach (DBHash masterHash in masterSO.Hashes)
			{
				if (!hashes.ContainsKey(masterHash.TypeName))
				{
					dirty.Add(masterHash.TypeName);
					Core.Str.AddNewLine(masterHash.TypeName, " - *removed*");
					continue;
				}
				masterHashes.Add(masterHash.TypeName, masterHash);
			}
			foreach (DBHash pendingHash in hashes.Values)
			{
				if (!masterHashes.TryGetValue(pendingHash.TypeName, out DBHash masterHash))
				{
					dirty.Add(pendingHash.TypeName);
					Core.Str.AddNewLine(pendingHash.TypeName, " - *new*");
					continue;
				}
				for (int i = 0; i < pendingHash.Hash.Length; i++)
				{
					if (pendingHash.Hash[i] != masterHash.Hash[i])
					{
						dirty.Add(pendingHash.TypeName);
						Core.Str.AddNewLine(pendingHash.TypeName, " - ", Core.Util.BytesToString(pendingHash.Hash), " != ", Core.Util.BytesToString(masterHash.Hash));
						break;
					}
				}
			}
			Core.DictionaryPool<string, DBHash>.Return(masterHashes);
		}

		private static void ImportVariantInternal(
			StaticDataConfig dataConfig,
			DataVariant variant,
			Dictionary<string, DataVariant> dataVariants,
			Dictionary<DataVariant, List<DataTable>> variantTables,
			List<DataTable> sourceTables,
			string progressTitlePrefix,
			string[] ignoreSheetNames)
		{
			DataValidationUtil.SetVariant(variant.Name);

			//////////////////////////////////////////////
			// PROCESS EXCEL FILES
			//////////////////////////////////////////////
			Log($"{progressTitlePrefix}Start", variant);
			DisplayProgressBar($"{progressTitlePrefix}Prepare Excel Tables", string.Empty, 0.0f);
			// Process excel data to prepare for import
			Dictionary<string, Raw.Sheet> sheets = ImportExcel.ConvertDataTablesToRawSheets(sourceTables);
			Log($"Prepare Excel tables", variant);
			if (variant != DataVariant.Base) // Base variant is always pure
			{
				DisplayProgressBar($"{progressTitlePrefix}Apply Variant Delta", string.Empty, 0.0f);
				Stack<DataVariant> variantStack = new Stack<DataVariant>();
				DataVariant currVariant = variant;
				variantStack.Push(currVariant);
				while (currVariant.Parent != DataVariant.Base.Name) // Iterate up variant tree
				{
					if (!dataVariants.TryGetValue(currVariant.Parent, out DataVariant nextVariant))
					{
						LogError($"Data variant's {currVariant} parent {currVariant.Parent} does not exist.", variant);
						break;
					}
					currVariant = nextVariant;
					variantStack.Push(currVariant);
				}

				while (variantStack.Count > 0) // Apply variant parent first
				{
					DataVariant v = variantStack.Pop();
					if (variantTables.TryGetValue(v, out List<DataTable> table))
					{
						DataValidationUtil.SetVariant(v.Name); // Ensure error go to variant that is currently being applied
						ApplyVariant(table, sheets);
					}
				}
				DataValidationUtil.SetVariant(variant.Name); // Restore valiation variant
				Log($"Apply variant delta", variant);
			}
			// Don't bother trying to import invalid excel data
			if (DataValidationUtil.Failed())
			{
				return;
			}

			//////////////////////////////////////////////
			// SETUP
			//////////////////////////////////////////////
			// Set to current variant and also, unload all data so DB classes will be sure to grab the latest after import
			DBManager.SetVariantAndClear(variant.Name);

			s_Ignore.Clear();
			HashSet<string> ignoreSet = new HashSet<string>(ignoreSheetNames);

			HashSet<System.Type> types = new HashSet<System.Type>(Core.TypeUtility.GetMatchingTypes(typeof(DBBase), IsDerivedFrom));
			HashSet<System.Type> sourceTypes = new HashSet<System.Type>(Core.TypeUtility.GetMatchingTypes(typeof(DataSourceBase), IsDerivedFrom));

			DisplayProgressBar($"{progressTitlePrefix}Apply Variant Delta", string.Empty, 0.0f);
			PreImportCleanUp(dataConfig, variant, types, sourceTypes);
			Log($"Pre import cleanup", variant);

			//////////////////////////////////////////////
			// FIRST PASS IMPORT
			//////////////////////////////////////////////
			Dictionary<System.Type, DBBase> dbList = new Dictionary<System.Type, DBBase>(types.Count);
			int progressTotal = types.Count * 3 + sourceTypes.Count + dataConfig.ServerDataSheetNames.Length;
			float progressCount = 0.0f;
			foreach (System.Type type in types)
			{
				progressCount++;
				DisplayProgressBar($"{progressTitlePrefix}Import Client Data", type.Name, progressCount / progressTotal);
				string path = dataConfig.GetClientAssetPath(variant, type);
				DBBase so = GetOrCreateDataSO<DBBase>(path, type, out _);
				if (ignoreSet.Contains(so.SheetName))
				{
					s_Ignore.Add(so.GetType());
					continue;
				}
				Raw.Sheet sheet = Raw.Sheet.EMPTY;
				if (so.ImportFromSheet && !sheets.TryGetValue(so.SheetName, out sheet))
				{
					DataValidationUtil.Raise(so.SheetName, $"Could not find sheet named {so.SheetName} for {type}");
					continue;
				}
				so.EditorImportData(sheet, out string importError);
				EditorUtility.SetDirty(so); // We're applying changes through SerializedObjects so need to mark asset to be saved
				if (!string.IsNullOrEmpty(importError))
				{
					DataValidationUtil.Raise(so.SheetName, importError);
					continue;
				}
				dbList.Add(type, so);
			}

			string masterPath = dataConfig.GetMasterAssetPath(variant);
			DBMasterSO masterSO = GetOrCreateDataSO<DBMasterSO>(masterPath, out _);

			// Try to create DataSources now because if one doesn't exist we want it to count as dirty so we don't early out bellow
			// We never want to finish a data import with a data source not present
			List<DataSourceBase> sourceList = new List<DataSourceBase>(sourceTypes.Count);
			HashSet<string> dirty = new HashSet<string>();
			foreach (System.Type type in sourceTypes)
			{
				string path = dataConfig.GetClientAssetPath(variant, type);
				DataSourceBase so = GetOrCreateDataSO<DataSourceBase>(path, type, out bool createdSource);
				sourceList.Add(so);
				if (createdSource)
				{
					string name = so.GetType().Name;
					dirty.Add(name);
					Core.Str.AddNewLine(name);
				}
			}
			Log($"First pass, created {dirty.Count} Data Sources:{Core.Str.Finish()}", variant);

			//////////////////////////////////////////////
			// POST PROCESS
			//////////////////////////////////////////////
			DBManager.RegisterOnEditorAccessAction(OnDBAccess);
			foreach (KeyValuePair<Type, DBBase> pair in dbList)
			{
				DBBase db = pair.Value;
				progressCount++;
				DisplayProgressBar($"{progressTitlePrefix}Post Process Client Data", pair.Key.Name, progressCount / progressTotal);
				db.PostProcess(db.ImportFromSheet ? sheets[db.SheetName] : Raw.Sheet.EMPTY);
				// During post processing DB's can start talking to eachother to fill out their data,
				// we should clear db's from the manager after post processing as they may have been altered.
				// This will force a fresh copy to be loaded next time the db is accessed through the manager
				DBManager.Clear(pair.Key);
			}
			Log($"Post process", variant);

			//////////////////////////////////////////////
			// CHECK DIRTY
			//////////////////////////////////////////////
			// Save assets to disk and so we can hash them and check which DBs are dirty
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
			DBHashes hashes = DBHashes.Create(dbList.Values);
			int dirtyCount = dirty.Count;
			CalculateDirty(masterSO, dbList, hashes, dirty);
			bool nonGeneratedDataDirty = dirty.Count > dirtyCount;
			Log($"Found {dirty.Count} dirty DBs and Data Sources{Core.Str.Finish()}", variant);
			// Attempt to early out if no data is dirty, regenerating data and server jsons wont do anything if nothing is dirty
			if (dirty.Count == 0)
			{
				Log($"Found no dirty DBs, early out", variant);
				return;
			}

			//////////////////////////////////////////////
			// VALIDATE
			//////////////////////////////////////////////
			if (nonGeneratedDataDirty) // Only need to run validation pass if some non generated data is dirty
			{
				int validatedCount = 0;
				DBBase db;
				string message = string.Empty;
				List<Type> dbDependancies = ListPool<Type>.Request();
				foreach (KeyValuePair<Type, DBBase> pair in dbList)
				{
					db = pair.Value;
					s_Current = db;
					progressCount++;
					DisplayProgressBar($"{progressTitlePrefix}Validate Client Data", pair.Key.Name, progressCount / progressTotal);

					dbDependancies.Clear();
					db.AddValidationDependencies(dbDependancies);
					if (!IsDirtyOrDepenancies(dirty, db.GetType(), dbDependancies, out string triggerMessage))
					{
						continue;
					}
					message += $"Validate {db.name} because {triggerMessage}\n";

					// Validate
					validatedCount++;
					DBManager.EditorClearDBsAccessed();
					db.Validate(db.ImportFromSheet ? sheets[db.SheetName] : Raw.Sheet.EMPTY);
					if (typeof(IDataValidate).IsAssignableFrom(pair.Key) &&
						DBManager.TryGet(pair.Key, out DBBase instance) &&
						instance is IDataValidate validateInstance)
					{
						string error = validateInstance.OnDataValidate();
						if (!string.IsNullOrEmpty(error))
						{
							DataValidationUtil.Raise(db.SheetName, error);
						}
					}

					if (DataValidationUtil.HasErrors(db.SheetName)) // If there was an error then the correct DB's might not have been accessed
					{
						continue;
					}

					if (db.EditorRawDataCount > 0) // If the sheet is empty then depenancies will fail
					{
						// Check for incorrect dependencies
						HashSet<Type> accessed = DBManager.EditorDBsAccessed;
						foreach (Type access in accessed)
						{
							if (access != db.GetType() && !dbDependancies.Contains(access))
							{
								DataValidationUtil.RaiseCode(db.GetType(), $"Accessed {access.Name} during data validation which is not listed as a dependency. Make sure AddValidationDependencies() list is filled out properly.");
							}
						}
						foreach (Type dependency in dbDependancies)
						{
							if (dependency == db.GetType())
							{
								DataValidationUtil.RaiseCode(db.GetType(), $"{dependency.Name} is marked as a dependency of itself, don't do this. Uneeded depedencies will increase import times. Make sure AddValidationDependencies() list is filled out properly.");
							}
							else if (!accessed.Contains(dependency))
							{
								DataValidationUtil.RaiseCode(db.GetType(), $"{dependency.Name} is marked as a dependency but it wasn't accessed during validation. Uneeded depedencies will increase import times. Make sure AddValidationDependencies() list is filled out properly.");
							}
						}
					}
				}
				ListPool<Type>.Return(dbDependancies);
				DBManager.DeregisterOnEditorAccessAction(OnDBAccess);
				Debug.Log($"[DataImport] [{variant}] Validated following {validatedCount} DBs:\n{message}"); // Extra log for extra info
				Log($"Validated {validatedCount} DBs", variant);
				// We should only try to generate client data and server data based on data that passed validation
				if (DataValidationUtil.Failed())
				{
					return;
				}
			}

			//////////////////////////////////////////////
			// GENERATE
			//////////////////////////////////////////////
			string generateMessage = string.Empty;
			foreach (DataSourceBase so in sourceList)
			{
				progressCount++;
				DisplayProgressBar(progressTitlePrefix + "Generating Data", so.GetType().Name, progressCount / progressTotal);
				if (so.ImportData(dirty, ref generateMessage))
				{
					EditorUtility.SetDirty(so); // We're applying changes through SerializedObjects so need to mark asset to be saved
				}
			}
			Debug.Log($"[DataImport] [{variant}] Generated following DBs:\n{generateMessage}");
			Log($"Generate", variant);
			// Unload all data so DB classes will be sure to grab the latest after import
			DBManager.Clear();

			//////////////////////////////////////////////
			// SERVER JSONS
			//////////////////////////////////////////////
			DisplayProgressBar($"{progressTitlePrefix}Importing server data", "Converting...", progressCount / progressTotal);
			Dictionary<string, System.Type> overrideServerJsonDBs = DictionaryPool<string, System.Type>.Request();
			foreach (DBBase db in dbList.Values)
			{
				if (db is IOverrideExcelToJsonSerialization)
				{
					overrideServerJsonDBs.Add(db.SheetName, db.GetType());
				}
			}
			for (int i = 0; i < dataConfig.ServerDataSheetNames.Length; i++)
			{
				string serverSheetName = dataConfig.ServerDataSheetNames[i];
				progressCount++;
				DisplayProgressBar($"{progressTitlePrefix}Create Server JSONs", serverSheetName, progressCount / progressTotal);
				string json = null;
				if (overrideServerJsonDBs.TryGetValue(serverSheetName, out System.Type serverDBType))
				{
					if (DBManager.TryGet(serverDBType, out DBBase db) && db is IOverrideExcelToJsonSerialization overrideServerJsonDB)
					{
						json = overrideServerJsonDB.OnSerializeServerJson();
					}
					else
					{
						DataValidationUtil.Raise(serverSheetName, $"DBManager does not contain a DB of type {serverDBType} which is expected by the server");
					}
				}
				else
				{
					if (!sheets.TryGetValue(serverSheetName, out Raw.Sheet sheet))
					{
						DataValidationUtil.Raise(serverSheetName, "Unable to find sheet which is expected by the server");
						continue;
					}
					json = JsonConvert.SerializeObject(sheet.Rows.Values, Formatting.Indented);
				}
				if (Application.platform == RuntimePlatform.OSXEditor)
				{
					json = json.Replace("\n", "\r\n"); // Try to fix up osx line endings
				}
				string jsonPath = dataConfig.GetServerAssetPath(variant, serverSheetName);
				File.WriteAllText(jsonPath, json);
			}
			Log($"Server", variant);

			//////////////////////////////////////////////
			// MASTER DB
			//////////////////////////////////////////////
			// Don't write hashes into Master object until we get a successful import,
			// we want to make sure the DB's stay dirty so we keep running the validation pass next import
			if (DataValidationUtil.Failed())
			{
				return;
			}
			// Populate master data SOs.
			masterSO._EditorImport(dbList.Values, sourceList, hashes.Values);
			EditorUtility.SetDirty(masterSO); // We're applying changes through SerializedObjects so need to mark asset to be saved
			Log($"Master", variant);
		}

		private static void TryCleanVariantDirectory(in string directory, in Dictionary<string, DataVariant> dataVariants, in HashSet<DataVariant> importVariants)
		{
			if (!Directory.Exists(directory))
			{
				return;
			}
			if (s_ForceValidateAndRegenerate)
			{
				AssetDatabase.DeleteAsset(directory); // Force import, just clear all variant data
				return;
			}
			foreach (string variantDirectory in Directory.GetDirectories(directory)) // Client
			{
				string[] path = variantDirectory.Split('\\', '/');
				if (!dataVariants.TryGetValue(path[path.Length - 1], out DataVariant dataVariant) || !importVariants.Contains(dataVariant))
				{
					AssetDatabase.DeleteAsset(variantDirectory);
				}
			}
		}

		private static bool IsDirtyOrDepenancies(in HashSet<string> dirty, in Type db, in List<Type> depenancies, out string triggerMessage)
		{
			if (dirty.Contains(db.Name))
			{
				triggerMessage = "self is diry";
				return true;
			}
			foreach (Type dependancy in depenancies)
			{
				if (dirty.Contains(dependancy.Name))
				{
					triggerMessage = $"{dependancy.Name} is a dirty dependancy";
					return true;
				}
			}
			triggerMessage = null;
			return false;
		}

		private static void DisplayProgressBar(string title, string info, float progress)
		{
			if (EditorUtility.DisplayCancelableProgressBar(title, info, progress) && !DataValidationUtil.Failed())
			{
				DataValidationUtil.Raise("User canceled import. Importing again is required."); // Raise error so we can safely early out and next avaliable time
			}
		}

		private static T GetOrCreateDataSO<T>(string path, out bool createdNew) where T : ScriptableObject
			=> GetOrCreateDataSO<T>(path, typeof(T), out createdNew);

		private static T GetOrCreateDataSO<T>(string path, Type dataType, out bool createdNew) where T : ScriptableObject
		{
			createdNew = false;
			T so = (T)AssetDatabase.LoadAssetAtPath(path, dataType);
			if (so == null)
			{
				createdNew = true;
				so = (T)ScriptableObject.CreateInstance(dataType);
				AssetDatabase.CreateAsset(so, path);
			}
			return so;
		}

		private static TimedStepLoggerEditor s_Logger = new TimedStepLoggerEditor();

		public static void Log(string s, DataVariant variant = null)
		{
			if (variant != null)
			{
				s_Logger.Log($"[{variant}] {s}");
				//Debug.Log($"[DataImport][{variant.Value}] {s}");
			}
			else
			{
				s_Logger.Log(s);
				//Debug.Log($"[DataImport] {s}");
			}
		}

		public static void LogError(string s, DataVariant variant = null)
		{
			if (variant != null)
			{
				s_Logger.LogError($"[{variant}] {s}");
				//Debug.LogError($"[DataImport][{variant.Value}] {s}");
			}
			else
			{
				s_Logger.LogError(s);
				//Debug.LogError($"[DataImport] {s}");
			}
		}
	}
}