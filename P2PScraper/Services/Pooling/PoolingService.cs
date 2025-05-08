using Microsoft.Extensions.Logging;

namespace P2PScraper.Services.Pooling;

using P2PScraper.Services.Receiver;

// Compose Polling and ReceiverService implementations
public class PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger)
    : PollingServiceBase<ReceiverService>(serviceProvider, logger);