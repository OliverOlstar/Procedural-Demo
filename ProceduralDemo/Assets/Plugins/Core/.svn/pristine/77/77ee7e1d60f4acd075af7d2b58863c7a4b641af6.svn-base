using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Data
{
	[Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public class ColumnDocument
	{
		public static ColumnDocument Create(string name, Type type, string validation, string notes)
		{
			return new ColumnDocument()
			{
				m_ColumnName = name,
				m_ColumnType = type,
				m_Validation = validation,
				m_SheetNotes = notes,
			};
		}

		[SerializeField, JsonProperty("name")]
		private string m_ColumnName = string.Empty;
		[SerializeField]
		private SerializedType m_ColumnType = null;
		[JsonProperty("type")]
		private string JsonColumnType => m_ColumnType.Value?.Name ?? "(none)";
		[SerializeField, JsonProperty("validation")]
		private string m_Validation = string.Empty;
		[SerializeField, TextArea(3, 10), JsonProperty("sheet_notes")]
		private string m_SheetNotes = string.Empty;
		[SerializeField, TextArea(3, 10), JsonProperty("extra_notes")]
		private string m_ExtraNotes = string.Empty;

		public string ColumnName => m_ColumnName;
		public Type ColumnType => m_ColumnType;
		public string Validation => m_Validation;
		public string SheetNotes => m_SheetNotes;
		public string ExtraNotes => m_ExtraNotes;

		private ColumnDocument()
		{
		}

		public void SetExtraNotes(string extraNotes)
		{
			m_ExtraNotes = extraNotes;
		}
	}
}
