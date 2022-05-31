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
            Console.WriteLine(string.Empty);
            Console.WriteLine("Olá! Escreva uma mensagem e aperte <ENTER> para enviar.");
            Console.WriteLine(string.Empty);

            AmazonSQSClient objClient = new AmazonSQSClient(RegionEndpoint.SAEast1);

            SendMessageRequest objRequest = null;

            string message = string.Empty;

            while (true)
            {
                message = Console.ReadLine();

                if (!string.IsNullOrEmpty(message))
                {
                    objRequest = new SendMessageRequest()
                    {
                        QueueUrl = QUEUE_URL,
                        MessageBody = message
                    };

                    await objClient.SendMessageAsync(objRequest);
                }
            }
        }
    }
}