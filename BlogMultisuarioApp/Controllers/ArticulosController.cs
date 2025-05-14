using Amazon.S3;
using Amazon.S3.Model;
using BlogMultisuarioApp.Models;
using BlogMultisuarioApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogMultisuarioApp.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly ArticuloRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAmazonS3 _s3Client;
        private const string bucketName = "blogmultisuariobucket";

        public ArticulosController(ArticuloRepository repository, IHttpContextAccessor httpContextAccessor, IAmazonS3 s3Client)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _s3Client = s3Client;
        }

        public async Task<IActionResult> Index()
        {
            var articulos = await _repository.GetAllAsync();
            return View(articulos);
        }

        [Authorize]
        public async Task<IActionResult> AllUserArticles()
        {
            var articulos = await _repository.GetAllUserAsync();
            return View(articulos);
        }

        public async Task<IActionResult> Details(string articuloId, string userId, string returnTo = "Index")
        {
            var articulo = await _repository.GetByIdAsync(articuloId,userId);
            if (articulo == null)
                return NotFound();

            ViewBag.ReturnTo = returnTo;
            return View(articulo);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Articulo articulo, IFormFile imagen)
        {
            articulo.UserId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value!;
            articulo.ArticuloId = Guid.NewGuid().ToString();
            articulo.FechaCreacion = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            articulo.FechaActualizacion = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            articulo.Estado = Estado.Borrador;
            articulo.Autor = User.FindFirst("name")?.Value!;

            if (imagen != null && imagen.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
                using var stream = imagen.OpenReadStream();
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = stream,
                    ContentType = imagen.ContentType
                };

                await _s3Client.PutObjectAsync(putRequest);
                articulo.ImagenNombre = fileName;
            }

            await _repository.SaveAsync(articulo);
            return RedirectToAction(nameof(AllUserArticles));
        }

        [Authorize]
        public async Task<IActionResult> Edit(string articuloId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value!;

            var articulo = await _repository.GetByIdAsync(articuloId, userId);
            if (articulo == null)
                return NotFound();

            return View(articulo);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Articulo articulo, IFormFile imagen)
        {
            articulo.UserId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value!;
            articulo.FechaActualizacion = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

            var articuloExistente = await _repository.GetByIdAsync(articulo.ArticuloId, articulo.UserId);

            articulo.ImagenNombre = articuloExistente.ImagenNombre;

            if (imagen != null && imagen.Length > 0)
            {
                var nombreNuevo = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
                var bucketName = "blogmultisuariobucket";

                using var stream = imagen.OpenReadStream();
                var s3Client = new Amazon.S3.AmazonS3Client();

                var putRequest = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = nombreNuevo,
                    InputStream = stream,
                    ContentType = imagen.ContentType
                };
                await s3Client.PutObjectAsync(putRequest);

                if (!string.IsNullOrEmpty(articuloExistente.ImagenNombre))
                {
                    var deleteRequest = new Amazon.S3.Model.DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = articuloExistente.ImagenNombre
                    };
                    await s3Client.DeleteObjectAsync(deleteRequest);
                }

                articulo.ImagenNombre = nombreNuevo;
            }

            await _repository.SaveAsync(articulo);
            return RedirectToAction(nameof(AllUserArticles));
        }

        [Authorize]
        public async Task<IActionResult> Delete(string articuloId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value!;

            var articulo = await _repository.GetByIdAsync(articuloId, userId);
            if (articulo == null)
                return NotFound();

            return View(articulo);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string articuloId)
        {
            await _repository.DeleteAsync(articuloId);
            return RedirectToAction(nameof(AllUserArticles));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publicar(string articuloId)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value!;
            var articulo = await _repository.GetByIdAsync(articuloId, userId);

            if (articulo == null)
                return NotFound();

            articulo.Estado = Estado.Publicado;
            articulo.FechaActualizacion = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            await _repository.SaveAsync(articulo);

            return RedirectToAction(nameof(AllUserArticles));
        }

    }
}
