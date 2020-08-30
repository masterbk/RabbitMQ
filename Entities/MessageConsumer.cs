using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Entities
{
    public class MessageConsumer : IConsumer<Message>
    {
        private readonly ILogger<MessageConsumer> _logger;
        public MessageConsumer(ILogger<MessageConsumer> logger)
        {
            _logger = logger;
        }
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        public Task Consume(ConsumeContext<Message> context)
        {
            try
            {
                // Set Status to Locked
                _readWriteLock.EnterWriteLock();
                try
                {
                    // Append text to the file
                    using (StreamWriter sw = File.AppendText("message.csv"))
                    {
                        sw.WriteLine("{0},{1},{2}", context.Message.SendedDate.ToString("yyyy-MM-ddTHH:mm:ss"), context.Message.ClientId, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                        sw.Close();
                    }
                }
                finally
                {
                    // Release lock
                    _readWriteLock.ExitWriteLock();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            
            return Task.CompletedTask;
        }
    }
}
