namespace Unite.Data.Source.Web.Handlers.Contract.Extensions;

public static class FormExtensions
{
    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string fieldName, string fieldValue)
    {
        if (string.IsNullOrEmpty(fieldValue))
            return form;

        form.Add(new StringContent(fieldValue), fieldName);
        return form;
    }

    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string fieldName, string fileName, Stream fileStream)
    {
        if (fileStream == null)
            return form;

        form.Add(new StreamContent(fileStream), fieldName, fileName);
        return form;
    }
}
