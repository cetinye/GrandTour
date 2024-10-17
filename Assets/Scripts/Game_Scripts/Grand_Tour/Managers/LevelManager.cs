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
			StartGame();
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
			int shortestCost = hexController.GetTotalCost();
			int playerCost = playerController.GetTravelledWeights();
			float currentScore = ((float)(shortestCost - playerCost) / shortestCost) * 100;
			currentScore = Mathf.CeilToInt(currentScore);
			currentScore = Mathf.Abs(currentScore);
			bool isSuccess;

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

			StartCoroutine(RestartGame(isSuccess));
		}

		IEnumerator RestartGame(bool isSuccess)
		{
			yield return new WaitForSeconds(1);

			DecideLevel(isSuccess);

			if (++roundsPlayed >= totalRounds + 1)
			{
				Debug.Log("Grand Tour Completed");
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