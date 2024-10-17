using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace GrandTour
{
	public class LevelManager : MonoBehaviour
	{
		[Header("Level Variables")]
		private int levelId;
		[SerializeField] private List<LevelSO> levels = new List<LevelSO>();
		public static LevelSO LevelSO;
		private List<int> levelScores = new List<int>();
		private int shortestCost;
		private int playerCost;
		private int currentScore;

		[Header("Other Controllers")]
		[SerializeField] private UIManager uiManager;
		[SerializeField] private HexController hexController;
		[SerializeField] private PlayerController playerController;

		[Header("Timer Variables")]
		private bool isLevelTimerOn = false;
		private float timer, timeLimit;

		[Header("Round Variables")]
		private int roundsPlayed = 1;
		private int totalRounds;

		[Header("Virtual Camera Variables")]
		[SerializeField] private CinemachineVirtualCamera virtualCamera;

		void Awake()
		{

		}

		void Start()
		{
			uiManager.PlayPauseVideo();
			uiManager.StartIntro();
		}

		public void StartGame()
		{
			AudioManager.instance.Play(SoundType.Engine);

			AssignLevel();
			AssignLevelVariables();

			uiManager.Restart();
			hexController.Restart();
			playerController.Restart();

			uiManager.UpdateTimeText((int)timeLimit);
			hexController.Initialize();
		}

		public void StartPlaying()
		{
			SetTimerState(true);
			playerController.SetParentHex(false);
			playerController.SetCarControls(true);
			AudioManager.instance.PlayOneShot(SoundType.Horn);
		}

		private void AssignLevel()
		{
			levelId = PlayerPrefs.GetInt("GrandTour_Level", 1);
			levelId = Mathf.Clamp(levelId, 1, levels.Count);
			LevelSO = levels[levelId - 1];

			uiManager.UpdateLevelText(levelId);
		}

		private void AssignLevelVariables()
		{
			timeLimit = LevelSO.timeLimit;
			totalRounds = LevelSO.totalRounds;

			timer = timeLimit;
			uiManager.UpdateRoundText(roundsPlayed, totalRounds);
		}

		private void Update()
		{
			if (!isLevelTimerOn) return;

			timer -= Time.deltaTime;

			if (timer < 0)
			{
				isLevelTimerOn = false;
				timer = 0;
				hexController.ShowShortestPath(true);
			}

			uiManager.UpdateTimeText((int)timer);
		}

		public void EndLevel(bool isTimesUp = false)
		{
			shortestCost = hexController.GetTotalCost();
			playerCost = playerController.GetTravelledWeights();
			currentScore = Mathf.Abs(Mathf.CeilToInt(((float)(shortestCost - playerCost) / shortestCost) * 100));
			bool isSuccess;

			Debug.Log("Current Score: " + currentScore);
			Debug.Log("Pass Percent: " + LevelSO.passPercent);

			if (currentScore <= LevelSO.passPercent)
				isSuccess = true;

			else
				isSuccess = false;

			if (isTimesUp)
			{
				AudioManager.instance.PlayOneShot(SoundType.Fail);
				Debug.Log("Times Up");
			}
			else if (!isSuccess && !isTimesUp)
			{
				AudioManager.instance.PlayOneShot(SoundType.Fail);
				Debug.Log("Level Failed");
			}
			else if (isSuccess && !isTimesUp)
			{
				hexController.FireConfetti();
				Debug.Log("Level Completed");
			}

			CalculateScore(currentScore);
			DecideLevel(isSuccess);
			uiManager.ShowStatPanel();
		}

		private void CalculateScore(int percent)
		{
			int score = LevelSO.maxScoreMap - (LevelSO.maxScoreMap * percent / 100);
			levelScores.Add(score);
		}

		private int GetTotalScore()
		{
			int totalScore = 0;
			for (int i = 0; i < levelScores.Count; i++)
			{
				totalScore += levelScores[i];
			}
			totalScore /= levelScores.Count;
			return Mathf.Clamp(totalScore, 0, 1000);
		}

		public void RestartGame()
		{
			StartCoroutine(RestartGameRoutine());
		}

		IEnumerator RestartGameRoutine()
		{
			yield return new WaitForEndOfFrame();

			uiManager.SetStatPanelState(false);

			if (++roundsPlayed >= totalRounds + 1)
			{
				uiManager.UpdateScoreText(GetTotalScore());
				yield return new WaitForSeconds(1f);
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			}
			uiManager.UpdateRoundText(roundsPlayed, totalRounds);

			StartGame();
		}

		private void DecideLevel(bool isSuccess)
		{
			if (isSuccess)
			{
				int upCounter = PlayerPrefs.GetInt("GrandTour_LevelUpCounter", 0);
				if (++upCounter >= LevelSO.levelUpCriteria)
				{
					upCounter = 0;
					PlayerPrefs.SetInt("GrandTour_LevelDownCounter", 0);
					PlayerPrefs.SetInt("GrandTour_Level", ++levelId);
				}
				PlayerPrefs.SetInt("GrandTour_LevelUpCounter", upCounter);
			}
			else
			{
				int downCounter = PlayerPrefs.GetInt("GrandTour_LevelDownCounter", 0);
				if (++downCounter >= LevelSO.levelDownCriteria)
				{
					downCounter = 0;
					PlayerPrefs.SetInt("GrandTour_LevelUpCounter", 0);
					PlayerPrefs.SetInt("GrandTour_Level", --levelId);
				}
				PlayerPrefs.SetInt("GrandTour_LevelDownCounter", downCounter);
			}
		}

		public void SetVirtualCamPriority(int value)
		{
			virtualCamera.Priority = value;
		}

		public void SetTimerState(bool state)
		{
			isLevelTimerOn = state;
		}

		public bool GetTimerStatus()
		{
			return isLevelTimerOn;
		}

		public int GetBestRoute()
		{
			return shortestCost;
		}

		public int GetPlayerRoute()
		{
			return playerCost;
		}

		public int GetPercent()
		{
			return currentScore;
		}

		#region DEBUG BUTTON FUNCTIONS

		public void LevelChange(int status)
		{
			levelId += status;
			PlayerPrefs.SetInt("GrandTour_Level", levelId);

			uiManager.UpdateLevelText(levelId);
		}

		#endregion
	}
}