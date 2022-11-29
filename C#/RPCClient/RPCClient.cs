using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public class RpcClient{
 //   private const string QUEUE_NAME = "rpc_queue";

    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper =
                new ConcurrentDictionary<string, TaskCompletionSource<string>>();

    public RpcClient()
    {

        var factory = new ConnectionFactory() { HostName = "localhost" };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        // declare a server-named queue
        replyQueueName = channel.QueueDeclare(queue: "").QueueName;
        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string> tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        channel.BasicConsume(
          consumer: consumer,
          queue: replyQueueName,
          autoAck: true);
    }

    public Task<string> CallAsync(string QUEUE_NAME,string message, CancellationToken cancellationToken = default(CancellationToken))
    {
      IBasicProperties props = channel.CreateBasicProperties();
      var correlationId = Guid.NewGuid().ToString();
      props.CorrelationId = correlationId;
      props.ReplyTo = replyQueueName;
      var messageBytes = Encoding.UTF8.GetBytes(message);
      var tcs = new TaskCompletionSource<string>();
      callbackMapper.TryAdd(correlationId, tcs);

      channel.BasicPublish(
          exchange: "",
          routingKey: QUEUE_NAME,
          basicProperties: props,
          body: messageBytes);

      cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
      return tcs.Task;
    }

    public void Close()
    {
        channel.Close();
        connection.Close();        
    }
}


class NewTask{

    public NewTask(string hostname, string queue_name, string message){
        var factory = new ConnectionFactory() { HostName = hostname };
        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
        channel.QueueDeclare(queue: queue_name,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: "",
                                routingKey: queue_name,
                                basicProperties: properties,
                                body: body);
        Console.WriteLine(" [x] Sent {0}", message);
        }

    }
}


public class Rpc{

  public static void Main(string[] args){
      Console.WriteLine("RPC Client");
      string n = args.Length > 0 ? args[0] : "30";
      Task t = InvokeAsync();

      t.Wait();

      Console.WriteLine(" Press [enter] to exit.");
      Console.ReadLine();
  }

  private static async Task InvokeAsync(){
   /* List<string> mylist = new List<string>();

    //  
     # var rpcClient = new RpcClient();
      Console.WriteLine(" [x] Requesting fib({0})", n);
        
      var response = await rpcClient.CallAsync(n.ToString());
    
      Console.WriteLine(" [.] Got '{0}'", response);

      rpcClient.Close();
  

      if(response!="-1"){
      mylist.Add(response);        
      Console.WriteLine(response);
      ///////////////////////////////////////////////////////////////////////////////////////////////
      var rpcClient2 = new RpcClient();
      string x = "1";
      // cantidad
      // x = response ;x+="," 
      // enviar id,cantidad 
      var response2 = await rpcClient2.CallAsync(x.ToString());
      Console.WriteLine(response2);
      rpcClient2.Close();
    
      if(response2=="1"){
             mylist.Add(x);
             int go = mylist.Count;
             string ret = "";

            for(int i=0;i<go;i++){
             ret+=(mylist[i]);
              if(i!=go-1)
                    ret+=" ";
            }     

     Console.WriteLine("enviando a python");
     NewTask zz =new NewTask("localhost","task_queue",ret);
    // 191.168.43.155    
    //NewTask zz = new NewTask("192.168.43.155","task_queue");
      }
        
    }

    else {
        Console.WriteLine("No se pudo encontrar su paquete");
    }
    
   //NewTask zz =new NewTask("localhost","task_queue");
    // 191.168.43.155    
    //NewTask zz = new NewTask("192.168.43.155","task_queue");
    */

  var list = new List<KeyValuePair<string, string>>();
	int n = 5;
	Console.WriteLine("Name : ");
	var rpcClient = new RpcClient();

	for(int i=0;i<n;i++){
		String read_user = Console.ReadLine();
		if(read_user == "q") break;
		var response = await rpcClient.CallAsync("rpc_queue",read_user.ToString());
		if(response!="-1"){
			list.Add(new KeyValuePair<string, string>(response, read_user));
		}
	}
  rpcClient.Close();
  /*
  foreach(KeyValuePair<string,string> kvp in list){
		string message = kvp.Key;
		message+=" ";
		message+=kvp.Value;
		Console.WriteLine(message);
	}	
  */
  var ret = new List<KeyValuePair<string, string>>();
	
	Console.WriteLine("Cantidad : ");
	
	foreach( KeyValuePair<string, string> kvp in list ){
		Console.WriteLine("Digita la cantidad = {0}", kvp.Value);
		String read_cant = Console.ReadLine();
    if(kvp.Key!="-1"){
		  ret.Add(new KeyValuePair<string, string>(kvp.Key,read_cant));
    }
	}

	
  var rpcClient2 = new RpcClient();
	int s = ret.Count;
	int ii = 0;
	string message ="";
  foreach(KeyValuePair<string,string> kvp in ret){	
		message+= kvp.Key;
		message+=",";
		message+=kvp.Value;
		if(ii!=s-1)message+=" ";
    ii++;
	}

  Console.WriteLine(message);
  var response2 = await rpcClient2.CallAsync("rpc_queue2",message.ToString());
  Console.WriteLine(response2);
  rpcClient2.Close();
  

    Console.WriteLine("enviando a python");
    message = "ricardo ricardo@uni.pe";
	
  /*
  Emanuel aca tienes que enviar como quieres:
  envias message = "ricardo ricardo@uni.pe" -> first
  en el siguiente bucle agarra a cada message y envia :


  */
   Console.WriteLine(message);
	 NewTask zz =new NewTask("localhost","task_queue",message);
	
	foreach(KeyValuePair<string,string> kvp in ret){
		message = kvp.Key;
		message+=" ";
		message+=kvp.Value;
		Console.WriteLine(message);
    zz = new NewTask("localhost","task_queue",message);
	}	  
  }
}