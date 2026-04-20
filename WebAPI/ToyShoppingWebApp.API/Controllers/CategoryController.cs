namespace ToyShoppingWebApp.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Services.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        [Produces(typeof(List<CategoryDto>))]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        [Produces(typeof(CategoryDto))]
        public async Task<ActionResult<CategoryDto>> GetCategoryById([FromRoute] int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound(new { error = $"Category with id {id} not found" });

            return Ok(category);
        }

        /// <summary>
        /// Create category (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Produces(typeof(CategoryDto))]
        [Consumes("application/json")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update category (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [Produces(typeof(CategoryDto))]
        [Consumes("application/json")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(
            [FromRoute] int id,
            [FromBody] CreateCategoryRequest request)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, request);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete category (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}