/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@martinTayx)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            15-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/
using UnityEngine;

namespace Tayx.Graphy.Fps
{
    public class G_FpsMonitor : MonoBehaviour
    {
        private const int MAX_SAMPLE_COUNT = 1024;
        
        public short CurrentFPS => (short)_currentFPS;
        public short AverageFPS => (short)_averageFPS;
        
        private int _collectedCount;
        
        private float _currentFPS;
        private double _averageFPS;

        private void Update()
        {
            _currentFPS = 1f / Time.unscaledDeltaTime;
            
            _averageFPS = (_averageFPS * _collectedCount + _currentFPS) / (_collectedCount + 1);

            _collectedCount = Mathf.Min(_collectedCount + 1, MAX_SAMPLE_COUNT);
        }
    }
}