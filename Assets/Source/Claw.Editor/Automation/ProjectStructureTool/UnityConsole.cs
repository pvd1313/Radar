namespace Beast.Claw.Editor.Automation.ProjectStructureTool
{
    internal class UnityConsole : IConsole
    {
        public UnityConsole(bool clearConsole)
        {
            if (clearConsole)
            {
                ConsoleUtil.ClearLog();
            }
        }
        
        public void WriteLine(string text)
        {
            UnityEngine.Debug.Log(text);
        }
    }
}