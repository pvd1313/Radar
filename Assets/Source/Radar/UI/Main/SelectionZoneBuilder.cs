using Unity.Mathematics;
using UnityEditor;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectionZoneBuilder : ISelectionZoneBuilder
    {
        public SelectableGridData Build(Selectable[] selectables, int2 size, bool isLooping)
        {
            Selectable[,] selectionsGrid = new Selectable[size.x, size.y];
            
            int index = 0;
            bool elementsExhausted = false;

            for (int y = 0; y < size.y; y++)
            {
                if (elementsExhausted)
                {
                    break; 
                }
                for (int x = 0; x < size.x; x++)
                {
                    if (index < selectables.Length)
                    {
                        selectionsGrid[x, y] = selectables[index];
                        index++;
                    }
                    else
                    {
                        elementsExhausted = true;
                        break; 
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
        
        public SelectableGridData Build(Selectable[,] selectables, bool isLooping)
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