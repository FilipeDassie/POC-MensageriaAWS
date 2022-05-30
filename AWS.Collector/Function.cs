using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using SharedClass;
using SharedClass.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWS.Collector
{
    public class Function
    {
        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            foreach (var record in dynamoEvent.Records)
            {
                if (record.EventName == "INSERT")
                {
                    Order objOrder = record.Dynamodb.NewImage.ToObject<Order>();
                    objOrder.Status = StatusOrder.Collected;

                    try
                    {
                        await ProcessOrderAmount(objOrder);

                        await AmazonUtil.SendToQueue(EnumQueueSQS.Order, objOrder);

                        context.Logger.LogLine($"Sucesso na coleta do pedido: '{objOrder.Id}'");
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogLine($"Error: '{ex.Message}'");

                        objOrder.CancellationJustification = ex.Message;
                        objOrder.Canceled = true;

                        await AmazonUtil.SendToQueue(EnumQueueSNS.Failure, objOrder);
                    }

                    await objOrder.SaveAsync();
                }
            }
        }

        private async Task ProcessOrderAmount(Order objOrder)
        {
            foreach (Product objProduct in objOrder.Products)
            {
                Product objStockProduct = await GetProductDynamoDBAsync(objProduct.Id);

                if (objStockProduct == null) throw new InvalidOperationException($"Produto não encontrado na tabela estoque. {objProduct.Id}");

                objProduct.Value = objStockProduct.Value;
                objProduct.Name = objStockProduct.Name;
            }

            decimal amount = objOrder.Products.Sum(x => x.Value * x.Quantity);

            if (objOrder.Quantity != 0 && objOrder.Quantity != amount)
                throw new InvalidOperationException($"O valor esperado do pedido é de R$ {objOrder.Quantity} e o valor verdadeiro é R$ {amount}");

            objOrder.Quantity = amount;
        }

        private async Task<Product> GetProductDynamoDBAsync(string id)
        {
            AmazonDynamoDBClient objClient = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);

            QueryRequest objQueryRequest = new QueryRequest
            {
                TableName = "Stock",
                KeyConditionExpression = "Id = :v_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":v_id", new AttributeValue { S = id } } }
            };

            QueryResponse objResponse = await objClient.QueryAsync(objQueryRequest);

            var item = objResponse.Items.FirstOrDefault();

            if (item == null) return null;

            return item.ToObject<Product>();
        }
    }
}