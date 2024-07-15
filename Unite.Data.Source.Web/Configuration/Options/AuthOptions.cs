using System.Text;

namespace Unite.Data.Source.Web.Configuration.Options;

public class AuthOptions
{
    public byte[] Key
    {
        get
        {
            var key = Environment.GetEnvironmentVariable("UNITE_AUTH_KEY");

            if (key == null)
                throw new ArgumentNullException("'UNITE_AUTH_KEY' environment variable has to be set");

            if (key.Length != 32)
                throw new ArgumentOutOfRangeException("'UNITE_AUTH_KEY' environment variable has to be a 32 bit string");

            return Encoding.ASCII.GetBytes(key);
        }
    }
}
