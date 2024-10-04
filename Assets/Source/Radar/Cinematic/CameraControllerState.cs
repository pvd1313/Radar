using System.IO;
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

        public void BinarySerialize(BinaryWriter stream)
        {
            // TODO: This is very bad. It is not obvious what types are serialized. We need our own interface...
            stream.Write(zoom01);
            stream.Write(position.x);
            stream.Write(position.y);
        }

        public void BinaryDeserialize(BinaryReader stream)
        {
            zoom01 = stream.ReadSingle();
            position.x = stream.ReadSingle();
            position.y = stream.ReadSingle();
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