using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.Profiling
{
    [System.Serializable]
    public class ProfilerConfig : IConfig, IValidatable
    {
        private const int MAX_RECORD_COUNT = (1 << 16) - 2;

        public bool AnyProfilingActive => enableClawProfiling || enableUnityProfiling;

        public bool logOperationProfiling;
        public bool enableClawProfiling;
        public bool enableUnityProfiling;
        public int maxRecordCount;
        public int stackSize;

        public ProfilerConfig(bool enableClawProfiling, bool enableUnityProfiling, int maxRecordCount, int stackSize)
        {
            this.enableClawProfiling = enableClawProfiling;
            this.enableUnityProfiling = enableUnityProfiling;
            this.maxRecordCount = maxRecordCount;
            this.stackSize = stackSize;
        }
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }

        public string Validate()
        {
            if (maxRecordCount < 0 || maxRecordCount > MAX_RECORD_COUNT)
            {
                return $"maxRecordCount must be in range [0, {MAX_RECORD_COUNT}], but was " + maxRecordCount;
            }

            if (stackSize < 0)
            {
                return $"stackSize must be more or equal zero, but was " + stackSize;
            }

            return null;
        }
    }
}