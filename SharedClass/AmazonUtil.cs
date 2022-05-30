using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using SharedClass.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SharedClass
{
    public static class AmazonUtil
    {
        public static async Task SaveAsync(this Order objOrder)
        {
            AmazonDynamoDBClient objClient = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);

            DynamoDBContext objContext = new DynamoDBContext(objClient);

            await objContext.SaveAsync(objOrder);
        }

        public static T ToObject<T>(this Dictionary<string, AttributeValue> dictionary)
        {
            AmazonDynamoDBClient objClient = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);

            DynamoDBContext objContext = new DynamoDBContext(objClient);

            Document objDocument = Document.FromAttributeMap(dictionary);

            return objContext.FromDocument<T>(objDocument);
        }

        public static async Task SendToQueue(EnumQueueSQS objEnumQueueSQS, Order objOrder)
        {
            var json = JsonConvert.SerializeObject(objOrder);

            var client = new AmazonSQSClient(RegionEndpoint.SAEast1);

            var request = new SendMessageRequest
            {
                QueueUrl = $"https://sqs.sa-east-1.amazonaws.com/097660114810/{objEnumQueueSQS}",
                MessageBody = json
            };

            await client.SendMessageAsync(request);
        }

        public static async Task SendToQueue(EnumQueueSNS objEnumQueueSNS, Order objOrder)
        {
            await Task.CompletedTask;
        }
    }
}