namespace Beast.Radar.GameSave
{
    public interface ISavedDataReader
    {
        void SaveData(SavedData savedData);
        void LoadData(SavedData savedData);
    }
}