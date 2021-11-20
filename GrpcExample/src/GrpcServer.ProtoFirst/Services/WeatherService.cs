using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcModel.ProtoFirst;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrpcService.ProtoFirst
{
    public class WeatherService : WeatherForecaster.WeatherForecasterBase
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly List<WeatherForecast> _weatherForcasts;

        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
            _weatherForcasts = new List<WeatherForecast>
            {
                new WeatherForecast{Location = "Korea", Date = DateTime.UtcNow.ToTimestamp(), TemperatureC = 28 },
                new WeatherForecast{Location = "U.S", Date = DateTime.UtcNow.ToTimestamp(), TemperatureC = 23 },
                new WeatherForecast{Location = "Russia", Date = DateTime.UtcNow.ToTimestamp(), TemperatureC = -16 },
                new WeatherForecast{Location = "Japan", Date = DateTime.UtcNow.ToTimestamp(), TemperatureC = 34 },
            };
        }

        public override Task<ResetForcastResponse> ResetForecastUnary(Empty request, ServerCallContext context)
        {
            List<WeatherForecast> temp = new();
            temp.AddRange(_weatherForcasts);
            _weatherForcasts.Clear();

            var response = new ResetForcastResponse();
            response.Forecasts.AddRange(temp);
            return Task.FromResult(response);
        }

        public override async Task<SetForecastResponse> SetForecastStream(IAsyncStreamReader<SetForecastRequest> requestStream, ServerCallContext context)
        {
            int count = 0;
            await foreach (var message in requestStream.ReadAllAsync())
            {
                if (!_weatherForcasts.Contains(message.Forecast))
                {
                    _weatherForcasts.Add(message.Forecast);
                    count++;
                }
            }

            var response = new SetForecastResponse();
            response.Count = count;
            return response;
        }

        public override async Task GetForecastStream(GetForecastRequest request, IServerStreamWriter<GetForecastResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                Predicate<WeatherForecast> match = (s) => { return s.Location.Equals(request.Location); };
                List<WeatherForecast> temp = _weatherForcasts.FindAll(match);

                var response = new GetForecastResponse();
                response.Forecasts.AddRange(temp);
                await responseStream.WriteAsync(response);
                await Task.Delay(TimeSpan.FromMilliseconds(100), context.CancellationToken);
            }
        }
    }
}
