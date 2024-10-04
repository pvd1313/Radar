using System.IO;
using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using Beast.Claw.Serialization;
using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    public struct CameraControllerState : IBeastBinarySerializable
    {
        public float zoom01;
        public float2 position;

        public override string ToString()
        {
            return $"{{zoom01: {zoom01:N3}; position: ({position.x:N0}, {position.y:N0})}}";
        }

        public void Serialize(IBeastBinaryWriter stream)
        {
            stream.WriteFloat(zoom01);
            stream.WriteFloat2(position);
        }

        public void Deserialize(IBeastBinaryReader stream)
        {
            zoom01 = stream.ReadFloat();
            position = stream.ReadFloat2();
        }

        /// <summary>
        /// Only for test purposes. Can be removed once not used.
        /// </summary>
        public static bool AreSame(CameraControllerState a, CameraControllerState b)
        {
            return BMath.Approximately(a.zoom01, b.zoom01) &&
                   BMath.Approximately(a.position, b.position);
        }
    }
}