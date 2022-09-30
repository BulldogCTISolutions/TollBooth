using System.Net;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;

namespace UploadImages;

internal class Program
{
    private static List<MemoryStream> _sourceImages;
    private static readonly Random _random = new();
    private static string _blobStorageConnection;

    private static int Main( string[] args )
    {
        if( args.Length == 0 )
        {
            Console.WriteLine( "You must pass the Blob Storage connection string as an argument when executing this application." );
            _blobStorageConnection = Console.ReadLine();
            //return 1;
        }
        else
        {
            _blobStorageConnection = args[0];
        }

        Console.WriteLine( "Enter one of the following numbers to indicate what type of image upload you want to perform:" );
        Console.WriteLine( "\t1 - Upload a handful of test photos" );
        Console.WriteLine( "\t2 - Upload 1000 photos to test processing at scale" );
        _ = int.TryParse( Console.ReadLine(), out int choice );

        bool upload1000 = choice == 2;

        UploadImages( upload1000 );

        _ = Console.ReadLine();

        return 0;
    }

    private static void UploadImages( bool upload1000 )
    {
        Console.WriteLine( "Uploading images" );
        int uploaded = 0;
        CloudStorageAccount account = CloudStorageAccount.Parse( _blobStorageConnection );
        CloudBlobClient blobClient = account.CreateCloudBlobClient();
        CloudBlobContainer blobContainer = blobClient.GetContainerReference( "images" );
        _ = blobContainer.CreateIfNotExists();

        // Setup the number of the concurrent operations.
        TransferManager.Configurations.ParallelOperations = 64;
        // Set ServicePointManager.DefaultConnectionLimit to the number of eight times the number of cores.
        ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 8;
        ServicePointManager.Expect100Continue = false;
        // Setup the transfer context and track the upload progress.
        //var context = new SingleTransferContext
        //{
        //    ProgressHandler =
        //        new Progress<TransferStatus>(
        //            (progress) => { Console.WriteLine("Bytes uploaded: {0}", progress.BytesTransferred); })
        //};

        if( upload1000 )
        {
            LoadImagesFromDisk( true );
            for( int i = 0; i < 200; i++ )
            {
                foreach( MemoryStream image in _sourceImages )
                {
                    string filename = GenerateRandomFileName();
                    CloudBlockBlob destBlob = blobContainer.GetBlockBlobReference( filename );

                    Task task = TransferManager.UploadAsync( image, destBlob );
                    task.Wait();
                    uploaded++;
                    Console.WriteLine( $"Uploaded image {uploaded}: {filename}" );
                }
            }
        }
        else
        {
            LoadImagesFromDisk( false );
            foreach( MemoryStream image in _sourceImages )
            {
                string filename = GenerateRandomFileName();
                CloudBlockBlob destBlob = blobContainer.GetBlockBlobReference( filename );

                Task task = TransferManager.UploadAsync( image, destBlob );
                task.Wait();
                uploaded++;
                Console.WriteLine( $"Uploaded image {uploaded}: {filename}" );
            }
        }

        Console.WriteLine( "Finished uploading images" );
    }

    private static string GenerateRandomFileName()
    {
        const int RANDOM_STRING_LENGTH = 8;
        const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string random = new( Enumerable.Repeat( CHARS, RANDOM_STRING_LENGTH )
          .Select( s => s[_random.Next( s.Length )] ).ToArray() );
        return $"{random}.jpg";
    }

    private static void LoadImagesFromDisk( bool upload1000 )
    {
        // This loads the images to be uploaded from disk into memory.
        _sourceImages = upload1000
            ? Directory.GetFiles( @"..\..\..\..\..\license plates\copyfrom\" )
                    .Select( f => new MemoryStream( File.ReadAllBytes( f ) ) )
                    .ToList()
            : Directory.GetFiles( @"..\..\..\..\..\license plates\" )
                    .Select( f => new MemoryStream( File.ReadAllBytes( f ) ) )
                    .ToList();
    }
}
