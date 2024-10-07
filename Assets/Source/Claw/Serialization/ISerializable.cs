namespace Beast.Claw.Serialization
{
    public interface ISerializable
    {
        void Serialize(IBinaryWriter stream);

        void Deserialize(IBinaryReader stream);
    }
}