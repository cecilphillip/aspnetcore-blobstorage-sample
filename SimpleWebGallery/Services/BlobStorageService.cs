using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleWebGallery.Services
{
    public class BlobStorageService
    {
        private readonly BlobStorageConfig _storageConfig;
        private readonly CloudBlobClient _blobClient;
        private readonly string[] _supportedFormats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };


        public BlobStorageService(IOptions<BlobStorageConfig> storageConfig)
        {
            this._storageConfig = storageConfig.Value;

            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            _blobClient = storageAccount.CreateCloudBlobClient();
        }

        public async Task UploadImageToBlobStorage(IFormFile fileUpload)
        {
            CloudBlobContainer container = _blobClient.GetContainerReference(_storageConfig.ImageContainer);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileUpload.FileName);
            using (Stream uploadStream = fileUpload.OpenReadStream())
            {
                await blob.UploadFromStreamAsync(uploadStream);
            }          
        }

        public async Task<IEnumerable<string>> RetrieveImageBlobUrls()
        {
            List<string> imageUrls = new List<string>();
            CloudBlobContainer container = _blobClient.GetContainerReference(_storageConfig.ImageContainer);

            BlobContinuationToken continuationToken = null;

            BlobResultSegment resultSegment = null;

            do
            {
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);

                foreach (var blobItem in resultSegment.Results)
                {
                    imageUrls.Add(blobItem.StorageUri.PrimaryUri.ToString());
                }

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }

            while (continuationToken != null);

            return imageUrls;
        }

        public bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            return _supportedFormats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

    }
}
