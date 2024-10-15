using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandTour
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private TMP_Text timeText;
		[SerializeField] private TMP_Text levelText;
		[SerializeField] private TMP_Text scoreText;

		[Header("Info Panel Variables")]
		[SerializeField] private GameObject infoPanel;
		[SerializeField] private InfoElement infoElementPref;
		[SerializeField] private VerticalLayoutGroup layoutGroup;

		public void UpdateTimeText(int time)
		{
			timeText.text = time.ToString();
		}

		public void UpdateLevelText(int level)
		{
			levelText.text = "Level" + level.ToString();
		}

		public void UpdateScoreText(int score)
		{
			scoreText.text = "Score: " + score.ToString();
		}

		public void SetUpInfoElement(Sprite sprite, string text)
		{
			InfoElement infoElement = Instantiate(infoElementPref, layoutGroup.transform);
			infoElement.SetElement(sprite, text);
		}

		public void InfoPanelStateSwitch()
		{
			infoPanel.SetActive(!infoPanel.activeSelf);
		}
	}
}