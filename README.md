1. Open solution TestRBMQ
2. Config RabbitMQ server in appsettings.json for each project

	  "RabbitMQ": {
	    "Host": "localhost",
	    "Queue": "message-queue"
	  }

3. Run at the same time 2 projects Sender and Receiver
4. Test system
		Call Api:
		  - Name : /topic/test-concurrent
		  - Method: POST
		  - Body: 
			  	{
					"numberConcurrent": 10, // number of concurrency
					"delay": 1000, //Time delay between each messages in second
					"totalPerConcurrent": 10 //Total message send from each thread
				}
