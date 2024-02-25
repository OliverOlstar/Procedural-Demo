using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CameraAnimationSequence : ScriptableObject
{
	[System.Serializable]
	public struct CameraAnimationKeyframe
	{
		[SerializeField][Core.Frames]
		float m_FrameValue;
		public float GetFrameValue() { return m_FrameValue; }
		[SerializeField]
		int m_FOV;
		public int GetFOV() { return m_FOV; }
		[SerializeField]
		Vector3 m_Position;
		public Vector3 GetPosition() { return m_Position; }
		[SerializeField]
		Vector3 m_Rotation;
		public Quaternion GetRotation() {return Quaternion.Euler(m_Rotation); }
		public Vector3 GetVectorRotation() { return m_Rotation; }
	
		public CameraAnimationKeyframe(int FOV, Vector3 pos, Vector3 rot)
		{
			m_FrameValue = 0;
			m_FOV = FOV;
			m_Position = pos;
			m_Rotation = rot;
		}
	}
	[SerializeField]
	string m_BoneName = "";
	public string GetBoneName() { return m_BoneName; }
	[SerializeField]
	public List<CameraAnimationKeyframe> m_Keyframes;
	public float Duration
	{ get
		{
			return m_Keyframes[m_Keyframes.Count -1].GetFrameValue();
		}
	}

	public CameraAnimationKeyframe Interpolate(float t)
	{
		int fov = 0;
		Vector3 rotation = Vector3.zero;
		Vector3 position = Vector3.zero;

		float durPerc = Duration * t;

		for(int i = 0; i < m_Keyframes.Count; ++i)
		{
			if(i < m_Keyframes.Count -1 && durPerc >= m_Keyframes[i].GetFrameValue() && durPerc <= m_Keyframes[i+1].GetFrameValue())
			{
					float interpVal = (durPerc - m_Keyframes[i].GetFrameValue()) /(m_Keyframes[i+1].GetFrameValue() - m_Keyframes[i].GetFrameValue());
					fov = (int)Mathf.Lerp(m_Keyframes[i].GetFOV(), m_Keyframes[i+1].GetFOV(),interpVal);
					position = Vector3.Lerp(m_Keyframes[i].GetPosition(), m_Keyframes[i+1].GetPosition(), interpVal);
					rotation = Quaternion.Lerp(m_Keyframes[i].GetRotation(), m_Keyframes[i+1].GetRotation(), interpVal).eulerAngles;
					break;
			}
			else if(i == m_Keyframes.Count -1)
			{
				fov = m_Keyframes[i].GetFOV();
				position = m_Keyframes[i].GetPosition();
				rotation = m_Keyframes[i].GetVectorRotation();
			}
		}
		return new CameraAnimationKeyframe(fov, position, rotation);
	}
}

