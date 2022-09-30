// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

// Learn how to locally debug an Event Grid-triggered function:
//    https://aka.ms/AA30pjh

// Use for local testing:
//   https://{ID}.ngrok.io/runtime/webhooks/EventGrid?functionName=ProcessImage


namespace TollBooth;

public static class ProcessImage
{
    private static HttpClient _client;
    private static string GetBlobNameFromUrl( string bloblUrl )
    {
        Uri uri = new( bloblUrl );
        CloudBlob cloudBlob = new( uri );
        return cloudBlob.Name;
    }

    [FunctionName( "ProcessImage" )]
    public static async Task Run( [EventGridTrigger] EventGridEvent eventGridEvent,
        [Blob(blobPath: "{data.url}", access: FileAccess.Read,
            Connection = "blobStorageConnection")] Stream incomingPlate,
        ILogger log )
    {
        string licensePlateText = string.Empty;
        // Reuse the HttpClient across calls as much as possible so as not to exhaust all available sockets on the server on which it runs.
        _client ??= new HttpClient();

        try
        {
            if( incomingPlate != null )
            {
                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true
                };
                StorageBlobCreatedEventData createdEvent = eventGridEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>( options );
                string name = GetBlobNameFromUrl( createdEvent.Url );

                log.LogInformation( $"Processing {name}" );

                byte[] licensePlateImage;
                // Convert the incoming image stream to a byte array.
                using( BinaryReader br = new( incomingPlate ) )
                {
                    licensePlateImage = br.ReadBytes( (int) incomingPlate.Length );
                }

                // TODO 1: Set the licensePlateText value by awaiting a new FindLicensePlateText.GetLicensePlate method.
                // COMPLETE: licensePlateText = await new.....

                // Send the details to Event Grid.
                await new SendToEventGrid( log, _client ).SendLicensePlateData( new LicensePlateData()
                {
                    FileName = name,
                    LicensePlateText = licensePlateText,
                    TimeStamp = DateTime.UtcNow
                } ).ConfigureAwait( false );
            }
        }
        catch( Exception ex )
        {
            log.LogCritical( ex.Message );
            throw;
        }

        log.LogInformation( $"Finished processing. Detected the following license plate: {licensePlateText}" );
    }
}
