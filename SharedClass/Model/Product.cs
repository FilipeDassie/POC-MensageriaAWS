namespace SharedClass.Model
{
    public class Product
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public decimal Value { get; set; }

        public int Quantity { get; set; }

        public string Variant { get; set; }

        public bool Reserved { get; set; }
    }
}