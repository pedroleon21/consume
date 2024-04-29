using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Net.Mail;

class ConsumeEmailQueue
{
    private const string QueueName = "email";

    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory()
        { 
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                Console.WriteLine("Lendo evento");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Deserialize the message content into an appropriate object (replace with your actual email data structure)
                var emailData = JsonSerializer.Deserialize<Email>(message);

                // Send the email using your implementation of IEmailSender
                SendEmail(emailData);

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                Console.WriteLine("fim do evento");
            };

            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            Console.WriteLine("Consuming email queue...");
            await Task.Run(() => {
                Console.WriteLine("Digite enter para finalizar");
                Console.ReadLine();
            }); // Keep the console application running
        }
    }

    private static async void SendEmail(Email emailData) // Replace with actual email sending logic
    {
        MailMessage message = new MailMessage();
        message.Subject = emailData.Subject;
        message.Body = emailData.Body;
        message.IsBodyHtml = true;
        message.To.Add(emailData.To);

        string host = "localhost";
        int port = 1025;
        string fromAddress = "store@app.com";

        message.From = new MailAddress(fromAddress);

        using (var smtpClient = new SmtpClient(host, port))
        {
            await smtpClient.SendMailAsync(message);
        }
    }
}

public class Email // Replace with your actual email data structure
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
