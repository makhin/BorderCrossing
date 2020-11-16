using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using BorderCrossing.DbContext;
using BorderCrossing.Models;
using BorderCrossing.Models.Google;
using Microsoft.Extensions.Caching.Memory;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingServiceWebWrapper
    {
        Task<QueryRequest> GetQueryRequestAsync(string requestId);
        Task<List<CheckPoint>> ParseLocationHistoryAsync(string requestId, QueryRequest model,  ProgressChangedEventHandler callback);
        Task UpdateResultAsync(string requestId, List<CheckPoint> checkPoints);
        Task<string> AddNewRequestAsync(string ipAddress, string userAgent);
        Task PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, string requestId, ProgressChangedEventHandler callback);
        Task<List<CheckPoint>> GetResultAsync(string requestId);
    }

    public class BorderCrossingServiceWebWrapper : IBorderCrossingServiceWebWrapper
    {
        private readonly IBorderCrossingService _borderCrossingService;
        private readonly IBorderCrossingRepository _repository;
        private readonly IMemoryCache _cache;

        public BorderCrossingServiceWebWrapper(IBorderCrossingService borderCrossingService, IBorderCrossingRepository repository, IMemoryCache cache)
        {
            _borderCrossingService = borderCrossingService;
            _repository = repository;
            _cache = cache;
        }

        public Task<QueryRequest> GetQueryRequestAsync(string requestId)
        {
            var locationHistory = GetLocationHistory(requestId);
            return _borderCrossingService.GetQueryRequestAsync(locationHistory);
        }

        public Task<List<CheckPoint>> ParseLocationHistoryAsync(string requestId, QueryRequest model, ProgressChangedEventHandler callback)
        {
            var locationHistory = GetLocationHistory(requestId);
            return _borderCrossingService.ParseLocationHistoryAsync(locationHistory, model, callback);
        }

        public async Task UpdateResultAsync(string requestId, List<CheckPoint> checkPoints)
        {
            await _repository.UpdateResultAsync(requestId, checkPoints);
        }

        public async Task<string> AddNewRequestAsync(string ipAddress, string userAgent)
        {
            var request = await _repository.AddNewRequest(Guid.NewGuid(), ipAddress, userAgent);
            return request.RequestId.ToString();
        }
        public async Task PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, string requestId, ProgressChangedEventHandler callback)
        {
            _ = _repository.SaveLocationHistoryFileAsync(memoryStream, fileName, requestId);
            var locationHistory = await BorderCrossingHelper.ExtractJsonAsync(memoryStream, callback);
            AddLocationHistory(locationHistory, requestId);
        }

        public async Task<List<CheckPoint>> GetResultAsync(string requestId)
        {
            return await _repository.GetResultAsync(requestId);
        }

        private void AddLocationHistory(LocationHistory locationHistory, string requestId)
        {
            _cache.Set(requestId, locationHistory, TimeSpan.FromMinutes(15));
        }

        private LocationHistory GetLocationHistory(string requestId)
        {
            return _cache.Get<LocationHistory>(requestId);
        }
    }
}
