﻿global using System.Globalization;
global using System.Net;
global using System.Net.Http.Headers;
global using System.Text.Json;
global using System.Web.Http;
global using Azure.Messaging.EventGrid;
global using Azure.Messaging.EventGrid.SystemEvents;
global using CsvHelper;
global using CsvHelper.Configuration;
global using Microsoft.Azure.Cosmos;
global using Microsoft.Azure.WebJobs;
global using Microsoft.Extensions.Logging;
global using Microsoft.WindowsAzure.Storage;
global using Microsoft.WindowsAzure.Storage.Blob;
global using Polly;
global using Polly.CircuitBreaker;
global using Polly.Wrap;
global using TollBooth.Models;

global using Microsoft.Azure.WebJobs.Extensions.Http;
global using Microsoft.Azure.WebJobs.Extensions.EventGrid;

