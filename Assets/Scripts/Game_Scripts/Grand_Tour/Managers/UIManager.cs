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
		[SerializeField] private TMP_Text infoPlayerText;

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
			levelText.text = "Level: " + level.ToString();
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

		public void SortInfoElements()
		{
			for (int i = 0; i < layoutGroup.transform.childCount; i++)
			{
				for (int j = i + 1; j < layoutGroup.transform.childCount; j++)
				{
					InfoElement currentElement = layoutGroup.transform.GetChild(i).GetComponent<InfoElement>();
					InfoElement nextElement = layoutGroup.transform.GetChild(j).GetComponent<InfoElement>();

					if (nextElement.GetCost() > currentElement.GetCost())
					{
						nextElement.transform.SetSiblingIndex(i);
						currentElement.transform.SetSiblingIndex(j);
					}
				}
			}
		}

		public void InfoPanelStateSwitch()
		{
			infoPanel.SetActive(!infoPanel.activeSelf);
		}

		public void WriteCoveredTiles(int playerCoveredTotal)
		{
			infoPlayerText.text = playerCoveredTotal.ToString();
		}

		public void Restart()
		{
			WriteCoveredTiles(0);

			for (int i = 0; i < layoutGroup.transform.childCount; i++)
			{
				Destroy(layoutGroup.transform.GetChild(i).gameObject);
			}

			infoPanel.SetActive(false);
		}
	}
}