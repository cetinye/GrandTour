using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace GrandTour
{
    public class HexController : MonoBehaviour
    {
        public static HexController instance;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Cinemachine.CinemachineTargetGroup targetGroup;

        private int width, height;
        private bool isFirstRun = true;

        [SerializeField] private List<GameObject> countries = new List<GameObject>();
        [SerializeField] private float showPathInterval;
        [SerializeField] private float yOffsetInterval;
        [SerializeField] private float colorTransitionDuration;
        [SerializeField] private float highlightDuration;
        [SerializeField] private List<GT_Color> colors = new List<GT_Color>();

        [SerializeField] private Transform hexPref;
        [SerializeField] private GameObject finishFlagPref;
        private GameObject finishFlag;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField] private PlayerController playerController;
        private ParticleSystem spawnedConfetti;

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
            public bool isVisited = false;
        }

        private void Awake()
        {
            instance = this;
        }

        public void Initialize()
        {
            SpawnCountry(isFirstRun);

            CreateGrid();

            if (isFirstRun)
                StartCoroutine(ShowCountryRoutine());

            isFirstRun = false;
        }

        public void Restart()
        {
            if (selectedCountry != null)
                Destroy(selectedCountry.gameObject);

            if (playerController != null)
                targetGroup.RemoveMember(playerController.transform);

            if (finishFlag != null)
                targetGroup.RemoveMember(finishFlag.transform);
        }

        private void SpawnCountry(bool isFirstRun)
        {
            int randCountryIndex = 0;
            if (isFirstRun)
                randCountryIndex = Random.Range(0, countries.Count);

            selectedCountry = Instantiate(countries[randCountryIndex], transform).GetComponent<Country>();
            width = selectedCountry.width;
            height = selectedCountry.height;

            if (!isFirstRun)
                selectedCountry.SetOutline(false);

            targetGroup.AddMember(playerController.transform, 1f, 1f);
        }

        IEnumerator ShowCountryRoutine()
        {
            levelManager.SetVirtualCamPriority(1);
            playerController.SetParentHex(true);
            Sequence countrySeq = selectedCountry.SpawnAnim();
            yield return countrySeq.WaitForCompletion();
            levelManager.SetVirtualCamPriority(3);
            yield return new WaitForSeconds(2f);
            levelManager.SetTimerState(true);
            playerController.SetParentHex(false);
            playerController.SetCarControls(true);
        }

        private void CreateGrid()
        {
            float cellSize = 1f;
            gridHexXZ =
                new GridHexXZ<GridObject>(width, height, cellSize, selectedCountry.transform.position, (GridHexXZ<GridObject> g, int x, int y) => new GridObject());

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

            gridHexXZ.GetGridObject(startPointX, startPointZ).isVisited = true;

            playerController.SetGridPosition(startPointX, startPointZ);
        }

        private void SelectEndPoint()
        {
            do
            {
                endPointX = Random.Range(0, gridHexXZ.GetWidth());
                endPointZ = Random.Range(0, gridHexXZ.GetHeight());
            } while (gridHexXZ.GetGridObject(endPointX, endPointZ).isActive == false || (endPointX == startPointX && endPointZ == startPointZ));

            finishFlag = Instantiate(finishFlagPref, finishFlagPref.transform.position, finishFlagPref.transform.rotation, gridHexXZ.GetGridObject(endPointX, endPointZ).visualTransform);
            finishFlag.transform.position = gridHexXZ.GetWorldPosition(endPointX, endPointZ);
            spawnedConfetti = Instantiate(confetti, finishFlag.transform.position, confetti.transform.rotation, finishFlag.transform.parent);

            targetGroup.AddMember(finishFlag.transform, 1f, 1f);

            Highlight(endPointX, endPointZ);
        }

        private void AssignRandomWeights()
        {
            List<GT_Color> tempColors = new List<GT_Color>(colors.GetRange(0, LevelManager.LevelSO.types));
            List<int> tempWeights = new List<int>();

            tempWeights = GenerateTileWeights(LevelManager.LevelSO.algorithmId);

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
                        gridObject.tileWeight = tempWeights[randIndex];
                        gridObject.meshRenderer.material.color = tempColors[randIndex].color;
                        gridObject.color = tempColors[randIndex];

                        gridObject.isActive = true;
                        pathfindingHexXZ.GetNode(x, y).isWalkable = true;
                    }
                    else
                    {
                        pathfindingHexXZ.GetNode(x, y).isWalkable = false;
                    }
                }
            }

            for (int i = 0; i < tempColors.Count; i++)
            {
                uiManager.SetUpInfoElement(tempColors[i].sprite, tempWeights[i].ToString("F0"));
            }
            uiManager.SortInfoElements();
        }

        private List<int> GenerateTileWeights(int algorithmId)
        {
            List<int> values = new List<int>();
            if (algorithmId == 1)
            {
                for (int i = 0; i < LevelManager.LevelSO.types; i++)
                {
                    List<int> rolledValues = new List<int>();
                    int randVal;
                    do
                    {
                        randVal = Random.Range(1, 10);
                    } while (rolledValues.Contains(randVal));

                    randVal *= 10;
                    values.Add(randVal);
                }

                int unlucky = Random.Range(1, LevelManager.LevelSO.types + 1);
                values[unlucky - 1] *= 10;

                int lucky;
                do
                {
                    lucky = Random.Range(1, LevelManager.LevelSO.types + 1);
                } while (lucky == unlucky);
                values[lucky - 1] /= 10;
            }

            else if (algorithmId == 2)
            {
                for (int i = 0; i < LevelManager.LevelSO.types; i++)
                {
                    List<int> rolledValues = new List<int>();
                    int randVal;
                    do
                    {
                        randVal = Random.Range(1, 10);
                    } while (rolledValues.Contains(randVal));

                    randVal *= 10;
                    values.Add(randVal);
                }

                int unlucky = Random.Range(1, LevelManager.LevelSO.types + 1);
                values[unlucky - 1] *= 5;

                int lucky;
                do
                {
                    lucky = Random.Range(1, LevelManager.LevelSO.types + 1);
                } while (lucky == unlucky);
                values[lucky - 1] /= 5;
            }

            else if (algorithmId == 3)
            {
                for (int i = 0; i < LevelManager.LevelSO.types; i++)
                {
                    int randVal;
                    do
                    {
                        randVal = Random.Range(1, 10);
                    } while (values.Contains(randVal));

                    randVal *= 10;
                    values.Add(randVal);
                }

                List<int> usedRands = new List<int>();
                for (int i = 0; i < LevelManager.LevelSO.types; i++)
                {
                    int r;
                    do
                    {
                        r = Random.Range(1, 101);
                    } while (usedRands.Contains(r));
                    usedRands.Add(r);
                    values[i] += r;
                    values[i] += LevelManager.LevelSO.levelId * 10;
                }
            }
            return values;
        }

        public void ShowShortestPath(bool isTimesUp = false)
        {
            pathList = pathfindingHexXZ.FindPath(startPointX, startPointZ, endPointX, endPointZ);
            StartCoroutine(ShowPathAnim(isTimesUp));
        }

        public int GetTotalCost() { return pathfindingHexXZ.GetTotalCost(); }

        IEnumerator ShowPathAnim(bool isTimesUp = false)
        {
            playerController.SetParentHex(true);

            for (int i = 0; i < pathList.Count; i++)
            {
                GridObject gridObj = gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y);
                gridObj.visualTransform.DOMoveY(0.5f, yOffsetInterval).SetEase(Ease.InOutQuart);
                ColorHex(pathList[i].x, pathList[i].y, new Color(0.1843137f, 0.3803922f, 0.03921569f, 0f), 2f, false);

                if (gridObj.isVisited)
                {
                    AudioManager.instance.PlayOneShot(SoundType.CorrectHex);
                }
                else
                {
                    AudioManager.instance.PlayOneShot(SoundType.WrongHex);
                }

                yield return new WaitForSeconds(showPathInterval);
            }
            yield return new WaitForSeconds(showPathInterval);

            playerController.SetParentHex(false);
            levelManager.EndLevel(isTimesUp);
        }

        public void ColorHex(int x, int z, Color color, float emissionVal, bool useEmission = true)
        {
            GridObject gridObj = gridHexXZ.GetGridObject(x, z);
            if (useEmission)
                gridObj.meshRenderer.material.EnableKeyword("_EMISSION");
            else
                gridObj.meshRenderer.material.DisableKeyword("_EMISSION");
            // gridObj.meshRenderer.material.color = color;
            gridObj.meshRenderer.material.DOColor(color, colorTransitionDuration);
            gridObj.meshRenderer.material.SetColor("_EmissionColor", color * emissionVal);

            /*

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

            */
        }

        private void Highlight(int x, int z)
        {
            GridObject gridObj = gridHexXZ.GetGridObject(x, z);
            gridObj.meshRenderer.material.EnableKeyword("_EMISSION");

            Sequence seq = DOTween.Sequence();
            seq.Append(gridObj.meshRenderer.material.DOColor(gridObj.meshRenderer.material.color * 2.5f, "_EmissionColor", highlightDuration));
            seq.Append(gridObj.meshRenderer.material.DOColor(gridObj.meshRenderer.material.color * 0f, "_EmissionColor", highlightDuration));
            seq.SetLoops(-1, LoopType.Restart);
        }

        public void FireConfetti()
        {
            AudioManager.instance.PlayOneShot(SoundType.Success);
            spawnedConfetti.Play();
        }

        public int GetStartPointX() { return startPointX; }
        public int GetStartPointZ() { return startPointZ; }
        public int GetEndPointX() { return endPointX; }
        public int GetEndPointZ() { return endPointZ; }
    }

    [System.Serializable]
    public class GT_Color
    {
        public string name;
        public Color color;
        public Sprite sprite;
    }
}