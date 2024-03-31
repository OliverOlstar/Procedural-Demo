using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RootMotion.FinalIK;

[CustomEditor(typeof(CCDIKPositionHandle))]
public class CCDIKPositionHandleEditor : Editor
{
	private CCDIK m_TargetIK = null;
	private bool m_KeepUpdating = false;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (!TryInitalize())
		{
			return;
		}
		Vector3 position = m_TargetIK.solver.GetIKPosition();
		position = EditorGUILayout.Vector3Field("IKPosition", position);
		SetIKPosition(position);

		m_KeepUpdating = EditorGUILayout.Toggle("Keep Updating", m_KeepUpdating);
	}

	private void SetIKPosition(Vector3 pPosition)
	{
		if (pPosition != m_TargetIK.solver.GetIKPosition() || m_KeepUpdating)
		{
			m_TargetIK.solver.SetIKPosition(pPosition);
			m_TargetIK.UpdateSolverExternal();
		}
	}

	public void OnSceneGUI()
	{
		if (!TryInitalize())
		{
			return;
		}
		Vector3 position = m_TargetIK.solver.GetIKPosition();
		position = Handles.PositionHandle(position, Quaternion.identity);
		SetIKPosition(position);
	}

	private bool TryInitalize()
	{
		if (!Application.isPlaying)
		{
			return false;
		}
		if (m_TargetIK != null)
		{
			return true;	
		}
		if (target is not MonoBehaviour mono)
		{
			return false;
		}
		return mono.TryGetComponent(out m_TargetIK);
	}
}