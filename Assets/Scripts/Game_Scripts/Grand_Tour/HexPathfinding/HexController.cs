using System.Collections.Generic;
using UnityEngine;

namespace GrandTour
{
    public class HexController : MonoBehaviour
    {
        public static HexController instance;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Cinemachine.CinemachineTargetGroup targetGroup;

        private int width, height;

        [SerializeField] private List<GameObject> countries = new List<GameObject>();

        [SerializeField] private List<GT_Color> colors = new List<GT_Color>();
        [SerializeField] private List<int> weights = new List<int>();
        [SerializeField] private TMPro.TMP_Text infoPlayerText;

        [SerializeField] private Transform hexPref;
        [SerializeField] private GameObject finishFlagPref;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GridHexXZ<GridObject> destinationHex;

        public GridHexXZ<GridObject> gridHexXZ;
        private PathfindingHexXZ pathfindingHexXZ;

        [SerializeField] private int startPointX, startPointZ;
        [SerializeField] private int endPointX, endPointZ;

        private Country selectedCountry;

        private List<PathNodeHexXZ> pathList;

        public class GridObject
        {
            public Transform visualTransform;
            public MeshRenderer meshRenderer;
            public GT_Color color;
            public bool isActive = false;
            public int tileWeight;
        }

        private void Awake()
        {
            instance = this;
        }

        public void Initialize()
        {
            SpawnCountry();
            CreateGrid();
        }

        public void Restart()
        {
            if (selectedCountry != null)
                Destroy(selectedCountry.gameObject);

            // if (gridHexXZ != null)
            //     gridHexXZ = null;

            // if (pathfindingHexXZ != null)
            //     pathfindingHexXZ = null;

            // if (pathList != null)
            //     pathList = null;
        }

        private void SpawnCountry()
        {
            int randCountryIndex = Random.Range(0, countries.Count);
            selectedCountry = Instantiate(countries[randCountryIndex], transform).GetComponent<Country>();
            Debug.Log(selectedCountry);
            width = selectedCountry.width;
            height = selectedCountry.height;

            targetGroup.AddMember(playerController.transform, 1f, 1f);
        }

        private void CreateGrid()
        {
            float cellSize = 1f;
            gridHexXZ =
                new GridHexXZ<GridObject>(width, height, cellSize, Vector3.zero, (GridHexXZ<GridObject> g, int x, int y) => new GridObject());

            int hexCounter = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    // Create map
                    /*
                    Transform visualTransform = Instantiate(hexPref, gridHexXZ.GetWorldPosition(x, z), Quaternion.identity, transform);
                    MeshRenderer meshRenderer = visualTransform.GetComponentInChildren<MeshRenderer>();
                    gridHexXZ.GetGridObject(x, z).visualTransform = visualTransform;
                    gridHexXZ.GetGridObject(x, z).meshRenderer = meshRenderer;
                    */

                    //Read map
                    GridObject gridObject = new GridObject
                    {
                        visualTransform = selectedCountry.transform.GetChild(0).GetChild(hexCounter),
                        meshRenderer = selectedCountry.transform.GetChild(0).GetChild(hexCounter).GetComponentInChildren<MeshRenderer>(),
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

            targetGroup.AddMember(finishFlag.transform, 1f, 1f);
        }

        private void AssignRandomWeights()
        {
            colors = colors.GetRange(0, LevelManager.LevelSO.types);
            weights = weights.GetRange(0, LevelManager.LevelSO.types);

            for (int x = 0; x < gridHexXZ.GetWidth(); x++)
            {
                for (int y = 0; y < gridHexXZ.GetHeight(); y++)
                {
                    GridObject gridObject = gridHexXZ.GetGridObject(x, y);

                    if (gridObject == null)
                        continue;

                    if (gridObject.visualTransform.childCount > 0 && gridObject.visualTransform.GetChild(0).gameObject.activeSelf != false)
                    {
                        int randIndex = Random.Range(0, LevelManager.LevelSO.types);
                        gridObject.tileWeight = weights[randIndex];
                        gridObject.meshRenderer.material.color = colors[randIndex].color;
                        gridObject.color = colors[randIndex];

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
                uiManager.SetUpInfoElement(colors[i].sprite, weights[i].ToString("F0"));
            }
        }

        public void WriteCoveredTiles(int playerCoveredTotal)
        {
            infoPlayerText.text = "Player Covered: " + playerCoveredTotal;
        }

        public void FindPath()
        {
            pathList = pathfindingHexXZ.FindPath(startPointX, startPointZ, endPointX, endPointZ);
            Debug.Log("Total Cost of Path: " + pathfindingHexXZ.GetTotalCost());

            for (int i = 0; i < pathList.Count - 1; i++)
            {
                // Debug.DrawLine(pathList[i], pathList[i + 1], Color.green, 3f);
                // gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y).meshRenderer.material.color = Color.red;

                GridObject gridObj = gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y);
                gridObj.visualTransform.position = new Vector3(gridObj.visualTransform.position.x, 0.25f, gridObj.visualTransform.position.z);
                ColorHex(pathList[i].x, pathList[i].y);
            }
        }

        public void ColorHex(int x, int z)
        {
            GridObject gridObj = gridHexXZ.GetGridObject(x, z);
            gridObj.meshRenderer.material.EnableKeyword("_EMISSION");

            switch (gridObj.color.name)
            {
                case "Red":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 2.5f);
                    break;
                case "Green":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 1.5f);
                    break;
                case "Blue":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 2.5f);
                    break;
                case "Yellow":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 1.5f);
                    break;
                case "Pink":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 2.5f);
                    break;
                case "Brown":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 2.5f);
                    break;
                case "Purple":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 2.5f);
                    break;
                case "Cyan":
                    gridObj.meshRenderer.material.SetColor("_EmissionColor", gridObj.color.color * 2.5f);
                    break;
            }
        }
    }

    [System.Serializable]
    public class GT_Color
    {
        public string name;
        public Color color;
        public Sprite sprite;
    }
}