using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcModel.ProtoFirst;
using GrpcServer.ProtoFirst.Test.TestFixture;
using NUnit.Framework;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Ref: https://dzone.com/articles/integration-tests-for-grpc-services-in-aspnet-core
/// </summary>
namespace GrpcServer.ProtoFirst.Test
{
    public class IntergrationTests
    {
        public IntergrationTests()
        {
            var testServerFixture = new TestServerFixture();
            var channel = testServerFixture.GrpcChannel;
            _client = new WeatherForecaster.WeatherForecasterClient(channel);
        }

        //private readonly ILazyCounterService _clientService;
        private readonly WeatherForecaster.WeatherForecasterClient _client;
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestResetForecastUnary()
        {
            var response = await _client.ResetForecastUnaryAsync(new Empty());
            Console.WriteLine(response.Forecasts.ToString());
            Console.WriteLine(JsonSerializer.Serialize(response.Forecasts));
            Assert.Pass();
        }

        [Test]
        public async Task TestSetForecastStream()
        {
            using var call = _client.SetForecastStream();
            for (int i = 0; i < 10; i++)
            {
                var forcast = new WeatherForecast() { Location = "Suwon", Date = DateTime.UtcNow.ToTimestamp(), TemperatureC = 20 };
                var request = new SetForecastRequest() { Forecast = forcast };
                await call.RequestStream.WriteAsync(request);
                await Task.Delay(100);
            }
            await call.RequestStream.CompleteAsync();
            Assert.AreEqual(10, call.ResponseAsync.Result.Count);
        }

        [Test]
        public async Task TestGetForecastStream()
        {
            var request = new GetForecastRequest() { Location = "Korea" };
            var cts = new CancellationTokenSource();
            using var call = _client.GetForecastStream(request, cancellationToken:cts.Token);
            try
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(JsonSerializer.Serialize(response.Forecasts));
                }
            }
            catch(RpcException ex)
            {
                Console.WriteLine(ex.Message);
                Assert.AreEqual(StatusCode.Cancelled, ex.StatusCode);
                return;
            }
            Assert.Fail();
        }
    }
}