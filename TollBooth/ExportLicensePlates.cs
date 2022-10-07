
namespace TollBooth;

public static class ExportLicensePlates
{
    [FunctionName( "ExportLicensePlates" )]
    public static async Task<HttpResponseMessage> Run( [HttpTrigger( AuthorizationLevel.Function, "get", "post", Route = null )] HttpRequestMessage req, ILogger log )
    {
        int exportedCount = 0;
        log.LogInformation( "Finding license plate data to export" );
        CancellationToken cancellationToken = CancellationToken.None;

        DatabaseMethods databaseMethods = new DatabaseMethods( log );
        List<LicensePlateDataDocument> licensePlates = await databaseMethods.GetLicensePlatesToExportAsync( cancellationToken ).ConfigureAwait( false );
        if( licensePlates.Any() )
        {
            log.LogInformation( $"Retrieved {licensePlates.Count} license plates" );
            FileMethods fileMethods = new FileMethods( log );
            bool uploaded = await fileMethods.GenerateAndSaveCsvAsync( licensePlates ).ConfigureAwait( false );
            if( uploaded )
            {
                await databaseMethods.MarkLicensePlatesAsExportedAsync( licensePlates, cancellationToken ).ConfigureAwait( false );
                exportedCount = licensePlates.Count;
                log.LogInformation( "Finished updating the license plates" );
            }
            else
            {
                log.LogInformation( "Export file could not be uploaded. Skipping database update that marks the documents as exported." );
            }

            log.LogInformation( $"Exported {exportedCount} license plates" );
        }
        else
        {
            log.LogWarning( "No license plates to export" );
        }

        return exportedCount == 0
            ? req.CreateResponse( HttpStatusCode.NoContent )
            : req.CreateResponse( HttpStatusCode.OK, $"Exported {exportedCount} license plates" );
    }
}
