using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandTour
{
	public class InfoElement : MonoBehaviour
	{
		[SerializeField] private Image hexImage;
		[SerializeField] private TMP_Text weightCost;

		public void SetElement(Sprite sprite, string text)
		{
			hexImage.sprite = sprite;
			weightCost.text = text;
		}
	}
}