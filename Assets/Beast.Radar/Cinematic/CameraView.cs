using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public class CameraView: BeastMono
    {
        public Camera camera => _camera;
        
        [SerializeField] private Camera _camera;
    }
}