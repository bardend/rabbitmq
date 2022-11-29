import psycopg2
import pika


def process_invoice(connection, data):
    lines = data.split("\n")
    for i in range(len(lines)):
        lines[i] = lines[i].split(' ')

    invoice_id = insert_invoice(connection, lines[0][0], lines[0][1])

    for i in range(1, len(lines)):
        insert_invoice_detail(connection, invoice_id, lines[i][0], lines[i][1])


def insert_invoice(connection, customer_name, customer_email):
    cursor = connection.cursor()

    cursor.execute("INSERT INTO invoices(customer_name, customer_email) VALUES ('{}', '{}') RETURNING invoice_id".format(customer_name, customer_email))
    connection.commit()

    invoice_id = cursor.fetchone()[0]

    return invoice_id


def insert_invoice_detail(connection, invoice_id, product_id, quantity):
    connection.cursor().execute("INSERT INTO invoice_details VALUES ({}, {}, {})".format(invoice_id, product_id, quantity))
    connection.commit()


def create_tables(connection):
    cursor = connection.cursor()
    cursor.execute("CREATE TABLE invoices (\
                   invoice_id SERIAL PRIMARY KEY,\
                   customer_name VARCHAR (50) NOT NULL,\
                   customer_email VARCHAR (100) NOT NULL);")

    cursor.execute("CREATE TABLE invoice_details (\
                   invoice_id INT NOT NULL,\
                   product_id INT NOT NULL,\
                   quantity INT NOT NULL,\
                   FOREIGN KEY (invoice_id)\
                   REFERENCES invoices (invoice_id));")

    connection.commit()


def restart_tables(connection):
    cursor = connection.cursor()
    cursor.execute("DROP TABLE invoices, invoice_details")
    connection.commit()

    create_tables(connection)


def clear_tables(connection):
    cursor = connection.cursor()
    cursor.execute("TRUNCATE TABLE invoices, invoice_details")
    connection.commit()


def callback(ch, method, properties, body):
    print(" [x] Received %r" % body.decode())
    process_invoice(body.decode())
    print(" [x] Saved in DB1")
    ch.basic_ack(delivery_tag=method.delivery_tag)


connection = pika.BlockingConnection(
    pika.ConnectionParameters('localhost'))
channel = connection.channel()

channel.queue_declare(queue='task_queue', durable=True)
print(' [*] Waiting for messages. To exit press CTRL+C')

connection = psycopg2.connect(
    host='localhost',
    user='rabbitmq',
    password='postgres123',
    database='invoices'
)

if __name__ == "__main__":
    channel.basic_qos(prefetch_count=1)
    channel.basic_consume(queue='task_queue', on_message_callback=callback)

    channel.start_consuming()

    connection.close()