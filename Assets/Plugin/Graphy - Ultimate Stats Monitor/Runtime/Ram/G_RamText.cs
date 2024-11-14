/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@martinTayx)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            05-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using UnityEngine;
using UnityEngine.UI;
using Tayx.Graphy.Utils.NumString;

namespace Tayx.Graphy.Ram
{
    public class G_RamText : MonoBehaviour
    {
        #region Variables -> Serialized Private

        [SerializeField] private Text m_allocatedSystemMemorySizeText = null;
        [SerializeField] private Text m_reservedSystemMemorySizeText = null;
        [SerializeField] private Text m_monoSystemMemorySizeText = null;

        #endregion

        #region Variables -> Private

        private GraphyManager m_graphyManager = null;

        private G_RamMonitor m_ramMonitor = null;

        private float m_updateRate = 4f; // 4 updates per sec.

        private float m_deltaTime = 0.0f;

        #endregion

        #region Methods -> Unity Callbacks

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            m_deltaTime += Time.unscaledDeltaTime;

            if( m_deltaTime > 1f / m_updateRate )
            {
                // Update allocated, mono and reserved memory
                m_allocatedSystemMemorySizeText.text = m_ramMonitor.AllocatedRam.ToIntStringNonAlloc();
                m_reservedSystemMemorySizeText.text = m_ramMonitor.ReservedRam.ToIntStringNonAlloc();
                m_monoSystemMemorySizeText.text = m_ramMonitor.MonoRam.ToIntStringNonAlloc();

                m_deltaTime = 0f;
            }
        }

        #endregion

        #region Methods -> Public

        public void UpdateParameters()
        {
            m_allocatedSystemMemorySizeText.color = m_graphyManager.AllocatedRamColor;
            m_reservedSystemMemorySizeText.color = m_graphyManager.ReservedRamColor;
            m_monoSystemMemorySizeText.color = m_graphyManager.MonoRamColor;

            m_updateRate = m_graphyManager.RamTextUpdateRate;
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            // We assume no game will consume more than 16GB of RAM.
            // If it does, who cares about some minuscule garbage allocation lol.
            G_IntString.EnsureRange( 0, 16386 );

            m_graphyManager = transform.root.GetComponentInChildren<GraphyManager>();

            m_ramMonitor = GetComponent<G_RamMonitor>();

            UpdateParameters();
        }

        #endregion
    }
}