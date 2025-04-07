using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Validation;

namespace Beast.Claw.Profiling
{
    [Serializable]
    public class BricksConfig : IConfig, IValidatable
    {
        private const int MAX_RECORD_COUNT = (1 << 16) - 2;

        public bool bricksEnabled;
        public int maxRecordCount;
        public int stackSize;

        // For Validator.
        private BricksConfig()
        {
            
        }

        public BricksConfig(bool bricksEnabled, int maxRecordCount, int stackSize)
        {
            this.bricksEnabled = bricksEnabled;
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