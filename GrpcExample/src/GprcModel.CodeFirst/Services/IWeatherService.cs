using GprcModel1.Models;
using ProtoBuf;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GprcModel1.Services
{
    [Service(name:"WeatherForecaster")]
    public interface IWeatherService
    {
        ValueTask<ResetForcastResponse> ResetForecastUnary(CallContext context = default);
        ValueTask<SetForecastResponse> SetForecastStream(IAsyncEnumerable<SetForecastRequest> request, CallContext context = default);
        IAsyncEnumerable<GetForecastResponse> GetForecastStream(GetForecastRequest request, CallContext context = default);
    }

    [ProtoContract]
    public class ResetForcastResponse
    {
        [ProtoMember(1)]
        public List<WeatherForecast> Forecasts { get; set; }
    }

    [ProtoContract]
    public class SetForecastRequest
    {
        [ProtoMember(1)]
        public WeatherForecast Forecast { get; set; }
    }

    [ProtoContract]
    public class SetForecastResponse
    {
        [ProtoMember(1)]
        public int Count { get; set; }
    }

    [ProtoContract]
    public class GetForecastRequest
    {
        [ProtoMember(1)]
        public string Location { get; set; }
    }

    [ProtoContract]
    public class GetForecastResponse
    {
        [ProtoMember(1)]
        public List<WeatherForecast> Forecasts { get; set; }
    }

    [ProtoContract]
    public class WeatherForecast
    {
        [ProtoMember(1, DataFormat = DataFormat.WellKnown)]
        public DateTime Date { get; set; }
        [ProtoMember(2)]
        public int TemperatureC { get; set; }
        [ProtoMember(3)]
        public string Location { get; set; }
        [ProtoMember(4)]
        public string Summary { get; set; }
    }
}
