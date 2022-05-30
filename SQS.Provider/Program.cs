using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Threading.Tasks;

namespace SQS.Provider
{
    class Program
    {
        private const string QUEUE_URL = "https://sqs.sa-east-1.amazonaws.com/097660114810/Queue";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            AmazonSQSClient objClient = new AmazonSQSClient(RegionEndpoint.SAEast1);

            SendMessageRequest objRequest = new SendMessageRequest()
            {
                QueueUrl = QUEUE_URL,
                MessageBody = "teste 123"
            };

            await objClient.SendMessageAsync(objRequest);
        }
    }
}