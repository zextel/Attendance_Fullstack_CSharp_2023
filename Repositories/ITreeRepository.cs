using SGMK_REST.Models;

namespace SGMK_REST.Repositories
{
    public interface ITreeRepository
    {
        public List<Nomenclature> GetNomenclatures();
        public IEnumerable<TreeItem<HierarchicalNomenclature>> GetNomenclature(int id);
        public void AddNomenclature(PostNomenklature nomenclature);
    }
}
