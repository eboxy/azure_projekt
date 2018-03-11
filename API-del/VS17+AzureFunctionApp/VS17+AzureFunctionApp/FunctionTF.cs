using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Threading;
using System.Text.RegularExpressions;

namespace VS17AzureFunctionApp
{
    public class Program
    {
        private const string EndpointUrl = "https://azucos.documents.azure.com:443/";

        private const string PrimaryKey ="QhoCWcDzDiXuJmOG6beIsBmveIa1mrGCDNpkGGwDlCJwPQoGb1orYWr7lbe5W494jyGYPR8EibT4XXyfNbZAlg==";
        private DocumentClient client;

        
        //hämtar alla foto-url:erna från kollektionen med fotos som skall verifieras
        public string GetAllPhotoURLs(string cosmoDatabase, string cosmoCollection)
        {
            this.client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            IQueryable<VerifyPic> photoQuery = this.client.CreateDocumentQuery<VerifyPic>(
                  UriFactory.CreateDocumentCollectionUri(cosmoDatabase, cosmoCollection), queryOptions);

            var photoURL = from photo in photoQuery
                select photo.PhotoUrl;

            string allPhotoURLs = "";

            
                foreach (var pic in photoURL)
                    allPhotoURLs += $"URL: {pic}, ";
            
                

            return allPhotoURLs;
        }
    }

    public class VerifyPic
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string VerId { get; set; }
        public string UserId { get; set; }
        public string PhotoUrl { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }


    
    
    //hämtar alla foton som väntar på verifiering från cosmos
    public class FunctionTf
    {
        [FunctionName("FunctionTF")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            Program prg = new Program();
            log.Info("C# HTTP trigger function processed a request.");

            // parsar query parameter + hämtar querystringvärdet 
            string viewReviewQueue = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "mode", true) == 0)
                .Value;

            DateTime date = new DateTime();
            date = DateTime.Now;
            date.ToString("yyyy-MM dd");

            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            string allPicURLs = prg.GetAllPhotoURLs("cloud", "VerifyPhotosCollection");


            if (viewReviewQueue != null && viewReviewQueue == "viewReviewQueue")
            {
                return req.CreateResponse(HttpStatusCode.OK, "Date: " + date + ", Photos to verify:  " + allPicURLs);
            }

            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Did you forget?: ?mode=viewReviewQueue");
            }


        }


    }
}