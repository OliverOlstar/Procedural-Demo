using UnityEngine;

public class MatchRelPosition : MonoBehaviour
{
	public Transform Target = null;

	void LateUpdate()
	{
		if (Target != null)
		{
			transform.localPosition = Target.localPosition;
		}
	}
}
