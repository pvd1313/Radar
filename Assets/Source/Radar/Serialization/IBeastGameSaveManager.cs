namespace Beast.Radar.Serialization
{
    public interface IBeastGameSaveManager
    {
        void Load(string folderName, string fileName);
        
        void Save(string folderName, string fileName);
    }
}