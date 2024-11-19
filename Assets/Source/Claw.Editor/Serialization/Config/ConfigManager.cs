using System;
using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.Debug;
using Beast.Claw.Text;

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
            StringBuilder str = BUILDER_POOL.Next().Clear();

            Type configType = typeof(TConfig);

            str = str + "'" + configType.FullName + "' was not found. ";
            str.Line().Tab();
            str = str + "Please, create one and reload domain.";
            str.Line().Tab().Tab();
            str = str + "Asset > Create > Fancy > " + configType.Name;

            return str.ToString();
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
                StringBuilder str = BUILDER_POOL.Next().Clear();

                str = str + "ConfigManager: Found more than one config of type '" + configType.FullName + "'";
                str.Line();
                
                foreach (AssetInfo asset in assetList)
                {
                    str = str + asset.guid + " '" + asset.path + "'";
                    str.Line();
                }
                
                BDebug.StaticLogWarning(str.ToString());
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