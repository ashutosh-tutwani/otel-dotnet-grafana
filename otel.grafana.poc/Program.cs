using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace otel.grafana.poc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            Action<ResourceBuilder> configureResource =
               r => r.AddTelemetrySdk()
                   .AddService(serviceName: "weather-api",
               serviceNamespace: "weather-app",
                       serviceVersion: "1.2.2.3",
                       serviceInstanceId: "1.1");

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(configureResource)
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
                });


            builder.Services
               .AddOpenTelemetry()
                .ConfigureResource(configureResource)
                .WithMetrics(metricProviderBuilder =>
                {
                    metricProviderBuilder
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddPrometheusExporter();
                    //.AddOtlpExporter(otlpOptions =>
                    //{
                    //    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

                    //    otlpOptions.Endpoint = new Uri("http://localhost:4318" + "/v1/metrics");

                    //});

                });


            ///////////////////////////////////////
            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            app.MapControllers();

            app.Run();
        }
    }
}