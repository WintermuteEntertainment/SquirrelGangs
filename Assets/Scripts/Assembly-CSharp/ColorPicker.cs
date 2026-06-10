using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	public Slider redSlider;

	public Slider greenSlider;

	public Slider blueSlider;

	public Slider alphaSlider;

	public Image colorPreview;

	public string playerKey;

	private void Start()
	{
		LoadColor();
		redSlider.onValueChanged.AddListener(delegate
		{
			OnColorChanged();
		});
		greenSlider.onValueChanged.AddListener(delegate
		{
			OnColorChanged();
		});
		blueSlider.onValueChanged.AddListener(delegate
		{
			OnColorChanged();
		});
		alphaSlider.onValueChanged.AddListener(delegate
		{
			OnColorChanged();
		});
	}

	public void OnColorChanged()
	{
		Color color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
		colorPreview.color = color;
	}

	public Color SaveColor(Color color)
	{
		color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
		PlayerPrefsUtility.SetColor(playerKey, color);
		return color;
	}

	private void LoadColor()
	{
		Color color = PlayerPrefsUtility.GetColor(playerKey, Color.white);
		redSlider.value = color.r;
		greenSlider.value = color.g;
		blueSlider.value = color.b;
		alphaSlider.value = color.a;
		colorPreview.color = color;
	}

	public Color GetSelectedColor(Color nutColor)
	{
		return new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
	}
}
