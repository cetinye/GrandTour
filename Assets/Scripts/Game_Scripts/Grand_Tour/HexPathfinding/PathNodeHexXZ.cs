﻿namespace GrandTour
{
    public class PathNodeHexXZ
    {
        private GridHexXZ<PathNodeHexXZ> grid;
        public int x;
        public int y;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable = false;
        public PathNodeHexXZ cameFromNode;

        public PathNodeHexXZ(GridHexXZ<PathNodeHexXZ> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            isWalkable = true;
        }

        public void CalculateFCost()
        {
            // fCost = gCost + hCost + HexController.instance.gridHexXZ.GetGridObject(x, y).tileWeight;
            fCost = HexController.instance.gridHexXZ.GetGridObject(x, y).tileWeight;
        }

        public int GetTCost()
        {
            return HexController.instance.gridHexXZ.GetGridObject(x, y).tileWeight;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
            grid.TriggerGridObjectChanged(x, y);
        }

        public override string ToString()
        {
            return x + "," + y;
        }
    }
}