using RabbitMQ.Client;

namespace EmailService.Core.Infrastructure
{
    public class RabbitMQConnection : IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private bool _disposed;

        public RabbitMQConnection(
            IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && _disposed;

        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            if (IsConnected)
            {
                return _connection;   
            }
            _connection = await _connectionFactory.CreateConnectionAsync();
            return _connection;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _connection?.Dispose();
        }
    }
}
