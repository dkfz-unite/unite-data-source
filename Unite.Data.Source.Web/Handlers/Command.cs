using System.Diagnostics;

namespace Unite.Data.Source.Web.Handlers;

public static class Command
{
    public static async Task<string> Run(string path, params string[] args)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Command file not found: {path}");

        var process = PrepareProcess(path, args);

        return await RunProcess(process);
    }
    

    private static Process PrepareProcess(string path, params string[] args)
    {
        var process = new Process();

        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
        process.StartInfo.FileName = Path.GetFullPath(path);
        process.StartInfo.Arguments = string.Join(" ", args);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        return process;
    }

    private static async Task<string> RunProcess(Process process)
    {
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode > 0)
            throw new Exception(error);

        if (error.Length > 0)
            Console.Error.WriteLine(error);
            
        return output;
    }
}
