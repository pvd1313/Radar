namespace Beast.Claw.Graphy.Modules
{
    public struct AdvancedData
    {
        public string graphicsDeviceVersion;
        public string processorType;
        public string systemMemory;
        public string graphicsDeviceName;
        public string graphicsMemorySize;
        public string screenResolution;
        public string gameWindowResolution;
        public string operatingSystem;
        
        public AdvancedData UpdateWindowResolution(string value)
        {
            AdvancedData updatedData = this;
            
            updatedData.gameWindowResolution = value;
            
            return updatedData;
        }
    }
}