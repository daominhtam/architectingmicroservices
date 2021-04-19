using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ApiGateway.API.Dtos.Basket;
using ApiGateway.API.Dtos.CheckOut;
using ApiGateway.API.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestCommunication;
using ServiceDiscovery;
using Utilities;

namespace ApiGateway.API.Controllers
{
    /// <summary>
    ///     Gateway microservice that manages Shopping Basket experience
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BasketGatewayController : Controller
    {
        private readonly IRestClient _restClient;
        private readonly ILogger<BasketGatewayController> _logger;

        public BasketGatewayController(IRestClient restClient, 
                    ILogger<BasketGatewayController> logger)
        {
            _restClient = restClient;
            _logger = logger;
        }

        /// <summary>
        ///     Gets All Shopping Baskets.
        /// </summary>
        /// <returns>List of line items that make up a shopping basket</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(BasketDto), 200)]
        [HttpGet("Baskets", Name = "GetAllBasketsGatewayRoute")]
        public async Task<IActionResult> GetAllBaskets()
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<List<BasketDto>>(ServiceEnum.Basket,
                "api/Basket/Baskets", correlationToken);

            if (response == null || response.Data == null || response.Data.Count < 1)
            {
                _logger.LogError(LoggingEvents.GetAllBaskets, $"Basket: No baskets found for correlationToken:{correlationToken}");
                return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetAllBaskets, $"Basket: Baskets found for correlationToken:{correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Gets Specfied Shopping Basket and its Line Items
        /// </summary>
        /// <param name="basketId">Identifier for user shopping basket</param>
        /// <returns>List of line items that make up a shopping basket</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(BasketItemDto), 200)]
        [HttpGet("Basket/{basketId}", Name = "GetBasketGatewayRoute")]
        public async Task<IActionResult> GetBasket(string basketId)
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<BasketDto>(ServiceEnum.Basket,
                $"api/Basket/Basket/{basketId}", correlationToken);

            if (response == null || response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetBasket, $"Basket: No baskets found for correlationToken:{correlationToken}");
                return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetBasket, $"Basket: Baskets found for correlationToken: {correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Consumed by UI to determine if a specified shopping basket existss
        /// </summary>
        /// <param name="basketId">Identifier for user shopping basket</param>
        /// <returns>List of line items that make up a shopping basket</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(CartExistModel), 200)]
        [HttpGet("BasketSummary/{basketId}", Name = "GetBasketSummaryGatewayRoute")]
        public async Task<IActionResult> GetBasketSummary(string basketId)
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<BasketSummaryDto>(ServiceEnum.Basket,
                $"api/Basket/BasketSummary/{basketId}", correlationToken);

            if (response == null || response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetBasket, $"Basket: No baskets found for correlationToken:{correlationToken}");
                // This is a system hack. Return 200 status code to the CartSummaryComponent. 
                // We don't show error, but do not show the summary icon.
                return StatusCode(200);
            }

            _logger.LogInformation(LoggingEvents.GetBasket, $"Basket: Baskets found for correlationToken: {correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        ///     Adds New Line Item to Specified Shopping Basket
        /// </summary>
        /// <param name="productId">Product Identifier</param>
        /// <param name="basketId">Identifier for user shopping basket</param>
        /// <returns>The newly-created line item</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(BasketItemDto), 201)]
        [HttpPost("Basket/{basketId}/item/{productId:int}", Name = "NewBasketLineItemGatewayRoute")]
        public async Task<IActionResult> AddItemToBasket(string basketId, int productId)
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Send product information and basketId to the Basket microservice
            // Note: Sending querystring parameters in the post.
            var response = await _restClient.PostAsync<dynamic>(ServiceEnum.Basket,
                $"api/Basket/?productId={productId}&basketId={basketId}", correlationToken, null);

            if (response == null || response.Data == null)
            {
                _logger.LogError(LoggingEvents.AddItemToBasket, $"ProductId:{productId} not found for request:{correlationToken} - Is Catalog ReadModel populated?");
                return StatusCode((int)response.HttpStatusCode, $"{response.ErrorMessage} - Is the Catalog ReadModel populated?" );
            }
            
            _logger.LogInformation(LoggingEvents.AddItemToBasket, $"Basket: Added Item to Basket for correlationToken: {correlationToken}");
            
            //return CreatedAtRoute("NewBasketLineItemGatewayRoute", response);
            return CreatedAtRoute("NewBasketLineItemGatewayRoute", response.Data);
        }

        /// <summary>
        ///     Converts Shopping Basket to an Order
        /// </summary>
        /// <returns>The newly-created line item</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(string), 201)]
        [HttpPost("CheckOut", Name = "CheckOutGatewayRoute")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutDto checkOutDto)
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Note: Sending querystring parameters in the post.
            //var response = await _restClient.PostAsync<CheckOutDto>(EnumLookup.Basket,
            //    $"api/Basket/CheckOut/?_correlationToken={_correlationToken}", checkOutDto);

            var response = await _restClient.PostAsync<dynamic>(ServiceEnum.Basket,
                "api/Basket/CheckOut/", correlationToken, checkOutDto);
            //var response = await _restClient.PostAsync<CheckOutDto>(ServiceEnum.Basket,
            //    "api/Basket/CheckOut/", correlationToken, checkOutDto);
            
            if (response == null || response.Data == null)
            {
                _logger.LogInformation(LoggingEvents.Checkout, $"Basket: Basket could not be found for correlationToken: {correlationToken}");
                return BadRequest($"Shopping Basket with Id {checkOutDto.BasketId} could not be found");
            }

            _logger.LogInformation(LoggingEvents.Checkout, $"Basket: Item added to Basket for correlationToken: {correlationToken}");

            return Accepted(response.Data);
        }

