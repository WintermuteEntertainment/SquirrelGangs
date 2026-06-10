using UnityEngine;

public class DividerManager : MonoBehaviour
{
	[SerializeField]
	private GameObject dividerHorizontal;

	[SerializeField]
	private GameObject dividerVertical;

	public void UpdateDividers(int playerCount)
	{
		dividerHorizontal.SetActive(value: false);
		dividerVertical.SetActive(value: false);
		switch (playerCount)
		{
		case 2:
			dividerHorizontal.SetActive(value: true);
			break;
		case 3:
		{
			dividerHorizontal.SetActive(value: true);
			dividerVertical.SetActive(value: true);
			RectTransform component2 = dividerVertical.GetComponent<RectTransform>();
			if (component2 != null)
			{
				component2.anchorMin = new Vector2(0.5f, 0.5f);
				component2.anchorMax = new Vector2(0.5f, 1f);
				component2.anchoredPosition = Vector2.zero;
			}
			break;
		}
		case 4:
		{
			dividerHorizontal.SetActive(value: true);
			dividerVertical.SetActive(value: true);
			RectTransform component = dividerVertical.GetComponent<RectTransform>();
			if (component != null)
			{
				component.anchorMin = new Vector2(0.5f, 0f);
				component.anchorMax = new Vector2(0.5f, 1f);
				component.anchoredPosition = Vector2.zero;
			}
			break;
		}
		}
	}
}
