namespace Unite.Data.Source.Web.Configuration.Options;

public class FeedOptions
{
    public string PortalHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_PORTAL_HOST");

            if (string.IsNullOrWhiteSpace(value) && !CustomHostsAreSet)
                throw new ArgumentNullException($"'UNITE_PORTAL_HOST' environment variable has to be set");

            return value;
        }
    }

    public string DonorsHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_DONORS_FEED_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_DONORS_FEED_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/donors-feed";
            }
                

            return $"{value}/api";
        }
    }

    public string ImagesHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_IMAGES_FEED_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_IMAGES_FEED_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/images-feed";
            }

            return $"{value}/api";
        }
    }

    public string SpecimensHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_SPECIMENS_FEED_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_SPECIMENS_FEED_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/specimens-feed";
            }

            return $"{value}/api";
        }
    }

    public string GenomeHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_GENOME_FEED_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_GENOME_FEED_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/genome-feed";
            }

            return $"{value}/api";
        }
    }


    private bool MainHostIsSet
    {
        get
        {
            return !string.IsNullOrWhiteSpace(PortalHost);
        }
    }

    private bool CustomHostsAreSet
    {
        get
        {
            return !string.IsNullOrWhiteSpace(DonorsHost) &&
                   !string.IsNullOrWhiteSpace(ImagesHost) &&
                   !string.IsNullOrWhiteSpace(SpecimensHost) &&
                   !string.IsNullOrWhiteSpace(GenomeHost);
        }
    }
}
