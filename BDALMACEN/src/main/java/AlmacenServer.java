import com.mongodb.client.*;
import com.rabbitmq.client.*;

import java.util.ArrayList;

public class AlmacenServer {

    private static final String RPC_QUEUE_NAME1 = "rpc_queue";
    private static final String RPC_QUEUE_NAME2 = "rpc_queue2";

    private Connection connection;
    private Channel channel;

    private AlmacenDB almacenDB;

    AlmacenServer() throws Exception{
        almacenDB = new AlmacenDB();
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost("localhost");

        connection = factory.newConnection();
        channel = connection.createChannel();
        channel.queueDeclare(RPC_QUEUE_NAME1, false, false, false, null);
        channel.queuePurge(RPC_QUEUE_NAME1);
        channel.queueDeclare(RPC_QUEUE_NAME2, false, false, false, null);
        channel.queuePurge(RPC_QUEUE_NAME2);
        channel.basicQos(1);

        //listen();
    }
    private void listen() throws Exception {
        System.out.println(" [x] Awaiting RPC requests");

        //deliverCallback Send a response
        DeliverCallback deliverCallback1 = (consumerTag, delivery) -> {
            AMQP.BasicProperties replyProps = new AMQP.BasicProperties
                    .Builder()
                    .correlationId(delivery.getProperties().getCorrelationId())
                    .build();

            String response = "";
            try {
                String message = new String(delivery.getBody(), "UTF-8");
                System.out.println(" [.] fib(" + message + ")");
                response = almacenDB.find(message);

            } catch (RuntimeException e) {
                System.out.println(" [.] " + e);
            } finally {
                //envia respuesta a la cola 'delivery.getProperties().getReplyTo()'
                channel.basicPublish("", delivery.getProperties().getReplyTo(), replyProps, response.getBytes("UTF-8"));

                //envia un acknowlegement con respectivo tag
                channel.basicAck(delivery.getEnvelope().getDeliveryTag(), false);
            }
        };
        DeliverCallback deliverCallback2 = (consumerTag, delivery) -> {
            AMQP.BasicProperties replyProps = new AMQP.BasicProperties
                    .Builder()
                    .correlationId(delivery.getProperties().getCorrelationId())
                    .build();

            String response = "";
            try {
                String message = new String(delivery.getBody(), "UTF-8");

                System.out.println(" [.] fib(" + message + ")");

                String[] arrStr = message.split(" ");
                boolean check = true;

                for(String str:arrStr){
                    String[] res = str.split(",");
                    if(!almacenDB.confirm(Integer.parseInt(res[0]),Integer.parseInt(res[1]))){
                        check = false;
                        break;
                    }
                }
                if(check){
                    for(String str:arrStr){
                        String[] res = str.split(",");
                        almacenDB.update(Integer.parseInt(res[0]),Integer.parseInt(res[1]));
                    }
                    response =  "1";
                }else response="0";

            } catch (RuntimeException e) {
                System.out.println(" [.] " + e);
            } finally {
                //envia respuesta a la cola 'delivery.getProperties().getReplyTo()'
                channel.basicPublish("", delivery.getProperties().getReplyTo(), replyProps, response.getBytes("UTF-8"));

                //envia un acknowlegement con respectivo tag
                channel.basicAck(delivery.getEnvelope().getDeliveryTag(), false);
            }
        };
        channel.basicConsume(RPC_QUEUE_NAME1, false, deliverCallback1, (consumerTag -> {}));
        channel.basicConsume(RPC_QUEUE_NAME2, false, deliverCallback2, (consumerTag -> {}));
    }
    public static void main(String[] args) throws Exception {

        AlmacenServer almacenServer = new AlmacenServer();
        almacenServer.listen();
    }


}
