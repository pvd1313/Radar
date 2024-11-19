using System;
using System.Collections.Generic;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using Beast.Claw.Editor.Validation;
using Beast.Claw.Text;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor.Serialization
{
    public static class ConfigManager
    {
        private static readonly Dictionary<Type, ScriptableConfig> CONFIGS;
        private static readonly RingPool<StringBuilder> BUILDER_POOL;
        private static readonly RingPoolList<AssetInfo> ASSET_LIST_POOL;
        
        static ConfigManager()
        {
            try
            {
                CONFIGS = new Dictionary<Type, ScriptableConfig>();
                BUILDER_POOL = new RingPool<StringBuilder>(8, () => new StringBuilder(512));
                ASSET_LIST_POOL = new RingPoolList<AssetInfo>(8, () => new List<AssetInfo>(8));
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static string MissingConfigMessage<TConfig>()
        {
            StringBuilder builder = BUILDER_POOL.Next().Clear();

            Type configType = typeof(TConfig);

            builder += "'" + configType.FullName + "' was not found. ";
            builder.Line().Tab();
            builder += "Please, create one and reload domain.";
            builder.Line().Tab().Tab();
            builder += "Asset > Create > Fancy > " + configType.Name;

            return builder.ToString();
        }

        public static bool FindConfig<TConfig>(out TConfig config) where TConfig : ScriptableConfig
        {
            Type configType = typeof(TConfig);
            
            if (CONFIGS.TryGetValue(configType, out ScriptableConfig cachedConfig))
            {
                if (cachedConfig != null)
                {
                    config = (TConfig)cachedConfig;
                    return true;
                }

                CONFIGS.Remove(configType);
            }

            List<AssetInfo> assetList = ASSET_LIST_POOL.Next();
            assetList.Clear();

            BAsset.FindAssets<TConfig>(assetList);
            
            if (assetList.Count > 1)
            {
                StringBuilder builder = BUILDER_POOL.Next().Clear();

                builder += "ConfigManager: Found more than one config of type '" + configType.FullName + "'";
                builder.Line();
                
                foreach (AssetInfo asset in assetList)
                {
                    builder += asset.guid + " '" + asset.path + "'";
                    builder.Line();
                }
                
                BDebug.StaticLogWarning(builder.ToString());
            }
            
            if (assetList.Count > 0)
            {
                config = (TConfig)assetList[0].asset;
                CONFIGS[configType] = config;
                
                return true;
            }

            config = null;
            return false;
        }
    }
}