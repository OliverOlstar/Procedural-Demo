using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class UIFitInsideParent : MonoBehaviour {
	public float horizontalInset, verticalInset;
	public bool allowUpscale = false;

	void Rescale()
	{
		if (!TryGetComponent<RectTransform>(out RectTransform rt))
		{
			return;
		}

		if (!rt.parent.TryGetComponent<RectTransform>(out RectTransform pRt))
		{
			return;
		}

		float w = rt.rect.width + horizontalInset * 2f + Mathf.Abs(rt.localPosition.x) * 2f;
		float h = rt.rect.height + verticalInset * 2f + Mathf.Abs(rt.localPosition.y) * 2f;
		float ar = w / h;
		float pW = pRt.rect.width;
		float pH = pRt.rect.height;
		float pAr = pW / pH;
		float scaleBefore = rt.localScale.x;
		float scaleAfter = allowUpscale ? 1000f : 1f;
		if (ar > pAr)
		{
			scaleAfter = Mathf.Min(pW / w, scaleAfter);
		}
		else
		{
			scaleAfter = Mathf.Min(pH / h, scaleAfter);
		}

		if (!Core.Util.Approximately(scaleAfter, scaleBefore))
		{
			rt.localScale = new Vector3(scaleAfter, scaleAfter, rt.localScale.z);
		}
	}

	void OnEnable()
	{
		Rescale();
	}

	void LateUpdate()
	{
		Rescale();
	}
}
