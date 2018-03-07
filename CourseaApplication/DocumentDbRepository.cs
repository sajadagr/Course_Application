using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace CourseaApplication
{
    public class DocumentDbRepository<T> where T:class
    {
        //get databaseId and collectionId and declare client
        public static readonly string DatabaseId = ConfigurationManager.AppSettings["DatabaseId"];
        public static readonly string CollectionId = ConfigurationManager.AppSettings["CollectionId"];
        public static DocumentClient client;

        //initialize methos creates documentDb class by using Uri and key
        public static void Initialize()
        {
            Uri serviceEndpoint = new Uri(ConfigurationManager.AppSettings["CosmosDbUri"]);
            string authKey = ConfigurationManager.AppSettings["CosmosDbKey"];

            client = new DocumentClient(serviceEndpoint, authKey);

            //call the methods if database or collection doesnot exist
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        //method for creating database if it doesnot exist
        public static async Task CreateDatabaseIfNotExistsAsync()
        {

            try
            {//read from already created database
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch(DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {//if not found then create a new database
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                    throw;
            }
        }

        //method for creating collection if it does not exist
        public static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {//UriFactory class is used to create Uris needed for use in documentClient Instace
                Uri documentCollectionLink = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
                await client.ReadDocumentCollectionAsync(documentCollectionLink);
            }
            catch(DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {//await is used to put suspension till the awaited task completes,used only in async method
                    await client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(DatabaseId),
                                         new DocumentCollection { Id = CollectionId },
                                         new RequestOptions { OfferThroughput = 400 });
                }
                else
                    throw;
            }
        }



        //CRUD Operations
        public static async Task<T> GetItemAsync(string id)
        {
            try
            {//read the item and return
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                return (T)(dynamic)document;
            }
            catch(DocumentClientException e)
            {//if item not found
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                else
                    throw;
            }
        }

        //for List of items
        public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T,bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                                                                    new FeedOptions { MaxItemCount = -1 })
                                                                    .Where(predicate).AsDocumentQuery();

            List<T> results = new List<T>();
            while(query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }
            return results;
        }

        //insert item
        public static async Task<Document> CreateItemAsync(T item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
        }


        //update the item
        public static async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item);
        }

        public static async Task DeleteItemAsync(string id)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
        }
        
    }
}