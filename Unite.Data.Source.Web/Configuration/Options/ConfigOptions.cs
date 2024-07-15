using Unite.Data.Source.Web.Extensions;

namespace Unite.Data.Source.Web.Configuration.Options;

public class ConfigOptions
{
    /// <summary>
    /// Path to the configuration directory.
    /// </summary>
    public string ConfigPath
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_CONFIG_PATH");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("'UNITE_CONFIG_PATH' environment variable has to be set");

            return value.AbsolutePath();
        }
    }

    /// <summary>
    /// Path to the cache directory.
    /// </summary>
    public string CachePath
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_CACHE_PATH");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("'UNITE_CACHE_PATH' environment variable has to be set");

            return value.AbsolutePath();
        }
    }

    /// <summary>
    /// Path to the data directory.
    /// </summary>
    public string DataPath
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_DATA_PATH");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("'UNITE_DATA_PATH' environment variable has to be set");

            return value.AbsolutePath();
        }
    }
}
