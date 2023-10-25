using Microsoft.AspNetCore.Mvc;

using SGMK_REST.Models;
using SGMK_REST.Repositories;

namespace SGMK_REST.Controllers
{
    [ApiController]
    public class TreeController : ControllerBase
    {
        private readonly ILogger<TreeController> _logger;
        private TreeRepository repository = new TreeRepository();

        public TreeController(ILogger<TreeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("api/nomenklatures/", Name = "GetNomenklatures")]
        public IEnumerable<Nomenclature> Get()
        {
            return repository.GetNomenclatures();
        }

        [HttpGet("api/nomenklatures/{id}", Name = "GetNomenklatureById")]
        public IEnumerable<TreeItem<HierarchicalNomenclature>> Get(int id)
        {
            return repository.GetNomenclature(id);
        }

        [HttpPost("api/nomenklatures/", Name = "PostNomenklatures")]
        public void Post(PostNomenklature nomenclature)
        {
            repository.AddNomenclature(nomenclature);
        }
    }
}