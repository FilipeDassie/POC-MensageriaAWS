using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace SharedClass.Model
{
    [DynamoDBTable("Order")]
    public class Order
    {
        public string Id { get; set; }

        public decimal Quantity { get; set; }

        public DateTime CreationDate { get; set; }

        public List<Product> Products { get; set; }

        public Client Client { get; set; }

        public Payment Payment { get; set; }

        public string CancellationJustification { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StatusOrder Status { get; set; }

        public bool Canceled { get; set; }
    }

    public enum StatusOrder
    {
        Collected,
        Reserved,
        Paid,
        Billed
    }
}