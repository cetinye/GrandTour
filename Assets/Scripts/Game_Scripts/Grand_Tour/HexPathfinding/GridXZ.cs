using System;
using UnityEngine;

namespace GrandTour
{
    public class GridXZ<TGridObject>
    {
        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int z;
        }

        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;
        private TGridObject[,] gridArray;

        public GridXZ(int width, int height, float cellSize, Vector3 originPosition, Func<GridXZ<TGridObject>, int, int, TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;

            gridArray = new TGridObject[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    gridArray[x, z] = createGridObject(this, x, z);
                }
            }
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public float GetCellSize()
        {
            return cellSize;
        }

        public Vector3 GetWorldPosition(int x, int z)
        {
            return new Vector3(x, 0, z) * cellSize + originPosition;
        }

        public void GetXZ(Vector3 worldPosition, out int x, out int z)
        {
            x = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize);
            z = Mathf.RoundToInt((worldPosition - originPosition).z / cellSize);
        }

        public void SetGridObject(int x, int z, TGridObject value)
        {
            if (x >= 0 && z >= 0 && x < width && z < height)
            {
                gridArray[x, z] = value;
                TriggerGridObjectChanged(x, z);
            }
        }

        public void TriggerGridObjectChanged(int x, int z)
        {
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
        }

        public void SetGridObject(Vector3 worldPosition, TGridObject value)
        {
            GetXZ(worldPosition, out int x, out int z);
            SetGridObject(x, z, value);
        }

        public TGridObject GetGridObject(int x, int z)
        {
            if (x >= 0 && z >= 0 && x < width && z < height)
            {
                return gridArray[x, z];
            }
            else
            {
                return default(TGridObject);
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            int x, z;
            GetXZ(worldPosition, out x, out z);
            return GetGridObject(x, z);
        }

        public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
        {
            return new Vector2Int(
                Mathf.Clamp(gridPosition.x, 0, width - 1),
                Mathf.Clamp(gridPosition.y, 0, height - 1)
            );
        }

        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            int x = gridPosition.x;
            int z = gridPosition.y;

            if (x >= 0 && z >= 0 && x < width && z < height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsValidGridPositionWithPadding(Vector2Int gridPosition)
        {
            Vector2Int padding = new Vector2Int(2, 2);
            int x = gridPosition.x;
            int z = gridPosition.y;

            if (x >= padding.x && z >= padding.y && x < width - padding.x && z < height - padding.y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}