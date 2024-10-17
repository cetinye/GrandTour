using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace GrandTour
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private LevelManager levelManager;

		[Header("Text Variables")]
		[SerializeField] private TMP_Text timeText;
		[SerializeField] private TMP_Text levelText;
		[SerializeField] private TMP_Text scoreText;
		[SerializeField] private TMP_Text infoPlayerText;
		[SerializeField] private TMP_Text roundText;

		[Header("Info Panel Variables")]
		[SerializeField] private GameObject infoPanel;
		[SerializeField] private InfoElement infoElementPref;
		[SerializeField] private List<RectTransform> slotPositions = new List<RectTransform>();
		[SerializeField] private Transform infoElementsParent;
		[SerializeField] private float infoElementsMoveDuration;

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

		public void UpdateRoundText(int round, int totalRounds)
		{
			roundText.text = round.ToString() + " / " + totalRounds.ToString();
		}

		public void SetUpInfoElement(Sprite sprite, string text)
		{
			InfoElement infoElement = Instantiate(infoElementPref, infoElementsParent.transform);
			infoElement.SetElement(sprite, text);
			RectTransform r = infoElement.GetComponent<RectTransform>();
			r.anchoredPosition = new Vector3(0, -1500, 0);
		}

		public void AnimateInfoElements()
		{
			StartCoroutine(InfoElementAnim());
		}

		IEnumerator InfoElementAnim()
		{
			infoPanel.SetActive(true);

			for (int i = 0; i < infoElementsParent.childCount; i++)
			{
				RectTransform r = infoElementsParent.GetChild(i).GetComponent<RectTransform>();
				Tween t = r.DOAnchorPos(slotPositions[i].anchoredPosition, infoElementsMoveDuration).SetEase(Ease.InOutQuad);
				yield return t.WaitForCompletion();
			}
			yield return new WaitForSeconds(1f);

			infoPanel.SetActive(false);
			levelManager.StartPlaying();
		}

		public void SortInfoElements()
		{
			for (int i = 0; i < infoElementsParent.childCount; i++)
			{
				for (int j = i + 1; j < infoElementsParent.childCount; j++)
				{
					InfoElement currentElement = infoElementsParent.GetChild(i).GetComponent<InfoElement>();
					InfoElement nextElement = infoElementsParent.GetChild(j).GetComponent<InfoElement>();

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
			if (levelManager.GetTimerStatus())
				infoPanel.SetActive(!infoPanel.activeSelf);
		}

		public void SetInfoPanelState(bool state)
		{
			infoPanel.SetActive(state);
		}

		public void WriteCoveredTiles(int playerCoveredTotal)
		{
			infoPlayerText.text = playerCoveredTotal.ToString();
		}

		public void Restart()
		{
			WriteCoveredTiles(0);

			for (int i = 0; i < infoElementsParent.childCount; i++)
			{
				Destroy(infoElementsParent.GetChild(i).gameObject);
			}

			infoPanel.SetActive(false);
		}
	}
}