namespace TollBooth;

internal class DatabaseMethods
{
    private readonly string _endpointUrl = Environment.GetEnvironmentVariable( "cosmosDBEndPointUrl" );
    private readonly string _authorizationKey = Environment.GetEnvironmentVariable( "cosmosDBAuthorizationKey" );
    private readonly string _databaseId = Environment.GetEnvironmentVariable( "cosmosDBDatabaseId" );
    private readonly string _collectionId = Environment.GetEnvironmentVariable( "cosmosDBCollectionId" );
    private readonly ILogger _log;

    public DatabaseMethods( ILogger log )
    {
        this._log = log;
    }

    /// <summary>
    /// Retrieves all license plate records (documents) that have not yet been exported.
    /// </summary>
    /// <returns></returns>
    public async Task<List<LicensePlateDataDocument>> GetLicensePlatesToExportAsync( CancellationToken cancellationToken )
    {
        this._log.LogInformation( "Retrieving license plates to export" );
        List<LicensePlateDataDocument> licensePlates = new List<LicensePlateDataDocument>();

        using( CosmosClient client = new CosmosClient( this._endpointUrl, this._authorizationKey ) )
        {
            Database database = client.GetDatabase( this._databaseId );
            Container container = database.GetContainer( this._collectionId );

            // MaxItemCount value tells the document query to retrieve 100 documents at a time until all are returned.
            // TODO 5: Retrieve a List of LicensePlateDataDocument objects from the collectionLink where the exported value is false.

            string sql = @"SELECT VALUE c FROM c WHERE c.exported = @exported";

            QueryDefinition query = new QueryDefinition( sql ).WithParameter( "@exported", false );

            FeedIterator<LicensePlateDataDocument> iterator = container.GetItemQueryIterator<LicensePlateDataDocument>( query );
            FeedResponse<LicensePlateDataDocument> responses = await iterator.ReadNextAsync( cancellationToken ).ConfigureAwait( false );

            foreach( LicensePlateDataDocument plate in responses )
            {
                licensePlates.Add( plate );
            }

            // TODO 6: Remove the line below.
            //licensePlates = new List<LicensePlateDataDocument>();
        }

        int exportedCount = licensePlates.Count;
        this._log.LogInformation( $"{exportedCount} license plates found that are ready for export" );
        return licensePlates;
    }

    /// <summary>
    /// Updates license plate records (documents) as exported. Call after successfully
    /// exporting the passed in license plates.
    /// In a production environment, it would be best to create a stored procedure that
    /// bulk updates the set of documents, vastly reducing the number of transactions.
    /// </summary>
    /// <param name="licensePlates"></param>
    /// <returns></returns>
    public async Task MarkLicensePlatesAsExportedAsync( IEnumerable<LicensePlateDataDocument> licensePlates, CancellationToken cancellationToken )
    {
        this._log.LogInformation( "Updating license plate documents exported values to true" );

        using( CosmosClient client = new CosmosClient( this._endpointUrl, this._authorizationKey ) )
        {
            Database database = client.GetDatabase( this._databaseId );
            Container container = database.GetContainer( this._collectionId );

            foreach( LicensePlateDataDocument licensePlate in licensePlates )
            {
                licensePlate.Exported = true;
                ItemResponse<LicensePlateDataDocument> response = await container.ReplaceItemAsync( licensePlate, licensePlate.Id, cancellationToken: cancellationToken )
                                                                                 .ConfigureAwait( false );
                this._log.LogInformation( $"Update of document ({licensePlate.Id}) was ({response.StatusCode}) and cost ({response.RequestCharge})" );
            }
        }
    }
}
