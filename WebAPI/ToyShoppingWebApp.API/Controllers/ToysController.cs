namespace ToyShoppingWebApp.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Services.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class ToysController : ControllerBase
    {
        private readonly IToyService _toyService;

        public ToysController(IToyService toyService)
        {
            _toyService = toyService;
        }

        /// <summary>
        /// Get toys by category with pagination
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 10, max 100)</param>
        [HttpGet("category/{categoryId}")]
        [Produces(typeof(PaginatedResponse<ToyDto>))]
        public async Task<ActionResult<PaginatedResponse<ToyDto>>> GetToysByCategory(
            [FromRoute] int categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _toyService.GetToysByCategoryAsync(categoryId, page, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active toys
        /// </summary>
        [HttpGet]
        [Produces(typeof(List<ToyDto>))]
        public async Task<ActionResult<List<ToyDto>>> GetAllToys()
        {
            var toys = await _toyService.GetAllToysAsync();
            return Ok(toys);
        }

        /// <summary>
        /// Get toy by ID
        /// </summary>
        [HttpGet("{id}")]
        [Produces(typeof(ToyDto))]
        public async Task<ActionResult<ToyDto>> GetToyById([FromRoute] int id)
        {
            var toy = await _toyService.GetToyByIdAsync(id);

            if (toy == null)
                return NotFound(new { error = $"Toy with id {id} not found" });

            return Ok(toy);
        }

        /// <summary>
        /// Create new toy (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Produces(typeof(ToyDto))]
        [Consumes("application/json")]
        public async Task<ActionResult<ToyDto>> CreateToy([FromBody] CreateToyRequest request)
        {
            try
            {
                var toy = await _toyService.CreateToyAsync(request);
                return CreatedAtAction(nameof(GetToyById), new { id = toy.Id }, toy);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update toy (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [Produces(typeof(ToyDto))]
        [Consumes("application/json")]
        public async Task<ActionResult<ToyDto>> UpdateToy(
            [FromRoute] int id,
            [FromBody] CreateToyRequest request)
        {
            try
            {
                var toy = await _toyService.UpdateToyAsync(id, request);
                return Ok(toy);
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
        /// Delete toy (admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToy([FromRoute] int id)
        {
            try
            {
                await _toyService.DeleteToyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}