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
            var value = Environment.GetEnvironmentVariable("UNITE_FEED_DONORS_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_FEED_DONORS_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/feed-donors";
            }
                

            return $"{value}/api";
        }
    }

    public string ImagesHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_FEED_IMAGES_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_FEED_IMAGES_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/feed-images";
            }

            return $"{value}/api";
        }
    }

    public string SpecimensHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_FEED_SPECIMENS_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_FEED_SPECIMENS_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/feed-specimens";
            }

            return $"{value}/api";
        }
    }

    public string OmicsHost
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("UNITE_FEED_OMICS_HOST");

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!MainHostIsSet)
                    throw new ArgumentNullException("'UNITE_FEED_OMICS_HOST' environment variable has to be set");
                else
                    return $"{PortalHost}/api/feed-omics";
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
                   !string.IsNullOrWhiteSpace(OmicsHost);
        }
    }
}
