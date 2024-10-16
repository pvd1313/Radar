using Beast.Claw.Debug;
using Beast.Claw.Serialization;

namespace Beast.Radar.Debug
{
    public class DebugPreferences : ISerializable
    {
        public float
            playerPlayTimeThanShouldBeDefinitelyStoredNotHereAndBeWithAMuchShorterNameAsShortNamesAreMuchEasierToReadAndRemember;
        
        public void Serialize(IBinaryWriter stream)
        {
            stream.WriteFloat(
                playerPlayTimeThanShouldBeDefinitelyStoredNotHereAndBeWithAMuchShorterNameAsShortNamesAreMuchEasierToReadAndRemember);
        }

        public void Deserialize(IBinaryReader stream)
        {
            playerPlayTimeThanShouldBeDefinitelyStoredNotHereAndBeWithAMuchShorterNameAsShortNamesAreMuchEasierToReadAndRemember =
                stream.ReadFloat();
        }
    }
}