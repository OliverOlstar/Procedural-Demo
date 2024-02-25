using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	[Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public class DatabaseDocument
	{
		public static DatabaseDocument Create(string name, Type type, IEnumerable<ColumnDocument> columns)
		{
			DatabaseDocument document = new DatabaseDocument()
			{
				m_DatabaseName = name,
				m_DatabaseType = type,
			};
			document.m_Columns.AddRange(columns);
			return document;
		}

		[SerializeField, JsonProperty("name")]
		private string m_DatabaseName = string.Empty;
		[SerializeField, DerivedTypeConstraint(typeof(DBSO))]
		private SerializedType m_DatabaseType = null;
		[SerializeField, JsonProperty("columns")]
		private List<ColumnDocument> m_Columns = null;

		public string DatabaseName => m_DatabaseName;
		public Type DatabaseType => m_DatabaseType;
		public IEnumerable<ColumnDocument> Columns => m_Columns;

		[JsonProperty("type")]
		private string JsonType => m_DatabaseType.Value?.Name ?? "(none)";

		private DatabaseDocument()
		{
			m_Columns = new List<ColumnDocument>();
		}

		public bool TryGetColumn(string columnName, out ColumnDocument column)
		{
			foreach (ColumnDocument col in m_Columns)
			{
				if (col.ColumnName == columnName)
				{
					column = col;
					return true;
				}
			}
			column = null;
			return false;
		}

		public void AddColumn(ColumnDocument column)
		{
			if (TryGetColumn(column.ColumnName, out _))
			{
				throw new InvalidOperationException($"Document {DatabaseName} already contains a Column with name '{column.ColumnName}'");
			}
			m_Columns.Add(column);
		}
	}
}
