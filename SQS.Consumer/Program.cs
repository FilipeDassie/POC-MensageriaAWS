using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Threading.Tasks;

namespace SQS.Consumer
{
    class Program
    {
        private const string QUEUE_URL = "https://sqs.sa-east-1.amazonaws.com/097660114810/Queue";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            AmazonSQSClient objClient = new AmazonSQSClient(RegionEndpoint.SAEast1);

            ReceiveMessageRequest objRequest = new ReceiveMessageRequest()
            {
                QueueUrl = QUEUE_URL
            };

            while (true)
            {
                ReceiveMessageResponse objResponse = await objClient.ReceiveMessageAsync(objRequest);

                foreach (Message objMenssage in objResponse.Messages)
                {
                    Console.WriteLine(objMenssage.Body);

                    await objClient.DeleteMessageAsync(objRequest.QueueUrl, objMenssage.ReceiptHandle);
                }
            }
        }
    }
}