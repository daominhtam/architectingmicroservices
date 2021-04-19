using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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
    /// Gateway API microservice that manages Ordering experience
    /// </summary>
    [ApiController]
    //[ApiVersion("1.0")]
    //[ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class OrderingGatewayController : Controller
    {
        private readonly IRestClient _restClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderingGatewayController> _logger;
        private readonly bool _gRpcFeatureFlag;
        
        public OrderingGatewayController(IRestClient restClient,
                                         IConfiguration configuration,
                                         ILogger<OrderingGatewayController> logger)
        {
            _restClient = restClient;
            _configuration = configuration;
            _logger = logger;
            _gRpcFeatureFlag = Convert.ToBoolean(configuration[SystemConstants.gRPCFeatureFlag] ?? "false");
        }

        /// <summary>
        /// Get Details for Specified Order
        /// </summary>
        /// <param name="orderId">Identifer for an order</param>
        /// <returns>Details for specified Order</returns>
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("v{version:apiVersion}/Order/{orderId}", Name = "GetOrderGatewayRoute")]
        //[HttpGet("Order/{orderId}", Name = "GetOrderGatewayRoute")]
        public async Task<IActionResult> GetOrder(string orderId, string version)
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();
            
            var response = await _restClient.GetAsync<dynamic>(ServiceEnum.Ordering,
                $"api/ordering/v{version}/Order/{orderId}", correlationToken);

            if (response.Data == null)
            {
                _logger.LogError(LoggingEvents.GetOrder, $"Ordering: No order found for {correlationToken}");
                return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetOrder, $"Ordering: Order Found for {correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <summary>
        /// Gets All Orders
        /// </summary>
        /// <param name="orderId">Identifer for an order</param>
        /// <returns>Details for specified Order</returns>
        [ProducesResponseType(typeof(List<OrdersDto>), 200)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("Orders", Name = "GetOrdersGatewayRoute")]
        public async Task<IActionResult> GetOrders()
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response  = await _restClient.GetAsync<List<OrdersDto>>(ServiceEnum.Ordering,
            $"api/Ordering/Orders", correlationToken);

            if (response.Data == null || response.Data.Count < 1)
            {
                _logger.LogError(LoggingEvents.GetOrders, $"Ordering: No order found for{correlationToken}");
                return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
            }

            _logger.LogInformation(LoggingEvents.GetOrders, $"Ordering: Order Found for {correlationToken}");

            return new ObjectResult(response.Data);
        }

        /// <returns>Returns Http Status Code 500 - Internal Server Error</returns>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("SimulateError", Name = "GatewaySimulateErrorRoute")]
        public async Task<IActionResult> SimulateError()
        {
            var correlationToken = CorrelationTokenManager.GenerateToken();

            var response = await _restClient.GetAsync<List<OrdersDto>>(ServiceEnum.Ordering,
                $"api/Ordering/Orders/SimulateError", correlationToken);

            return StatusCode((int)response.HttpStatusCode, response.ErrorMessage);
        }
    }
}



