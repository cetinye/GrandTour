using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

		[Header("Stat Panel Variables")]
		[SerializeField] private GameObject statPanel;
		[SerializeField] private TMP_Text bestRouteText;
		[SerializeField] private TMP_Text yourRouteText;
		[SerializeField] private TMP_Text descText;
		[SerializeField] private RectTransform car;
		[SerializeField] private RectTransform startPos;
		[SerializeField] private RectTransform endPos;
		[SerializeField] private ParticleSystem confettiParticle;
		[SerializeField] private Button skipStatButton;

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

		public void ShowStatPanel()
		{
			StartCoroutine(StatPanelAnim());
		}

		IEnumerator StatPanelAnim()
		{
			yield return new WaitForSeconds(1f);
			skipStatButton.interactable = true;
			skipStatButton.gameObject.SetActive(false);
			statPanel.SetActive(true);

			int bestRoute = levelManager.GetBestRoute();
			int playerRoute = levelManager.GetPlayerRoute();
			int percent = levelManager.GetPercent();

			bestRouteText.DOText(bestRoute.ToString(), 1f, false, ScrambleMode.Numerals);
			yield return new WaitForSeconds(1f);
			yourRouteText.DOText(playerRoute.ToString(), 1f, false, ScrambleMode.Numerals);
			yield return new WaitForSeconds(1f);
			float distance = endPos.anchoredPosition.x - startPos.anchoredPosition.x;
			float missPercent = Mathf.Abs(Mathf.CeilToInt((float)(playerRoute - bestRoute) / playerRoute * 100));
			distance -= Mathf.Clamp(distance * missPercent / 100f, 0f, 500f);
			car.DOAnchorPosX(car.anchoredPosition.x + distance, 1f);
			yield return new WaitForSeconds(1f);

			if (playerRoute == bestRoute)
			{
				confettiParticle.Play();
				descText.DOText("PERFECT SCORE!", 2f).SetEase(Ease.Linear);
			}
			else if (percent <= LevelManager.LevelSO.passPercent)
			{
				confettiParticle.Play();
				descText.DOText("Well done! Although you exceeded the budget, you stayed within the %" + LevelManager.LevelSO.passPercent + " margin.", 2f).SetEase(Ease.Linear);
			}
			else
			{
				descText.DOText("Oops! You exceeded the budget limit by more than %" + LevelManager.LevelSO.passPercent + ".", 2f).SetEase(Ease.Linear);
			}

			yield return new WaitForSeconds(3f);
			confettiParticle.Stop();
			skipStatButton.gameObject.SetActive(true);
		}

		public void SetStatPanelState(bool state)
		{
			statPanel.SetActive(state);
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
			SetStatPanelState(false);

			bestRouteText.text = "0";
			yourRouteText.text = "0";
			descText.text = "";
			car.anchoredPosition = new Vector2(startPos.anchoredPosition.x, car.anchoredPosition.y);
		}
	}
}