using UnityEngine;

namespace GrandTour
{
	public class LevelSO : ScriptableObject
	{
		public int levelId;
		public int types;
		public float passPercent;
		public int timeLimit;
		public int totalRounds;
		public int rangeOfTileWeightMin;
		public int rangeOfTileWeightMax;
		public int levelUpCriteria;
		public int levelDownCriteria;
		public int maxScoreMap;
	}
}