using Microsoft.AspNetCore.Mvc;
using Stream_Linkify_Backend.DTOs;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Services;

namespace Stream_Linkify_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlConversionController(
        ILogger<UrlConversionController> logger,
        IMusicInput musicInput
            ) : ControllerBase
    {
        private readonly ILogger<UrlConversionController> logger = logger;

        [HttpPost("tracks")]
        public async Task<IActionResult> ConvertToAllTrackUrls([FromBody] TrackUrlRequestDto request)
        {
            if (!Uri.TryCreate(request.TrackUrl, UriKind.Absolute, out var uri))
                return BadRequest(new { error = "Invalid URL format" });

            try
            {
                var result = await musicInput.GetTrackUrlsAsync(request.TrackUrl);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Track not found or invalid operation for URL: {Url}", request.TrackUrl);
                return NotFound(new { error = ex.Message });
            }
            catch (NotSupportedException ex)
            {
                logger.LogWarning(ex, "Unsupported platform for URL: {Url}", request.TrackUrl);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Argument error for URL: {Url}", request.TrackUrl);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing URL: {Url}", request.TrackUrl);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpPost("albums")]
        public async Task<IActionResult> ConvertToAllAlbumUrls([FromBody] AlbumUrlRequestDto request)
        {
            if (!Uri.TryCreate(request.AlbumUrl, UriKind.Absolute, out var uri))
                return BadRequest(new { error = "Invalid URL format" });

            try
            {
                var resultTrack = await musicInput.GetAlbumUrlsAsync(request.AlbumUrl);
                return Ok(resultTrack);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Track not found or invalid operation for URL: {Url}", request.AlbumUrl);
                return NotFound(new { error = ex.Message });
            }
            catch (NotSupportedException ex)
            {
                logger.LogWarning(ex, "Unsupported platform for URL: {Url}", request.AlbumUrl);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Argument error for URL: {Url}", request.AlbumUrl);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing URL: {Url}", request.AlbumUrl);
                return StatusCode(500, new { error = $"An unexpected error occurred: {ex.Message}" });
            }
        }
    }
}