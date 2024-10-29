namespace Beast.Radar.Serialization
{
    public interface IFileManager
    {
        bool Exists(string folderName, string fileName);
        
        StreamData OpenToRead(string folderName, string fileName);
        
        StreamData OpenToWrite(string folderName, string fileName);
        
        void Close(StreamData stream);
    }
}