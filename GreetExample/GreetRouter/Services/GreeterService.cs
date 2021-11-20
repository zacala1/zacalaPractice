using Greeting;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreetRouter
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly Dictionary<string, Greeter.GreeterClient> _clients = new();

        public GreeterService(ILogger<GreeterService> logger, GrpcClientFactory grpcClientFactory)
        {
            _logger = logger;
            _clients.Add("greeter01", grpcClientFactory.CreateClient<Greeter.GreeterClient>("greeter01")); 
        }

        public override async Task<HelloReply> SayHelloUnary(HelloRequest request, ServerCallContext context)
        {
            var client = _clients["greeter01"];

            _logger.LogInformation($"Sending hello to {request.Name}");
            var response = await client.SayHelloUnaryAsync(request, context.RequestHeaders, context.Deadline, context.CancellationToken);
            
            return response;
        }

        public override async Task SayHelloServerStreaming(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var client = _clients["greeter01"];
            using var call = client.SayHelloServerStreaming(request, context.RequestHeaders, context.Deadline, context.CancellationToken);
            await foreach (var message in call.ResponseStream.ReadAllAsync())
            {
                _logger.LogInformation($"Sending greeting {message}.");
                await responseStream.WriteAsync(call.ResponseStream.Current);
            }
        }

        public override async Task<HelloReply> SayHelloClientStreaming(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            var client = _clients["greeter01"];
            using var call = client.SayHelloClientStreaming(context.RequestHeaders, context.Deadline, context.CancellationToken);
            await foreach (var request in requestStream.ReadAllAsync())
            {
                await call.RequestStream.WriteAsync(request);
            }
            await call.RequestStream.CompleteAsync();

            return call.ResponseAsync.Result;
        }

        public override async Task SayHelloBidirectionalStreaming(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var client = _clients["greeter01"];
            using var call = client.SayHelloBidirectionalStreaming(context.RequestHeaders, context.Deadline, context.CancellationToken);
            var requestTask = Task.Run(async () =>
            {
                await foreach (var message in requestStream.ReadAllAsync())
                {
                    await call.RequestStream.WriteAsync(message);
                }
                await call.RequestStream.CompleteAsync();
            });
            var responseTask = Task.Run(async () =>
            {
                await foreach (var message in call.ResponseStream.ReadAllAsync())
                {
                    await responseStream.WriteAsync(message);
                }
            });
            await Task.WhenAll(requestTask, responseTask);
        }
    }
}
