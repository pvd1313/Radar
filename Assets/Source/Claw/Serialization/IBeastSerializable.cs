namespace Beast.Claw.Serialization
{
    public interface IBeastSerializable
    {
        void Serialize(IBeastBinaryWriter stream);

        void Deserialize(IBeastBinaryReader stream);
    }
}