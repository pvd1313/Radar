namespace Beast.Claw.Graphy.Modules
{
    public struct InformationData
    {
        public string graphicsDeviceVersion;
        public string processorType;
        public string systemMemory;
        public string graphicsDeviceName;
        public string graphicsMemorySize;
        public string screenResolution;
        public string gameWindowResolution;
        public string operatingSystem;
        
        public InformationData UpdateWindowResolution(string value)
        {
            InformationData updatedData = this;
            
            updatedData.gameWindowResolution = value;
            
            return updatedData;
        }
    }
}