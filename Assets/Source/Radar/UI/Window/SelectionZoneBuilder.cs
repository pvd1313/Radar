using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectionZoneBuilder : ISelectionZoneBuilder
    {
        public SelectableGridData Build(Selectable[] selectables, int rows, bool isFillHorizontally, bool isLooping)
        {
            List<Selectable[]> selectionsGrid = new List<Selectable[]>();

            int totalItems = selectables.Length;
            int index = 0;

            if (isFillHorizontally)
            {
                for (int i = 0; i < rows; i++)
                {
                    Selectable[] column = new Selectable[totalItems / rows + (i < totalItems % rows ? 1 : 0)];

                    for (int j = 0; j < column.Length; j++)
                    {
                        int itemIndex = i + j * rows;
                        if (itemIndex < totalItems)
                        {
                            column[j] = selectables[itemIndex];
                        }
                    }

                    selectionsGrid.Add(column);
                }
            }
            else
            {
                while (index < totalItems)
                {
                    int itemsInThisColumn = Math.Min(rows, totalItems - index);
                    Selectable[] column = new Selectable[itemsInThisColumn];

                    Array.Copy(selectables, index, column, 0, itemsInThisColumn);
                    selectionsGrid.Add(column);

                    index += itemsInThisColumn;
                }
            }

            return new SelectableGridData()
            {
                selectables = selectionsGrid.ToArray(), 
                currentIndex = int2.zero,
                isLooping = isLooping
            };
        }

        public SelectableGridData Build(Selectable[] selectables, bool isFillHorizontally, bool isLooping)
        {
            List<Selectable[]> selectablesGrid = new List<Selectable[]>();
       
            if (isFillHorizontally)
            {
              
                selectablesGrid.Add(selectables);
              
            }
            else
            {
                foreach (var selectable in selectables)
                {
                    selectablesGrid.Add(new[] { selectable });
                }
            }

            return new SelectableGridData()
            {
                selectables = selectablesGrid.ToArray(),
                currentIndex = int2.zero,
                isLooping = isLooping
            };
        }

        public SelectableGridData Build(Selectable[][] selectables, bool isLooping)
        {
            return new SelectableGridData()
            {
                selectables = selectables,
                currentIndex = int2.zero,
                isLooping = isLooping
            };
        }
    }
}