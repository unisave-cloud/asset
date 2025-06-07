namespace Unisave
{
    /// <summary>
    /// Contains metadata about the Unisave asset
    /// </summary>
    public static class AssetMeta
    {
        /// <summary>
        /// Version of the asset
        /// </summary>
        public static readonly string Version = "0.14.1";
        
        /// <summary>
        /// UTM link tracking parameters when redirecting to the website
        /// </summary>
        public static readonly string LinkUtmParams
            = "utm_source=unisave&utm_medium=unisave_asset";
    }
}