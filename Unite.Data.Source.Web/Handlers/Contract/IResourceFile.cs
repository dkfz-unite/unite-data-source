namespace Unite.Data.Source.Web.Handlers.Contract;

public interface IResourceFile
{
    string Path { get; }


    MultipartFormDataContent AsForm();
    Resource AsResource(string type, string url);
}
