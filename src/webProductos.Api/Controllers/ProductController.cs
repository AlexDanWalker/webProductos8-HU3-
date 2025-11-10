using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webProductos.Application.DTOs.Product;
using webProductos.Application.Interfaces;

namespace webProductos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        //  Listar todos los productos
        [HttpGet]
        [Authorize(Roles = "Admin,Seller,User")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        //  Obtener producto por Id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller,User")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        //  Crear producto (solo Admin o Seller)
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Create(ProductDto productDto)
        {
            //  Obtener Id del usuario desde el token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var product = await _productService.CreateAsync(productDto, userId);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        //  Actualizar producto (solo Admin o Seller)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Update(int id, ProductDto productDto)
        {
            if (id != productDto.Id) return BadRequest();

            //  Opcional: validar que el usuario que actualiza es el creador o admin
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var updated = await _productService.UpdateAsync(productDto); 
            return Ok(updated);
        }

        //  Eliminar producto (solo Admin o Seller)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
