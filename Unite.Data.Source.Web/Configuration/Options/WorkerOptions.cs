namespace Unite.Data.Source.Web.Configuration.Options;

public class WorkerOptions
{
    /// <summary>
    /// Public host of the current application (e.g. 'http://unite-data-source').
    /// </summary>
    /// <value></value>
    public string Host
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_WORKER_HOST");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("'UNITE_WORKER_HOST' environment variable has to be set");

            return value;
        }
    }

    /// <summary>
    /// Token for the worker to authenticate with the main portal.
    /// </summary>
    public string Token
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_WORKER_TOKEN");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("'UNITE_WORKER_TOKEN' environment variable has to be set");

            return value;
        }
    }
}
