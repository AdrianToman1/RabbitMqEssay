using ConsumerConsoleApp;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true);

IConfiguration configuration = builder.Build();

var serverConnection = new MessageBrokerSettings();
configuration.GetSection("MessageBroker").Bind(serverConnection);

IBusControl busControl = null;
try
{
    busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host(serverConnection.Host, "/", h =>
        {
            h.Username(serverConnection.Username);
            h.Password(serverConnection.Password);
        });

        cfg.ReceiveEndpoint("my_queue", e => { e.Consumer<MyConsumer>(); });
    });


    busControl.Start(); // This is non-blocking
    Console.WriteLine("Press any key to exit");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

Console.ReadKey();

if (busControl != null)
{
    busControl.Stop();
}


public class MyConsumer : IConsumer<TestMessage>
{
    public async Task Consume(ConsumeContext<TestMessage> context)
    {
        Console.WriteLine($"Received: {context.Message.Text}");
    }
}