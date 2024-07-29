using Unite.Data.Source.Web.Handlers;
using Unite.Essentials.Extensions;

namespace Unite.Data.Source.Web.Workers;

public class ExploringWorker : BackgroundService
{
    private const int _interval = 1 * 60 * 1000;
    
    private readonly ExploringHandler _handler;
    private readonly ILogger _logger;


    public ExploringWorker(
        ExploringHandler handler,
        ILogger<ExploringWorker> logger)
    {
        _handler = handler;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Exploring service started");

        stoppingToken.Register(() => _logger.LogInformation("Exploring service stopped"));

        // Delay 5 seconds to let the web api start working
        await Task.Delay(5000, stoppingToken);

        try
        {
            await _handler.Prepare();
        }
        catch (Exception exception)
        {
            _logger.LogError("{error}", exception.GetShortMessage());
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _handler.Handle();
            }
            catch (Exception exception)
            {
                _logger.LogError("{error}", exception.GetShortMessage());
            }
            finally
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
