using Contracts;
using MassTransit;

Console.WriteLine("Hello, World!");
IBusControl busControl = null;
try
{
    busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("my_queue", e =>
        {
            e.Consumer<MyConsumer>();
        });
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




