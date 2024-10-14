using System.Collections.Generic;
using UnityEngine;

namespace GrandTour
{
    public class HexController : MonoBehaviour
    {
        public static HexController instance;

        public int width, height;

        [SerializeField] private int typeAmount;
        [SerializeField] private List<GT_Color> colors = new List<GT_Color>();
        [SerializeField] private List<int> weights = new List<int>();
        [SerializeField] private TMPro.TMP_Text infoText;
        [SerializeField] private TMPro.TMP_Text infoPlayerText;

        [SerializeField] private Transform hexPref;
        [SerializeField] private GameObject finishFlagPref;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GridHexXZ<GridObject> destinationHex;

        public GridHexXZ<GridObject> gridHexXZ;
        private PathfindingHexXZ pathfindingHexXZ;

        [SerializeField] private int startPointX, startPointZ;
        [SerializeField] private int endPointX, endPointZ;

        private List<PathNodeHexXZ> pathList;

        public class GridObject
        {
            public Transform visualTransform;
            public MeshRenderer meshRenderer;
            public bool isActive = false;
            public int tileWeight;
        }

        private void Awake()
        {
            instance = this;

            float cellSize = 1f;
            gridHexXZ =
                new GridHexXZ<GridObject>(width, height, cellSize, Vector3.zero, (GridHexXZ<GridObject> g, int x, int y) => new GridObject());

            int hexCounter = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridObject gridObject = new GridObject
                    {
                        visualTransform = transform.GetChild(0).GetChild(0).GetChild(hexCounter),
                        meshRenderer = transform.GetChild(0).GetChild(0).GetChild(hexCounter).GetComponentInChildren<MeshRenderer>(),
                    };
                    gridHexXZ.SetGridObject(x, z, gridObject);
                    hexCounter++;
                }
            }

            pathfindingHexXZ = new PathfindingHexXZ(width, height, cellSize);

            AssignRandomWeights();

            SelectStartPoint();
            SelectEndPoint();

            playerController.SetMovePosition(gridHexXZ.GetWorldPosition(startPointX, startPointZ), true);
            playerController.SetGridPosition(startPointX, startPointZ);
        }

        private void SelectStartPoint()
        {
            do
            {
                startPointX = Random.Range(0, gridHexXZ.GetWidth());
                startPointZ = Random.Range(0, gridHexXZ.GetHeight());
            } while (gridHexXZ.GetGridObject(startPointX, startPointZ).isActive == false);
        }

        private void SelectEndPoint()
        {
            do
            {
                endPointX = Random.Range(0, gridHexXZ.GetWidth());
                endPointZ = Random.Range(0, gridHexXZ.GetHeight());
            } while (gridHexXZ.GetGridObject(endPointX, endPointZ).isActive == false || (endPointX == startPointX && endPointZ == startPointZ));

            GameObject finishFlag = Instantiate(finishFlagPref, finishFlagPref.transform.position, finishFlagPref.transform.rotation, gridHexXZ.GetGridObject(endPointX, endPointZ).visualTransform);
            finishFlag.transform.position = gridHexXZ.GetWorldPosition(endPointX, endPointZ);
        }

        private void AssignRandomWeights()
        {
            colors = colors.GetRange(0, typeAmount);
            weights = weights.GetRange(0, typeAmount);

            for (int x = 0; x < gridHexXZ.GetWidth(); x++)
            {
                for (int y = 0; y < gridHexXZ.GetHeight(); y++)
                {
                    GridObject gridObject = gridHexXZ.GetGridObject(x, y);
                    if (gridObject.visualTransform.GetChild(0).gameObject.activeSelf != false)
                    {
                        int randIndex = Random.Range(0, typeAmount);
                        gridObject.tileWeight = weights[randIndex];
                        gridObject.meshRenderer.material.color = colors[randIndex].color;

                        gridObject.isActive = true;
                        pathfindingHexXZ.GetNode(x, y).isWalkable = true;
                    }
                    else
                    {
                        pathfindingHexXZ.GetNode(x, y).isWalkable = false;
                    }
                }
            }

            for (int i = 0; i < colors.Count; i++)
            {
                infoText.text = infoText.text + "\n" + colors[i].name + ": " + weights[i];
            }
        }

        public void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public void WriteCoveredTiles(int playerCoveredTotal)
        {
            infoPlayerText.text = "Player Covered: " + playerCoveredTotal;
        }

        public void FindPath()
        {
            if (pathList != null)
            {
                // Reset all colors to white
                for (int i = 0; i < pathList.Count - 1; i++)
                {
                    // gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y).meshRenderer.material.color = Color.white;

                    GridObject gridObj = gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y);
                    gridObj.visualTransform.position = new Vector3(gridObj.visualTransform.position.x, 0f, gridObj.visualTransform.position.z);
                }
            }

            pathList = pathfindingHexXZ.FindPath(startPointX, startPointZ, endPointX, endPointZ);
            infoText.text = infoText.text + "\nTotal Cost of Path: " + pathfindingHexXZ.GetTotalCost();

            for (int i = 0; i < pathList.Count - 1; i++)
            {
                // Debug.DrawLine(pathList[i], pathList[i + 1], Color.green, 3f);
                // gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y).meshRenderer.material.color = Color.red;

                GridObject gridObj = gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y);
                gridObj.visualTransform.position = new Vector3(gridObj.visualTransform.position.x, 0.25f, gridObj.visualTransform.position.z);
            }
        }
    }

    [System.Serializable]
    public class GT_Color
    {
        public string name;
        public Color color;

        public static implicit operator Color(GT_Color v)
        {
            throw new System.NotImplementedException();
        }
    }
}