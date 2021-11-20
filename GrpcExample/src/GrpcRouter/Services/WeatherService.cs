using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using GrpcModel.ProtoFirst;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcRouter.ProtoFirst
{
    public class WeatherService : WeatherForecaster.WeatherForecasterBase
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly WeatherForecaster.WeatherForecasterClient[] _client;
        public WeatherService(ILogger<WeatherService> logger, GrpcClientFactory grpcClientFactory)
        {
            _logger = logger;
            _client[0] = grpcClientFactory.CreateClient<WeatherForecaster.WeatherForecasterClient>("ForecastClient01");
            _client[1] = grpcClientFactory.CreateClient<WeatherForecaster.WeatherForecasterClient>("ForecastClient02");
        }

        public override async Task<ResetForcastResponse> ResetForecastUnary(Empty request, ServerCallContext context)
        {
            var client = _client[0];
            return await client.ResetForecastUnaryAsync(request, context.RequestHeaders, context.Deadline, context.CancellationToken);
        }

        public override async Task<SetForecastResponse> SetForecastStream(IAsyncStreamReader<SetForecastRequest> requestStream, ServerCallContext context)
        {
            var client = _client[0];
            using var call = client.SetForecastStream(context.RequestHeaders, context.Deadline, context.CancellationToken);
            await foreach(var request in requestStream.ReadAllAsync())
            {
                await call.RequestStream.WriteAsync(request);
            }

            return call.ResponseAsync.Result;
        }

        public override async Task GetForecastStream(GetForecastRequest request, IServerStreamWriter<GetForecastResponse> responseStream, ServerCallContext context)
        {
            var client = _client[0];
            using var call = client.GetForecastStream(request, context.RequestHeaders, context.Deadline, context.CancellationToken);
            while(await call.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(call.ResponseStream.Current);
            }
        }
    }
}
