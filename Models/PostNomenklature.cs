using System.Text.Json.Serialization;

namespace SGMK_REST.Models
{
    public class PostNomenklature
    /* 
    Модель для данных, отправляемых POST-запросом
     */
    {
        public string Name { get; set; }

        public double Price { get; set; }

        public int Quantity { get; set; }

        public int? ParentID { get; set; }

        [JsonConstructor]
        public PostNomenklature(string name, double price, int quantity, int? parentID)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
            ParentID = parentID;
        }
    }
}
