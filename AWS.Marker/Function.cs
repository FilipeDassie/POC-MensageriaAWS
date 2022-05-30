using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using SharedClass;
using SharedClass.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Amazon.Lambda.SQSEvents.SQSEvent;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWS.Marker
{
    public class Function
    {
        private AmazonDynamoDBClient AmazonDynamoDBClient { get; }

        public Function()
        {
            AmazonDynamoDBClient = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
        }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            if (evnt.Records.Count > 1) throw new InvalidOperationException("Somente uma mensagem pode ser tratada por vez!");

            SQSMessage objMessage = evnt.Records.FirstOrDefault();

            if (objMessage == null) return;

            await ProcessMessageAsync(objMessage, context);
        }

        private async Task ProcessMessageAsync(SQSMessage message, ILambdaContext context)
        {
            Order objOrder = JsonConvert.DeserializeObject<Order>(message.Body);

            objOrder.Status = StatusOrder.Reserved;

            foreach (Product objProduct in objOrder.Products)
            {
                try
                {
                    await DownloadStock(objProduct.Id, objProduct.Quantity);

                    objProduct.Reserved = true;

                    context.Logger.LogLine($"Produto baixado do estoque {objProduct.Id} - {objProduct.Name}");
                }
                catch (ConditionalCheckFailedException)
                {
                    objOrder.CancellationJustification = $"Produto indisponível no estoque {objProduct.Id} - {objProduct.Name}";
                    objOrder.Canceled = true;

                    context.Logger.LogLine($"Erro: {objOrder.CancellationJustification}");

                    break;
                }
            }

            if (objOrder.Canceled)
            {
                foreach (Product objProducts in objOrder.Products)
                {
                    if (objProducts.Reserved)
                    {
                        await ReturnStock(objProducts.Id, objProducts.Quantity);

                        objProducts.Reserved = false;

                        context.Logger.LogLine($"Produto devolvido ao estoque {objProducts.Id} - {objProducts.Name}");
                    }
                }

                await AmazonUtil.SendToQueue(EnumQueueSNS.Failure, objOrder);

                await objOrder.SaveAsync();
            }
            else
            {
                await AmazonUtil.SendToQueue(EnumQueueSQS.Reserved, objOrder);

                await objOrder.SaveAsync();
            }
        }

        private async Task DownloadStock(string id, int quantity)
        {
            var request = new UpdateItemRequest
            {
                TableName = "estoque",
                ReturnValues = "NONE",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue{ S = id } }
                },
                UpdateExpression = "SET Quantity = (Quantity - :orderQuantity)",
                ConditionExpression = "Quantity >= :orderQuantity",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":orderQuantity", new AttributeValue { N = quantity.ToString() } }
                }
            };

            await AmazonDynamoDBClient.UpdateItemAsync(request);
        }

        private async Task ReturnStock(string id, int quantity)
        {
            var request = new UpdateItemRequest
            {
                TableName = "Stock",
                ReturnValues = "NONE",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue{ S = id } }
                },
                UpdateExpression = "SET Quantity = (Quantity + :orderQuantity)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":orderQuantity", new AttributeValue { N = quantity.ToString() } }
                }
            };

            await AmazonDynamoDBClient.UpdateItemAsync(request);
        }
    }
}