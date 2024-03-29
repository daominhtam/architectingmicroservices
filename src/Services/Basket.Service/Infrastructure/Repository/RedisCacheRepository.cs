﻿using Basket.API.Contracts;
using Basket.API.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Basket.API.Domain.Entities;
using Microsoft.ApplicationInsights;

namespace Basket.API.Infrastructure.Repository
{
    public class RedisCacheRepository : IDistributedCacheRepository
    {
        private readonly ILogger<RedisCacheRepository> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        
        public RedisCacheRepository(ILoggerFactory loggerFactory, ConnectionMultiplexer redis)
        {
            _logger = loggerFactory.CreateLogger<RedisCacheRepository>();
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<bool> DeleteBasketAsync(string id, TelemetryClient telemetryClient)
        {
            // Telemetry variables
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                return await _database.KeyDeleteAsync(id);
                success = true;
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.TrackDependency("RedisCache", "DeleteBasket", success.ToString(), startTime, timer.Elapsed, success);
            }

            return success;
        }
        
        public async Task<BasketEntity> GetBasketAsync(string basketid, 
                                                       string correlationToken,
                                                       TelemetryClient telemetryClient)
        {
            // Telemetry variables
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var returnValue = "Not Found";

            try
            {
                var data = await _database.StringGetAsync(basketid);

                if (data.IsNullOrEmpty)
                {
                    return null;
                }
                else
                {
                    var basketdata = JsonConvert.DeserializeObject<BasketEntity>(data);
                    success = true;
                    returnValue = "Found";
                    return basketdata;
                }
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.TrackDependency("RedisCache", "GetBasket", returnValue, startTime, timer.Elapsed, success);
            }

            //var data = await _database.StringGetAsync(basketid);
            //if (data.IsNullOrEmpty)
            //{
            //    return null;
            //}
            //var basketdata = JsonConvert.DeserializeObject<BasketEntity>(data);
            //return basketdata;
        }

        public async Task<BasketEntity> UpdateBasketAsync(BasketEntity basketEntity,
                                                          string correlationToken,
                                                          TelemetryClient telemetryClient)
        {
            // Telemetry variables
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var returnValue = "Not Found";

            try
            {
                var created =
                    await _database.StringSetAsync(basketEntity.BasketId, JsonConvert.SerializeObject(basketEntity));
                
                // _database.SetAdd()

                if (!created)
                {
                    _logger.LogInformation("Redis cache could not persist an item.");
                    return null;
                }
                else
                {
                    success = true;
                    returnValue = basketEntity.BasketId;
                    _logger.LogInformation("Redis cache persisted item succesfully.");
                }
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                _logger.LogError(ex.ToString());
                throw;
            }
            finally
            {
                telemetryClient.TrackDependency("RedisCache", "UpdateBasket", returnValue, startTime, timer.Elapsed, success);
            }
            
            return await  GetBasketAsync(returnValue);
            //return null;
        }

        public async  Task<List<BasketEntity>> GetAllBaskets(string correlationToken,
                                                             TelemetryClient telemetryClient)
        {
            // Telemetry variables
            var success = false;
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var returnValue = "Not Found";

            var basketall = new List<BasketEntity>();

            try
            {
                var server = GetServer();
                var data = server.Keys();

                foreach (string key in data)
                {

                    var basket = await GetBasketAsync(key);
                    basketall.Add(basket);
                    returnValue = "Found";
                }
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.TrackDependency("RedisCache", "GetAllBaskets", returnValue, startTime, timer.Elapsed, success);
            }
            return basketall;

            //var server = GetServer();
            //var data = server.Keys();

            //foreach(string key in data)
            //{

            //    var basket = await GetBasketAsync(key);
            //    basketall.Add(basket);
            //}

            return basketall;
        }

        //public IEnumerable<string> GetUsers()
        //{
        //    var server = GetServer();
        //    var data = server.Keys();

        //    return data?.Select(k => k.ToString());
        //}

        private async Task<BasketEntity> GetBasketAsync(string basketid)
        {
            var data = await _database.StringGetAsync(basketid);

            if (data.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BasketEntity>(data);
        }

        private IServer GetServer()
        {
            var endpoint = _redis.GetEndPoints();
            return _redis.GetServer(endpoint.First());
        }

    }
}
