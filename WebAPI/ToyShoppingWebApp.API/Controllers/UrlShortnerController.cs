using Microsoft.AspNetCore.Mvc;
using ToyShoppingWebApp.Application.Services.Interfaces;
using ToyShoppingWebApp.Application.DTOs;

namespace ToyShoppingWebApp.API.Controllers
{
    [ApiController]
    [Route("api/")]
    public class UrlShortnerController : ControllerBase
    {
        private readonly IUrlShortnerService urlShortnerService;

        public UrlShortnerController(IUrlShortnerService shortnerService)
        {
            urlShortnerService = shortnerService;
        }


        [HttpPost("shorten")]
        public async Task<ActionResult<ShortenUrlResponse>> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            if (string.IsNullOrEmpty(request?.LongUrl))
                return BadRequest("Long URL cannot be empty.");
            
            var shortCode = await urlShortnerService.ShortUrl(request.LongUrl);
            
            var response = new ShortenUrlResponse
            {
                ShortCode = shortCode,
                ShortUrl = $"http://localhost:5082/api/{shortCode}"
            };
            
            return Ok(response);
        }

        [HttpGet("{shortUrl}")]
        public async Task<ActionResult<string>> GetLongUrl([FromRoute] string shortUrl)
        {
            if (string.IsNullOrEmpty(shortUrl))
                return BadRequest("Short URL cannot be empty.");

            var longUrl = await urlShortnerService.GetLongUrl(shortUrl);
            // redirect to long URL if found, otherwise return 404
            if (longUrl != null)
                return Redirect(longUrl);
            else
                return NotFound("Short URL not found.");
        }
    }
}