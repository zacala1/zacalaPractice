using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;

namespace GreetRouter.Extensions
{
    public static class GrpcClientServiceDefaultExtension
    {
        /// <summary>
        /// Adds a grpc client with default options
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClientWithDefaultOptions<TClient>(this IServiceCollection services, string name, Uri address) where TClient : class
        {
            // Keep alive pings
            // https://docs.microsoft.com/en-us/aspnet/core/grpc/performance?view=aspnetcore-5.0
            var defaultHttpHandler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true
            };

            // Configuration a gRPC retry policy
            // https://docs.microsoft.com/en-us/aspnet/core/grpc/retries?view=aspnetcore-5.0
            var drfaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable }
                }
            };
            var defaultServiceConfig = new ServiceConfig
            {
                MethodConfigs = { drfaultMethodConfig }
            };

            // Add gRPC Client to Factory
            // https://docs.microsoft.com/en-us/aspnet/core/grpc/clientfactory?view=aspnetcore-5.0
            services
                .AddGrpcClient<TClient>(name, o =>
                {
                    o.Address = address;
                })
                .ConfigureChannel(o =>
                {
                    o.HttpHandler = defaultHttpHandler;
                    o.ServiceConfig = defaultServiceConfig;
                })
                .EnableCallContextPropagation();
            return services;
        }
    }
}
