namespace Beast.Radar.Serialization
{
    public interface IBeastSerializableModule
    {
        void OnModuleSerialize(IBeastBlobStreamWriter writer);
        
        void OnModuleDeserialize(IBeastBlobStreamReader streamReader);
    }
}