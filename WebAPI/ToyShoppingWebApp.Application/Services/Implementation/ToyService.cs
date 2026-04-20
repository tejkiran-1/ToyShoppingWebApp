namespace ToyShoppingWebApp.Application.Services.Implementations
{
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Application.Services.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;

    public class ToyService : IToyService
    {
        private readonly IToyRepository _toyRepository;

        public ToyService(IToyRepository toyRepository)
        {
            _toyRepository = toyRepository;
        }

        public async Task<PaginatedResponse<ToyDto>> GetToysByCategoryAsync(
            int categoryId, int page = 1, int pageSize = 10)
        {
            // Validate inputs
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and pageSize must be greater than 0");

            // Safety cap on page size (prevent abuse)
            if (pageSize > 100)
                pageSize = 100;

            // Get from repository
            var (toys, totalCount) = await _toyRepository
                .GetToysByCategoryAsync(categoryId, page, pageSize);

            // Map to DTOs
            var toyDtos = toys.Select(t => new ToyDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                Stock = t.Stock,
                CategoryName = t.Category?.Name ?? string.Empty
            }).ToList();

            // Return paginated response
            return new PaginatedResponse<ToyDto>
            {
                Data = toyDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<List<ToyDto>> GetAllToysAsync()
        {
            var toys = await _toyRepository.GetAllToysAsync();

            return toys.Select(t => new ToyDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                Stock = t.Stock,
                CategoryName = t.Category?.Name ?? string.Empty
            }).ToList();
        }

        public async Task<ToyDto?> GetToyByIdAsync(int id)
        {
            var toy = await _toyRepository.GetToyByIdAsync(id);

            if (toy == null)
                return null;

            return new ToyDto
            {
                Id = toy.Id,
                Name = toy.Name,
                Description = toy.Description,
                Price = toy.Price,
                Stock = toy.Stock,
                CategoryName = toy.Category?.Name ?? string.Empty
            };
        }

        public async Task<ToyDto> CreateToyAsync(CreateToyRequest request)
        {
            // Business logic validation
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Toy name is required");

            if (request.Price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (request.Stock < 0)
                throw new ArgumentException("Stock cannot be negative");

            // Create entity
            var toy = new Toy
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            // Save to repository
            var createdToy = await _toyRepository.CreateToyAsync(toy);

            // Return DTO
            return new ToyDto
            {
                Id = createdToy.Id,
                Name = createdToy.Name,
                Description = createdToy.Description,
                Price = createdToy.Price,
                Stock = createdToy.Stock,
                CategoryName = createdToy.Category?.Name ?? string.Empty
            };
        }

        public async Task<ToyDto> UpdateToyAsync(int id, CreateToyRequest request)
        {
            // Get existing toy
            var toy = await _toyRepository.GetToyByIdAsync(id);

            if (toy == null)
                throw new KeyNotFoundException($"Toy with id {id} not found");

            // Business logic validation
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Toy name is required");

            if (request.Price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (request.Stock < 0)
                throw new ArgumentException("Stock cannot be negative");

            // Update properties
            toy.Name = request.Name;
            toy.Description = request.Description;
            toy.Price = request.Price;
            toy.Stock = request.Stock;
            toy.CategoryId = request.CategoryId;

            // Save to repository
            var updatedToy = await _toyRepository.UpdateToyAsync(toy);

            // Return DTO
            return new ToyDto
            {
                Id = updatedToy.Id,
                Name = updatedToy.Name,
                Description = updatedToy.Description,
                Price = updatedToy.Price,
                Stock = updatedToy.Stock,
                CategoryName = updatedToy.Category?.Name ?? string.Empty
            };
        }

        public async Task DeleteToyAsync(int id)
        {
            var toy = await _toyRepository.GetToyByIdAsync(id);

            if (toy == null)
                throw new KeyNotFoundException($"Toy with id {id} not found");

            // Soft delete (set IsActive to false)
            await _toyRepository.DeleteToyAsync(id);
        }
    }
}