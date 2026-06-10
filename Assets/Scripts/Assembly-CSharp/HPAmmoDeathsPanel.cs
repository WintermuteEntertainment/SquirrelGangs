using UnityEngine;

public class HPAmmoDeathsPanel : MonoBehaviour
{
	[SerializeField]
	private GameObject p1;

	[SerializeField]
	private GameObject p2;

	[SerializeField]
	private GameObject p3;

	[SerializeField]
	private GameObject p4;

	[SerializeField]
	private GameObject p1Ammo;

	[SerializeField]
	private GameObject p2Ammo;

	[SerializeField]
	private GameObject p3Ammo;

	[SerializeField]
	private GameObject p4Ammo;

	[SerializeField]
	private GameObject p1HP;

	[SerializeField]
	private GameObject p2HP;

	[SerializeField]
	private GameObject p3HP;

	[SerializeField]
	private GameObject p4HP;

	[SerializeField]
	private UIManager uIManager;

	private void Awake()
	{
		p1 = GetComponent<GameObject>();
		p2 = GetComponent<GameObject>();
		p3 = GetComponent<GameObject>();
		p4 = GetComponent<GameObject>();
	}

	public void HideUIElements()
	{
		if (uIManager != null)
		{
			if (p1.gameObject == null)
			{
				p1HP.SetActive(value: false);
				p1Ammo.SetActive(value: false);
			}
			if (p2.gameObject != null)
			{
				p2HP.SetActive(value: false);
				p2Ammo.SetActive(value: false);
			}
			if (p3.gameObject != null)
			{
				p3HP.SetActive(value: false);
				p3Ammo.SetActive(value: false);
			}
			if (p4.gameObject != null)
			{
				p4HP.SetActive(value: false);
				p4Ammo.SetActive(value: false);
			}
			uIManager.UpdateUI();
		}
	}

	private void Update()
	{
		HideUIElements();
	}
}
