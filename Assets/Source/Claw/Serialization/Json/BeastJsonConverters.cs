using System;
using Beast.Claw.Common;
using Newtonsoft.Json;

namespace Beast.Claw.Serialization.Json
{
    public static class BeastJsonConverters
    {
        public static readonly JsonConverter[] All;
        
        static BeastJsonConverters()
        {
            try
            {
                All = new JsonConverter [] 
                {
                    new BeastJsonFloat2Converter(),
                    new BeastJsonFloat3Converter(),
                    new BeastJsonFloat4Converter(),
                    new BeastJsonQuaternionConverter()
                };
            }
            catch (Exception e)
            {
                BDebug.LogStaticException(e, $"{nameof(BeastJsonConverters)} static cctor");
            }
        }
    }
}