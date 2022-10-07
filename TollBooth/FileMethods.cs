
namespace TollBooth;

internal class FileMethods
{
    private readonly CloudBlobClient _blobClient;
    private readonly string _containerName = Environment.GetEnvironmentVariable( "exportCsvContainerName" );
    private readonly string _blobStorageConnection = Environment.GetEnvironmentVariable( "blobStorageConnection" );
    private readonly ILogger _log;

    public FileMethods( ILogger log )
    {
        this._log = log;
        // Retrieve storage account information from connection string.
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse( this._blobStorageConnection );

        // Create a blob client for interacting with the blob service.
        this._blobClient = storageAccount.CreateCloudBlobClient();
    }

    public async Task<bool> GenerateAndSaveCsvAsync( IEnumerable<LicensePlateDataDocument> licensePlates )
    {
        bool successful = false;

        this._log.LogInformation( "Generating CSV file" );
        string blobName = $"{DateTime.UtcNow:s}.csv";

        using( MemoryStream stream = new MemoryStream() )
        {
            using( StreamWriter textWriter = new StreamWriter( stream ) )
            {
                CsvConfiguration csvConfiguration = new CsvConfiguration( CultureInfo.CurrentCulture )
                {
                    Delimiter = ","
                };

                using( CsvWriter csv = new CsvWriter( textWriter, csvConfiguration ) )
                {
                    csv.WriteRecords( licensePlates.Select( ToLicensePlateData ) );
                    await textWriter.FlushAsync().ConfigureAwait( false );

                    this._log.LogInformation( $"Beginning file upload: {blobName}" );
                    try
                    {
                        CloudBlobContainer container = this._blobClient.GetContainerReference( this._containerName );

                        // Retrieve reference to a blob.
                        CloudBlockBlob blob = container.GetBlockBlobReference( blobName );
                        _ = await container.CreateIfNotExistsAsync().ConfigureAwait( false );

                        // Upload blob.
                        stream.Position = 0;

                        // TODO 7: Asynchronously upload the blob from the memory stream.
                        await blob.UploadFromStreamAsync( stream ).ConfigureAwait( false );

                        successful = true;
                    }
                    catch( Exception e )
                    {
                        this._log.LogCritical( $"Could not upload CSV file: {e.Message}", e );
                        successful = false;
                    }
                }
            }
        }

        return successful;
    }

    /// <summary>
    /// Used for mapping from a LicensePlateDataDocument object to a LicensePlateData object.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private static LicensePlateData ToLicensePlateData( LicensePlateDataDocument source )
    {
        return new LicensePlateData
        {
            FileName = source.FileName,
            LicensePlateText = source.LicensePlateText,
            TimeStamp = source.TimeStamp
        };
    }
}
