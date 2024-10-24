namespace Beast.Claw.Jobs
{
    public struct ArrayMaintenanceData
    {
        public int requiredCapacity;
        public int itemPerAllocation;
        public bool copyOnResize;
    }
}