using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using BlogMultisuarioApp.Models;

namespace BlogMultisuarioApp.Repositories
{
    public class ArticuloRepository
    {
        private readonly IDynamoDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ArticuloRepository(IDynamoDBContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Articulo>> GetAllAsync()
        {            
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("Estado", ScanOperator.Equal, Estado.Publicado)
            };

            return await _context.ScanAsync<Articulo>(scanConditions).GetRemainingAsync();
        }

        public async Task<List<Articulo>> GetAllUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("UserId", ScanOperator.Equal, userId)
            };

            return await _context.ScanAsync<Articulo>(scanConditions).GetRemainingAsync();
        }

        public async Task<Articulo> GetByIdAsync(string articuloId, string userId)
        {
            return await _context.LoadAsync<Articulo>(userId, articuloId);
        }

        public async Task SaveAsync(Articulo articulo)
        {
            await _context.SaveAsync(articulo);
        }

        public async Task DeleteAsync(string articuloId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            await _context.DeleteAsync<Articulo>(userId, articuloId);
        }
    }
}
