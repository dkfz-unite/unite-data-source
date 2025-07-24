namespace Unite.Data.Source.Web.Handlers.Contract.Extensions;

public static class FormExtensions
{
    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string name, string value)
    {
        form.Add(new StringContent(value), name);
        return form;
    }

    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string name, DateTime? value)
    {
        form.Add(new StringContent(value?.ToString("yyyy-MM-dd") ?? null), name);
        return form;
    }

    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string name, DateOnly? value)
    {
        form.Add(new StringContent(value?.ToString("yyyy-MM-dd") ?? null), name);
        return form;
    }

    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string name, int? value)
    {
        form.Add(new StringContent(value?.ToString() ?? null), name);
        return form;
    }

    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string name, double? value)
    {
        form.Add(new StringContent(value?.ToString() ?? null), name);
        return form;
    }

    public static MultipartFormDataContent AddField(this MultipartFormDataContent form, string name, Stream value, string fileName)
    {
        form.Add(new StreamContent(value), name, fileName);
        return form;
    }
}