        /// <summary>
        ///     Removes Specified Line Item from a Shopping Basket
        /// </summary>
        /// <param name="basketId">Identifier for user shopping basket</param>
        /// <param name="productId">Product Identifier</param>
        /// <returns>Summary of shopping basket state</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(BasketItemRemoveDto), 200)]
        [HttpDelete("Basket/{basketId}/item/{productId:int}")]
        public async Task<IActionResult> DeleteLineItem(string basketId, int productId)
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Note: Sending querystring parameters in the post.
            var success = await _restClient.DeleteAsync(ServiceEnum.Basket,
                $"api/basket/{basketId}/lineitem/{productId}", correlationToken);

            if (!success)
            {
                _logger.LogError(LoggingEvents.Checkout, $"Could not delete line item {productId} for Basket {basketId} for Request {correlationToken}");
                return BadRequest($"Could not delete line item {productId} for Basket {basketId} for Request {correlationToken}");
            }

            _logger.LogInformation(LoggingEvents.DeleteLineItem, $"Basket: Line item deleted for correlationToken: {correlationToken}");
            
            return NoContent();
        }

        /// <summary>
        ///     Removes entire Shopping Basket and all its Line Items
        /// </summary>
        /// <param name="basketId">Identifier for user shopping basket</param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(204)]
        [HttpDelete("Basket/{basketId}")]
        public async Task<IActionResult> Delete(string basketId)
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            Guard.ForNullOrEmpty("basketId", "Must include BasketID value");

            // Note: Sending querystring parameters in the post.
            var response = await _restClient.DeleteAsync(ServiceEnum.Basket,
                $"api/Basket/?basketId={basketId}", correlationToken);

            if (!response)
            {
                _logger.LogError(LoggingEvents.Checkout, $"Could not delete Basket {basketId} for Request {correlationToken}");
                return BadRequest($"Could not delete Basket {basketId} for Request {correlationToken}");
            }

            _logger.LogInformation(LoggingEvents.DeleteLineItem, $"Basket: Removed shopping basket for correlationToken: {correlationToken}");
            
            return NoContent();
        }
    }
}