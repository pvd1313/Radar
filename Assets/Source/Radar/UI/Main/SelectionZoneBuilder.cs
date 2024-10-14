using System;
using Unity.Mathematics;

namespace Beast.Radar.UI
{
    public class SelectionZoneBuilder : ISelectionZoneBuilder
    {
        public Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, 
            UISelectable[] selectables,
            int cols, 
            int rows, 
            bool isFillHorizontally, 
            bool isLooping)
        {
            UISelectable[,] selectionsGrid = new UISelectable[rows, cols];
            
            int index = 0;
            for (int y = 0; y < (isFillHorizontally ? rows : cols); y++)
            for (int x = 0; x < (isFillHorizontally ? cols : rows); x++)
            {
                if (index < selectables.Length)
                {
                    UISelectable selectable = selectables[index]; 
                    selectable.SetIndex(new int2(x, y));
                    selectable.SetZone(zone);
                    selectionsGrid[x, y] = selectable;
                    index++;
                }
                else
                {
                    selectionsGrid[x, y] = null;
                }
            }
            
            return new Tuple<ESelectionZone, SelectableGridData>(zone, new SelectableGridData()
            {
                selectables = selectionsGrid,
                currentIndex = int2.zero,
                isLooping = isLooping
            });
        }

        public Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone,  UISelectable[] selectables, bool isFillHorizontally, bool isLooping)
        {
            UISelectable[,] selectionsGrid;

            if (isFillHorizontally)
            {
                selectionsGrid = new UISelectable[1, selectables.Length];
                for (int col = 0; col < selectables.Length; col++)
                {
                    UISelectable selectable = selectables[col];
                    selectable.SetIndex(new int2(0, col));
                    selectable.SetZone(zone);
                    selectionsGrid[0, col] = selectable;
                }
            }
            else
            {
                selectionsGrid = new UISelectable[selectables.Length, 1];
                for (int row = 0; row < selectables.Length; row++)
                {
                    UISelectable selectable = selectables[row];
                    selectable.SetIndex(new int2(row, 0));
                    selectable.SetZone(zone);
                    selectionsGrid[row, 0] = selectable;
                }
            }

            return new Tuple<ESelectionZone, SelectableGridData>(zone, new SelectableGridData()
            {
                selectables = selectionsGrid,
                currentIndex = int2.zero,
                isLooping = isLooping
            });
        }
        
        public Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable[,] selectionsGrid, bool isLooping)
        {
            int cols = selectionsGrid.GetLength(0);
            int rows = selectionsGrid.GetLength(1);
            for (int x = 0; x < rows; x++)
            for (int y = 0; y < cols; y++)
            {
                UISelectable uiSelectable = selectionsGrid[y,x];
                if (uiSelectable == null)
                {
                    continue;
                }
                uiSelectable.SetIndex(new int2(y, x));
                uiSelectable.SetZone(zone);
            }
            return new Tuple<ESelectionZone, SelectableGridData>(zone, new SelectableGridData()
            {
                selectables = selectionsGrid,
                currentIndex = int2.zero,
                isLooping = isLooping
            });
        }

        public Tuple<ESelectionZone,SelectableGridData> Build(
            ESelectionZone zone, UISelectable selectable)
        {
            selectable.SetIndex(int2.zero);
            selectable.SetZone(zone);
            return new Tuple<ESelectionZone, SelectableGridData>(zone, new SelectableGridData()
            {
                selectables = new[,] { { selectable } },
                currentIndex = int2.zero,
                isLooping = false
            });
        }
        
    }
}