namespace Beast.Claw.Serialization
{
    public interface IBeastBinarySerializable
    {
        void Serialize(IBeastBinaryWriter stream);

        void Deserialize(IBeastBinaryReader stream);
    }
}