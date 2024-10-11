using System.Collections.Generic;
using UnityEngine;

namespace GrandTour
{
    public class HexController : MonoBehaviour
    {
        [SerializeField] private int width, height;

        [SerializeField] private Transform pfHex;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GridHexXZ<GridObject> destinationHex;

        private GridHexXZ<GridObject> gridHexXZ;
        private PathfindingHexXZ pathfindingHexXZ;

        [SerializeField] private int startPointX, startPointZ;
        [SerializeField] private int endPointX, endPointZ;

        private class GridObject
        {
            public Transform visualTransform;
            public MeshRenderer meshRenderer;
        }

        private void Awake()
        {
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

            playerController.SetMovePosition(gridHexXZ.GetWorldPosition(startPointX, startPointZ));
        }

        private void SelectStartPoint()
        {
            startPointX = Random.Range(0, gridHexXZ.GetWidth());
            startPointZ = Random.Range(0, gridHexXZ.GetHeight());
            gridHexXZ.GetGridObject(startPointX, startPointZ).meshRenderer.material.color = Color.green;
        }

        private void SelectEndPoint()
        {
            endPointX = Random.Range(0, gridHexXZ.GetWidth());
            endPointZ = Random.Range(0, gridHexXZ.GetHeight());
            gridHexXZ.GetGridObject(endPointX, endPointZ).meshRenderer.material.color = Color.blue;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                List<PathNodeHexXZ> pathList = pathfindingHexXZ.FindPath(startPointX, startPointZ, endPointX, endPointZ);
                for (int i = 0; i < pathList.Count - 1; i++)
                {
                    // Debug.DrawLine(pathList[i], pathList[i + 1], Color.green, 3f);
                    gridHexXZ.GetGridObject(pathList[i].x, pathList[i].y).meshRenderer.material.color = Color.red;
                }
            }
        }
    }
}