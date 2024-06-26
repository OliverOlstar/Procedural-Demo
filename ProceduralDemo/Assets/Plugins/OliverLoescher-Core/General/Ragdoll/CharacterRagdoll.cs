using UnityEngine;

public class CharacterRagdoll : MonoBehaviour
{
	public Transform[] m_Transforms = null;

	private void Reset()
	{
		m_Transforms = transform.GetComponentsInChildren<Transform>();
	}

	public void MatchPosition(CharacterRagdoll toRagdoll)
	{
		if (toRagdoll.m_Transforms.Length != m_Transforms.Length)
		{
			Debug.LogError($"[{nameof(CharacterRagdoll)}] SwitchTo() was passed a {nameof(CharacterRagdoll)} that can not be matched");
			return;
		}
		for (int i = 0; i < m_Transforms.Length; i++)
		{
			m_Transforms[i].transform.SetPositionAndRotation(toRagdoll.m_Transforms[i].transform.position, toRagdoll.m_Transforms[i].transform.rotation);
			//transforms[i].transform.localScale = toRagdoll.transforms[i].transform.localScale;
		}
	}
}
