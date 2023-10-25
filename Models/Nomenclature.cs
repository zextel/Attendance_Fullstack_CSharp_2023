using System.Text.Json.Serialization;

namespace SGMK_REST.Models
{
    public class Nomenclature
    {
        public string Name { get; set; }

        public double Price { get; set; }

        public int Quantity { get; set; }


        [JsonConstructor]
        public Nomenclature(string name, double price, int quantity)
        {
            Name = name;

            Price = price;

            Quantity = quantity;
        }

    }
}
