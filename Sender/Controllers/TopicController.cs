using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sender.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly IBusControl _busControl;
        private readonly ILogger<TopicController> _logger;
        private readonly IConfiguration _configuration;
        public TopicController(IBusControl busControl, ILogger<TopicController> logger, IConfiguration configuration)
        {
            _busControl = busControl;
            _logger = logger;
            _configuration = configuration;
        }
        [HttpPost("test-concurrent")]
        public IActionResult Index([FromBody]InputTest inputTest )
        {
            List<Task> listTask = new List<Task>();

            for (int concurrent = 0; concurrent < inputTest.NumberConcurrent; concurrent++)
            {
                var current = concurrent;
                listTask.Add(Task.Factory.StartNew(async() =>
                {
                    await Dowork(current, inputTest.Delay, inputTest.TotalPerConcurrent);
                }));
            }

            return Ok("Starting...");
        }

        private async Task Dowork(int clientId, int delay, int totalRequest)
        {
            try
            {
                for (var i = 0; i < totalRequest; i++)
                {
                    Uri uri = new Uri("queue:"+ _configuration.GetSection("RabbitMQ:Queue").Get<string>());
                    var endpoint = await _busControl.GetSendEndpoint(uri);
                    await endpoint.Send(new Message
                    {
                        ClientId = clientId,
                        SendedDate = DateTime.Now
                    });

                    await Task.Delay(delay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
