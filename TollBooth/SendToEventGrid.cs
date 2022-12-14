
namespace TollBooth;

public class SendToEventGrid
{
    private readonly HttpClient _client;
    private readonly ILogger _log;

    public SendToEventGrid( ILogger log, HttpClient client )
    {
        this._log = log;
        this._client = client;
    }

    public async Task SendLicensePlateDataAsync( LicensePlateData data, CancellationToken cancellationToken )
    {
        if( data is null )
        {
            throw new ArgumentNullException( nameof( data ), "Need license plate data first" );
        }
        // Will send to one of two routes, depending on success.
        // Event listeners will filter and act on events they need to
        // process (save to database, move to manual checkup queue, etc.)
        if( data.LicensePlateFound )
        {
            // TODO 3: Modify send method to include the proper eventType name value for saving plate data.
            // COMPLETE: await Send(...);
            await this.SendAsync( "savePlateData", "TollBooth/CustomerService", data, cancellationToken )
                      .ConfigureAwait( false );
        }
        else
        {
            // TODO 4: Modify send method to include the proper eventType name value for queuing plate for manual review.
            // COMPLETE: await Send(...);
            await this.SendAsync( "queuePlateForManualCheckup", "TollBooth/CustomerService", data, cancellationToken )
                      .ConfigureAwait( false );
        }
    }

    private async Task SendAsync( string eventType, string subject, LicensePlateData data, CancellationToken cancellationToken )
    {
        // Get the API URL and the API key from settings.
        string uri = Environment.GetEnvironmentVariable( "eventGridTopicEndpoint" );
        string key = Environment.GetEnvironmentVariable( "eventGridTopicKey" );

        this._log.LogInformation( $"Sending license plate data to the ({eventType}) Event Grid type" );

        List<LicensePlateProcessedEvent<LicensePlateData>> events = new List<LicensePlateProcessedEvent<LicensePlateData>>()
        {
            new LicensePlateProcessedEvent<LicensePlateData>()
            {
                Data = data,
                EventTime = DateTime.UtcNow,
                EventType = eventType,
                Id = Guid.NewGuid().ToString(),
                Subject = subject
            }
        };

        this._client.DefaultRequestHeaders.Clear();
        this._client.DefaultRequestHeaders.Add( "aeg-sas-key", key );
        _ = await this._client.PostAsJsonAsync( uri, events, cancellationToken )
                              .ConfigureAwait( false );

        this._log.LogInformation( $"Sent the following to the Event Grid topic: {events[0]}" );
    }
}
