namespace Beast.Claw.Common
{
    public interface IProgressTrackerListener
    {
        void UpdateProgress(ProgressTrackerInfo info);
    }
}