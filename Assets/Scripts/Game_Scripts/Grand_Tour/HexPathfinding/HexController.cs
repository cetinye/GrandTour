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

        [SerializeField] private Transform pfHex;
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
            public int tileWeight;
        }

        private void Awake()
        {
            instance = this;

            float cellSize = 1f;
            gridHexXZ =
                new GridHexXZ<GridObject>(width, height, cellSize, Vector3.zero, (GridHexXZ<GridObject> g, int x, int y) => new GridObject());

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Transform visualTransform = Instantiate(pfHex, gridHexXZ.GetWorldPosition(x, z), Quaternion.identity, transform);
                    MeshRenderer meshRenderer = visualTransform.GetComponentInChildren<MeshRenderer>();
                    gridHexXZ.GetGridObject(x, z).visualTransform = visualTransform;
                    gridHexXZ.GetGridObject(x, z).meshRenderer = meshRenderer;
                }
            }

            pathfindingHexXZ = new PathfindingHexXZ(width, height, cellSize);

            SelectStartPoint();
            SelectEndPoint();

            AssignRandomWeights();

            playerController.SetMovePosition(gridHexXZ.GetWorldPosition(startPointX, startPointZ), true);
            playerController.SetGridPosition(startPointX, startPointZ);
        }

        private void SelectStartPoint()
        {
            startPointX = Random.Range(0, gridHexXZ.GetWidth());
            startPointZ = Random.Range(0, gridHexXZ.GetHeight());
            // gridHexXZ.GetGridObject(startPointX, startPointZ).meshRenderer.material.color = Color.green;
            gridHexXZ.GetGridObject(startPointX, startPointZ).visualTransform.Translate(0f, 0.25f, 0f);
        }

        private void SelectEndPoint()
        {
            endPointX = Random.Range(0, gridHexXZ.GetWidth());
            endPointZ = Random.Range(0, gridHexXZ.GetHeight());
            // gridHexXZ.GetGridObject(endPointX, endPointZ).meshRenderer.material.color = Color.blue;
            gridHexXZ.GetGridObject(endPointX, endPointZ).visualTransform.Translate(0f, 0.25f, 0f);
        }

        private void AssignRandomWeights()
        {
            colors = colors.GetRange(0, typeAmount);
            weights = weights.GetRange(0, typeAmount);

            for (int x = 0; x < gridHexXZ.GetWidth(); x++)
            {
                for (int y = 0; y < gridHexXZ.GetHeight(); y++)
                {
                    int randIndex = Random.Range(0, typeAmount);
                    gridHexXZ.GetGridObject(x, y).tileWeight = weights[randIndex];
                    gridHexXZ.GetGridObject(x, y).meshRenderer.material.color = colors[randIndex].color;
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

            pathList = pathfindingHexXZ.FindPath(playerController.x, playerController.z, endPointX, endPointZ);
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