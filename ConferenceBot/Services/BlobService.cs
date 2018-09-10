using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ConferenceBot.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json;

namespace ConferenceBot.Services
{
    public static class BlobService
    {
        public static async Task<NdcSydney> DownloadAsync()
        {
            var storageCredentials = new StorageCredentials(ConfigurationManager.AppSettings["AzureStoreAccountName"], ConfigurationManager.AppSettings["AzureStoreKey"]);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var container = cloudBlobClient.GetContainerReference("ndc");
            var newBlob = container.GetBlockBlobReference("NDCSydney18.json");
            var json = await newBlob.DownloadTextAsync();

            return JsonConvert.DeserializeObject<NdcSydney>(json);
        }
    }
}