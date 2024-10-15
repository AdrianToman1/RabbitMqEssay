using System.Text;
using ConsumerConsoleApp;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Hello, World!");

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true);

IConfiguration configuration = builder.Build();

var serverConnection = new MessageBrokerSettings();
configuration.GetSection("MessageBroker").Bind(serverConnection);



var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "hello",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");
};
channel.BasicConsume(queue: "hello",
    autoAck: true,
    consumer: consumer);


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

        //cfg.ReceiveEndpoint("incoming_test_messages", e => { e.Consumer<MyConsumer>(); });
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