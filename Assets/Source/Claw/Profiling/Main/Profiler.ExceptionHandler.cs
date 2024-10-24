using System;
using System.Collections.Generic;

namespace Beast.Claw.Profiling
{
    partial class Profiler
    {
        private struct ExceptionHandler
        {
            public void HandleJobState(IList<string> names, BuildTreeJob job)
            {
                TreeBuildState state = job.outState[0];

                if (state.code == ETreeBuildCode.Success)
                {
                    return;
                }
                
                string stackTrace = _buildPath(names, job, state.stackLast);
                switch (state.code)
                {
                    case ETreeBuildCode.StackIsNotEmptyAfterJobComplete:
                        throw new InvalidOperationException($"EndSample() was not called for {stackTrace}");
                    
                    case ETreeBuildCode.StackOverflow:
                        throw new StackOverflowException($"{stackTrace}{names[state.recordLast + 1]} caused stack overflow");
                    
                    case ETreeBuildCode.StackUnderflow:
                        throw new InvalidOperationException($"BeginSample() is missing for {stackTrace}{names[state.recordLast + 1]}");
                    
                    case ETreeBuildCode.NameMismatch:
                        string newNodePath = _buildPath(names, job, state.stackLast - 1) + names[state.recordLast + 1];
                        throw new InvalidOperationException($"BeginSample({stackTrace}) does not match EndSample({newNodePath})");
                }

                throw new InvalidOperationException($"Error during tree building: {state.code}");
            }
            
            private static string _buildPath(IList<string> names, BuildTreeJob job, int stackLast)
            {
                string result = string.Empty;

                for (int i = 0; i <= stackLast; i++)
                {
                    result += names[job.outNames[job.parentStack[i]]] + "/";
                }

                return result;
            }
        }
    }
}