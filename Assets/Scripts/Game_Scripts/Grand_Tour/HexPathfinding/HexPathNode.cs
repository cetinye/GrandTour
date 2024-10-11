namespace GrandTour
{
    public class HexPathNode
    {
        private GridHexXZ<HexPathNode> grid;
        public int x;
        public int y;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;
        public HexPathNode cameFromNode;

        public HexPathNode(GridHexXZ<HexPathNode> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            isWalkable = true;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
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