using UnityEngine;
using UnityEditor;
using System.IO;

namespace GrandTour
{
    public class CSVtoSO_GrandTour
    {
        //Check .csv path
        private static string CSVPath = "/Editor/LevelCSV_GrandTour.csv";

        [MenuItem("Tools/CSV_to_SO/GrandTour/Generate")]
        public static void GenerateSO()
        {
            int startingNamingIndex = 1;
            string[] allLines = File.ReadAllLines(Application.dataPath + CSVPath);

            for (int i = 1; i < allLines.Length; i++)
            {
                allLines[i] = RedefineString(allLines[i]);
            }

            for (int i = 1; i < allLines.Length; i++)
            {
                string[] splitData = allLines[i].Split(';');

                //Check data indexes
                LevelSO level = ScriptableObject.CreateInstance<LevelSO>();
                level.levelId = int.Parse(splitData[0]);
                level.types = int.Parse(splitData[1]);
                level.passPercent = float.Parse(splitData[2]);
                level.timeLimit = int.Parse(splitData[3]);
                level.totalRounds = int.Parse(splitData[4]);
                level.algorithmId = int.Parse(splitData[5]);
                level.levelUpCriteria = int.Parse(splitData[6]);
                level.levelDownCriteria = int.Parse(splitData[7]);
                level.maxScoreMap = int.Parse(splitData[8]);

                AssetDatabase.CreateAsset(level, $"Assets/Data/Grand_Tour/Levels/{"GrandTour_Level " + startingNamingIndex}.asset");
                startingNamingIndex++;
            }

            AssetDatabase.SaveAssets();

            static string RedefineString(string val)
            {
                char[] charArr = val.ToCharArray();
                bool isSplittable = true;

                for (int i = 0; i < charArr.Length; i++)
                {
                    if (charArr[i] == '"')
                    {
                        charArr[i] = ' ';
                        isSplittable = !isSplittable;
                    }

                    if (isSplittable && charArr[i] == ',')
                        charArr[i] = ';';

                    if (isSplittable && charArr[i] == '.')
                        charArr[i] = ',';
                }

                return new string(charArr);
            }
        }
    }
}