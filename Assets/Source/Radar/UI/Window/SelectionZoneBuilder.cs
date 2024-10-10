using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectionZoneBuilder : ISelectionZoneBuilder
    {
        public SelectableGridData Build(Selectable[] selectables, int cols, int rows, bool isFillHorizontally, bool isLooping)
        {
            Selectable[,] selectionsGrid = new Selectable[rows, cols];
            int index = 0;
            
            if (isFillHorizontally)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (index < selectables.Length)
                        {
                            selectionsGrid[c, r] = selectables[index];
                            index++;
                        }
                        else
                        {
                            selectionsGrid[c, r] = null;
                        }
                    }
                }
            }
            else
            {
                for (int c = 0; c < cols; c++)
                {
                    for (int r = 0; r < rows; r++)
                    {
                        if (index < selectables.Length)
                        {
                            selectionsGrid[r, c] = selectables[index];
                            index++;
                        }
                        else
                        {
                            selectionsGrid[r, c] = null;
                        }
                    }
                }
            }

            return new SelectableGridData()
            {
                selectables = selectionsGrid,
                currentIndex = int2.zero,
                isLooping = isLooping
            };
        }

        public SelectableGridData Build(Selectable[] selectables, bool isFillHorizontally, bool isLooping)
        {
            Selectable[,] selectionsGrid;

            if (isFillHorizontally)
            {
                selectionsGrid = new Selectable[1, selectables.Length];
                for (int col = 0; col < selectables.Length; col++)
                {
                    selectionsGrid[0, col] = selectables[col];
                }
            }
            else
            {
                selectionsGrid = new Selectable[selectables.Length, 1];
                for (int row = 0; row < selectables.Length; row++)
                {
                    selectionsGrid[row, 0] = selectables[row];
                }
            }

            return new SelectableGridData()
            {
                selectables = selectionsGrid,
                currentIndex = int2.zero,
                isLooping = isLooping
            };
        }

        public SelectableGridData Build(Selectable[][] selectables, bool isLooping)
        {
            throw new System.NotImplementedException();
        }

        public SelectableGridData Build(Selectable[,] selectables, bool isLooping)
        {
            return new SelectableGridData()
            {
                selectables = selectables,
                currentIndex = int2.zero,
                isLooping = isLooping
            };
        }

        public SelectableGridData Build(Selectable selectables)
        {
            return new SelectableGridData()
            {
                selectables = new[,]{{selectables}},
                currentIndex = int2.zero,
                isLooping = false
            };
        }
    }
}