namespace SGMK_REST.Models
{
    public class HierarchicalNomenclature : Nomenclature
    {
        public int Id { get; set; }

        public double SummarizedPrice { get; set; }

        public HierarchicalNomenclature(int id, string name, int quantity, double price, double summarizedPrice) : base(name, price, quantity)
        {
            Id = id;
            SummarizedPrice = summarizedPrice;
        }

    }

    public class LeveledNomenclature : HierarchicalNomenclature
    {
        public int Level { get; set; }
        public int ParentID { get; set; }

        public LeveledNomenclature(int id, string name, int quantity, double price, double summarizedPrice, int level, int parentID) : base(id, name, quantity, price, summarizedPrice)
        {
            Level = level;
            ParentID = parentID;
        }

        public HierarchicalNomenclature toHierarchicalNomenlature()
        {
            return new HierarchicalNomenclature(Id, Name, Quantity, Price, SummarizedPrice);
        }
    }
}
