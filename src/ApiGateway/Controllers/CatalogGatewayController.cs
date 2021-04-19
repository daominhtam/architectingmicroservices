using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ApiGateway.API.Dtos.Catalog;
using ApiGateway.API.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestCommunication;
using ServiceDiscovery;

namespace ApiGateway.API.Controllers
{
    /// <summary>
    ///     Gateway microservice that manages Product Catalog experience
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogGatewayController : Controller
    {
        private readonly ILogger _logger;
        private readonly IRestClient _restClient;

        public CatalogGatewayController(IRestClient restClient,   
            ILogger<CatalogGatewayController> logger)
        {
            _restClient = restClient;
            _logger = logger;
        }

        /// <summary>
        ///     Gets All Music Products
        /// </summary>
        /// <returns>All Music Items</returns>
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(MusicDto), 200)]
        [HttpGet("Music", Name = "GetAllMusicGatewayRoute")]
        public async Task<IActionResult> GetAllMusic()
        {
            // RestClient returns three values, or a Tuple type:
            //   value 1: payload
            //   vaule 2: optional error text message
            //   vaule 3: http status code for response

            var correlationToken = CorrelationTokenManager.GenerateToken();

            // Call backend microservice
            var response = await _restClient.GetAsync<List<MusicDto>>(ServiceEnum.Catalog,
                    "api/Catalog/Music", correlationToken);
            
            // Build return response
            if (response.Data == null || response.Data.Count < 1)
            {
                // Build error response
                _logger.LogError(LoggingEvents.GetAllMusic, $"No product found for request:{correlationToken}");
                return StatusCode((int) response.HttpStatusCode, response.ErrorMessage);
            }

            // Build success response
            _logger.LogInformation(LoggingEvents.GetAllMusic, $"Fetched all products for request: {correlationToken}");
            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Gets Specified Music Product
        /// </summary>
        /// <param name="id">Identifier of Music Item</param>
        /// <returns>Single Music Item</returns>
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(MusicDto), 200)]
        [HttpGet("Music/{id}", Name = "GetMusicGatewayRoute")]
        public async Task<IActionResult> GetMusic(int id)
        {
            // See GetAllMusic() action method above for explanation
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<MusicDto>(ServiceEnum.Catalog,
                $"api/Catalog/Music/{id}", correlationToken);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetMusic, $"ProductId:{id} not found for request:{correlationToken}");
                return StatusCode((int) response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetMusic, $"ProductId:{id} found for music for request:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Gets popular products
        /// </summary>
        /// <param name="count">Items to Show</param>
        /// <returns>Top Selling Music Items</returns>
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(List<MusicDto>), 200)]
        [HttpGet("TopSellingMusic/{count}", Name = "GetTopSellingMusicGatewayRoute")]
        public async Task<IActionResult> GetTopSellingItems(int count)
        {
            // See GetAllMusic() action method above for explanation

            var correlationToken = CorrelationTokenManager.GenerateToken();

            // Get music
            var response = await _restClient.GetAsync<List<MusicDto>>(ServiceEnum.Catalog,
                $"api/Catalog/TopSellingMusic/{count}", correlationToken);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetTopSellingMusic,
                    $"Popular products not found:{response.ErrorMessage} for request:{correlationToken}");
                return StatusCode((int) response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetTopSellingMusic,
                $"Popular products for request:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Get Specified Music Genre Type
        /// </summary>
        /// <param name="id">Identifier of Genre Item</param>
        /// <returns>Specific Genre Type</returns>
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(GenreDto), 200)]
        [HttpGet("Genre/{id:int}", Name = "GetGenreGatewayRoute")]
        public async Task<IActionResult> GetGenre(int id, [FromQuery] bool includeAlbums)
        {
            // See GetAllMusic() action method above for explanation
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<GenreDto>(ServiceEnum.Catalog,
                $"api/Catalog/Genre/{id}?includeAlbums={includeAlbums}", correlationToken);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetGenreById,
                    $"GenreId:{id} not found:{response.ErrorMessage} for request:{correlationToken}");
                return StatusCode((int) response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetGenreById,
                $"Gateway: GenreId:{id} found for request:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Gets All Music Genre Types
        /// </summary>
        /// <returns>List of all Genre Types</returns>
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(List<GenreDto>), 200)]
        [HttpGet("Genres", Name = "GetAllGenresGatewayRoute")]
        public async Task<IActionResult> GetAllGenres([FromQuery] bool includeAlbums)
        {
            // See GetAllMusic() action method above for explanation
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<List<GenreDto>>(ServiceEnum.Catalog,
                $"api/Catalog/Genres/?includeAlbums={includeAlbums}", correlationToken);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetAllGenres,
                    $"Genres not found:{response.ErrorMessage} for request:{correlationToken}");
                return StatusCode((int) response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetAllGenres,
                $"Genres found for request:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Gets All Music Artists
        /// </summary>
        /// <returns>List of all Artist Types</returns>
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(List<ArtistDto>), 200)]
        [HttpGet("Artists", Name = "GetAllArtistsGatewayRoute")]
        public async Task<IActionResult> GetAllArtists()
        {
            // See GetAllMusic() action method above for explanation
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<List<ArtistDto>>(ServiceEnum.Catalog,
                "api/Catalog/Artists", correlationToken);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetAllArtists,
                    $"Artist not found:{response.ErrorMessage} for request: {correlationToken}");
                return StatusCode((int) response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetAllArtists,
                $"Artists found for request:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Adds New Music Product
        /// </summary>
        /// <param name="musicDto">New Music Item</param>
        [ProducesResponseType((int) HttpStatusCode.Created)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [HttpPost("Music", Name = "AddMusicGatewayRoute")]
        public async Task<IActionResult> Post([FromBody] MusicDto musicDto)
        {
            // See GetAllMusic() action method above for explanation
            var correlationToken = CorrelationTokenManager.GenerateToken();

            if (!ModelState.IsValid)
            {
                _logger.LogError(LoggingEvents.CatalogPost,
                    $"ModelState invalid for product update. Request:{correlationToken}. Error:{ModelState.Values}");
                return BadRequest(ModelState);
            }

            var response = await _restClient.PostAsync<MusicDto>(ServiceEnum.Catalog,
                "api/Catalog", correlationToken, musicDto);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.CatalogPost,
                    $"Product not added:{response.ErrorMessage} for request:{correlationToken}");
                return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.CatalogPost,
                $"Product added for request:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Updates Existing Music Product
        /// </summary>
        /// <param name="musicDto">New Music Item</param>
        [ProducesResponseType((int) HttpStatusCode.Created)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [HttpPut("Music", Name = "UpdateMusicGatewayRoute")]
        public async Task<IActionResult> Put([FromBody] MusicDtoUpdate musicDto)
        {
            // See GetAllMusic() action method above for explanation
            var correlationToken = CorrelationTokenManager.GenerateToken();

            if (!ModelState.IsValid)
            {
                _logger.LogError(LoggingEvents.CatalogPut,
                    $"ModelState invalid for product update. Request:{correlationToken}. Error:{ModelState.Values}");
                return BadRequest(ModelState);
            }

            var response = await _restClient.PutAsync<MusicDtoUpdate>(ServiceEnum.Catalog,
                "api/Catalog", correlationToken, musicDto);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.CatalogPut,
                    $"Could not update product:{response.ErrorMessage} for correlationToken: {correlationToken}");
                return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.CatalogPut,
                $"Updated product for request:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Propagates products to Basket service read model
        /// </summary>
        /// <returns>Returns Http Status Code 500 - Internal Server Error</returns>
        [ProducesResponseType((int) HttpStatusCode.Created)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [HttpPost("Catalog/CreateBasketReadModel", Name = "CreateBasketGatewayRoute")]
        public async Task<IActionResult> PostReadModel()
        {
            // See GetAllMusic() action method above for explanation
            var correlationToken = CorrelationTokenManager.GenerateToken();

            if (!ModelState.IsValid)
            {
                _logger.LogError(LoggingEvents.CatalogPostReadModel,
                    $"ModelState invalid for ReadModel Post. Request:{correlationToken}. Error:{ModelState.Values}");
                return BadRequest(ModelState);
            }
            
            var response = await _restClient.PostAsync<MusicDtoUpdate>(ServiceEnum.Catalog,
                "api/Catalog/CreateBasketReadModel", correlationToken);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.CatalogPost,
                    $"Error propagating catalog read model:{response.ErrorMessage} for request:{correlationToken}");
                return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.CatalogPost,
                $"Read model propagated for request:{correlationToken}");

            return new ObjectResult(response);
        }
    }
}