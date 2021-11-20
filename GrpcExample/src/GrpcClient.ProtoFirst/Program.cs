using Grpc.Core;
using Grpc.Net.Client;
using GrpcModel.ProtoFirst;
using System;
using System.Threading.Tasks;

namespace GrpcClient.ProtoFirst
{
    class Program
    {
        static void Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new WeatherForecaster.WeatherForecasterClient(channel);

            //await UnaryCallExample(client);

            //await ClientStreamingCallExample(client);

            //await ServerStreamingCallExample(client);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        //private static async Task UnaryCallExample(WeatherForecaster.WeatherForecasterClient client)
        //{
        //    client.DeleteWeatherForecastsAsync()
        //}

        //private static async Task ClientStreamingCallExample(WeatherForecaster.WeatherForecasterClient client)
        //{
        //    using var call = client.AccumulateCount();
        //    for (var i = 0; i < 3; i++)
        //    {
        //        var count = Random.Next(5);
        //        Console.WriteLine($"Accumulating with {count}");
        //        await call.RequestStream.WriteAsync(new CounterRequest { Count = count });
        //        await Task.Delay(TimeSpan.FromSeconds(2));
        //    }

        //    await call.RequestStream.CompleteAsync();

        //    var response = await call;
        //    Console.WriteLine($"Count: {response.Count}");
        //}

        //private static async Task ServerStreamingCallExample(WeatherForecaster.WeatherForecasterClient client)
        //{
        //    using var call = client.Countdown(new Empty());

        //    await foreach (var message in call.ResponseStream.ReadAllAsync())
        //    {
        //        Console.WriteLine($"Countdown: {message.Count}");
        //    }
        //}
    }
}