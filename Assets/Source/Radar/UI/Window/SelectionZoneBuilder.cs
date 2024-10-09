using Unity.Mathematics;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectionZoneBuilder : ISelectionZoneBuilder
    {
        public SelectableGridData Build(Selectable[] selectables, int2 size, bool isFillHorizontally, bool isLooping)
        {
            Selectable[][] selectionsGrid = isFillHorizontally
                ? new Selectable[size.y][]
                : new Selectable[size.x][];

            for (int i = 0; i < (isFillHorizontally ? size.y : size.x); i++)
            {
                selectionsGrid[i] = new Selectable[isFillHorizontally ? size.x : size.y];
            }

            int index = 0;
            for (int outer = 0; outer < (isFillHorizontally ? size.y : size.x); outer++)
            {
                for (int inner = 0; inner < (isFillHorizontally ? size.x : size.y); inner++)
                {
                    if (index < selectables.Length)
                    {
                        if (isFillHorizontally)
                        {
                            selectionsGrid[outer][inner] = selectables[index];
                        }
                        else
                        {
                            selectionsGrid[inner][outer] = selectables[index];
                        }

                        index++;
                    }
                    else
                    {
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

        public SelectableGridData Build(Selectable[] selectables,bool isFillHorizontally, bool isLooping)
        {
            Selectable[][] selectablesGrid;

            if (isFillHorizontally)
            {
                selectablesGrid = new Selectable[1][];
                selectablesGrid[0] = new Selectable[selectables.Length];
        
                for (int i = 0; i < selectables.Length; i++)
                {
                    selectablesGrid[0][i] = selectables[i];
                }
            }
            else
            {
                selectablesGrid = new Selectable[selectables.Length][];
        
                for (int i = 0; i < selectables.Length; i++)
                {
                    selectablesGrid[i] = new Selectable[1];
                    selectablesGrid[i][0] = selectables[i];
                }
            }

            return new SelectableGridData()
            {
                selectables = selectablesGrid,
                currentIndex = int2.zero,
                isLooping = isLooping
            };
        }
    }
}