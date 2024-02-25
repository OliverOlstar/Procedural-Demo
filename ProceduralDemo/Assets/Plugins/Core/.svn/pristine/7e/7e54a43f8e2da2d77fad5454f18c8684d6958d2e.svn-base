
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DataSourceBase : DBSO
	{
		public abstract System.Type DataStructType { get; }
		public abstract object RawData { get; }
		public abstract int RawDataCount { get; }
		public abstract void AddDependencies(List<System.Type> dependencies);
		public abstract void GenerateData(out string error);

		public bool ImportData(HashSet<string> dirtyData, ref string message)
		{
			List<System.Type> dependences = Core.ListPool<System.Type>.Request();
			AddDependencies(dependences);
			HashSet<System.Type> dependenciesSet = Core.HashPool<System.Type>.Request();
			foreach (System.Type dependency in dependences)
			{
				if (!dependenciesSet.Contains(dependency))
				{
					dependenciesSet.Add(dependency);
				}
			}
			Core.ListPool<System.Type>.Return(dependences);

			string typeName = GetType().Name;
			bool import = false;
			if (dirtyData.Contains(typeName)) // If we're marked dirty we definitely need to import
			{
				Core.Str.AddNewLine(typeName);
				import = true;
			}
			else
			{
				// Otherwise check dependenices to see if any of them are dirty
				foreach (System.Type dependency in dependenciesSet)
				{
					string dependencyName = dependency.Name;
					if (dirtyData.Contains(dependencyName))
					{
						if (!import)
						{
							Core.Str.Add(dependencyName);
						}
						else
						{
							Core.Str.Add($", {dependencyName}");
						}
						import = true;
					}
				}
			}
			if (!import)
			{
				Core.HashPool<System.Type>.Return(dependenciesSet);
				return false;
			}

			message += $"Generating {typeName} because dependencies were dirty: [{Core.Str.Finish()}]\n";

			DBManager.EditorClearDBsAccessed();
			GenerateData(out string error);
			if (!string.IsNullOrEmpty(error)) // If there was an error then the correct DB's might not have been accessed
			{
				DataValidationUtil.RaiseCode(GetType(), error);
			}
			else
			{
				HashSet<System.Type> accessed = DBManager.EditorDBsAccessed;
				foreach (System.Type access in accessed)
				{
					if (!dependenciesSet.Contains(access))
					{
						DataValidationUtil.RaiseCode(GetType(), $"Accessed {access.Name} during data generation which is not listed as a dependency. Make sure AddDependencies() is filled out properly.");
					}
				}
				if (RawDataCount > 0) // If we didn't import anything than skip this check as we probably imported an empty sheet so none of the import code will have run and pinged other DBs
				{
					foreach (System.Type dependency in dependenciesSet)
					{
						if (!accessed.Contains(dependency))
						{
							DataValidationUtil.RaiseCode(GetType(), $"{dependency.Name} is marked as a dependency but it wasn't accessed during import. Uneeded depedencies will increase import times. Make sure AddDependencies() is filled out properly.");
						}
					}
				}
			}
			Core.HashPool<System.Type>.Return(dependenciesSet);
			return true;
		}
	}

	public abstract class DataSource<TDataStruct, TData> : DataSourceBase
		where TDataStruct : IReadOnlyCollection<TData>
		where TData : IDataDictItem
	{
#if UNITY_2019_1_OR_NEWER
		[SerializeReference] // Used to serialize generic data without using nested scriptable objects, was introduced in 2019
#else
		[SerializeField]
#endif
		private TDataStruct m_RawData = default;
		sealed public override object RawData => m_RawData;
		sealed public override int RawDataCount => m_RawData.Count;
		sealed public override System.Type DataStructType => typeof(TDataStruct);

		sealed public override void GenerateData(out string error)
		{
			//float ts = Time.realtimeSinceStartup;
			m_RawData = OnGenerateData(out error);
			//Debug.LogError($"Generate Data {name} {Time.realtimeSinceStartup - ts}");
		}

		// Making this virtual makes it easier to remove with #if UNITY_EDITOR in child classes
		protected virtual TDataStruct OnGenerateData(out string error)
		{
			error = null;
			TDataStruct data = default;
			return data;
		}
	}
}
