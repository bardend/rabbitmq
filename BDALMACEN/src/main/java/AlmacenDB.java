import com.mongodb.client.*;
import com.mongodb.client.model.Filters;
import com.mongodb.client.model.Updates;
import com.mongodb.client.model.fill.FillOptions;
import org.bson.Document;
import org.bson.conversions.Bson;

public class AlmacenDB {
    private MongoClient mongoClient;
    private MongoDatabase database;

    MongoCollection<Document> collection;
    AlmacenDB(){
        // Replace the uri string with your MongoDB deployment's connection string
        String uri = "mongodb://localhost:27017";
        MongoClient mongoClient = MongoClients.create(uri);
        database = mongoClient.getDatabase("productsdb");
        collection = database.getCollection("products");

    }
    public String find(String name){
        //MongoCollection<Document> collection = database.getCollection("products");
        String ans = "-1";
        /*MongoCursor<Document> cursor = collection.find(Filters.eq("nombre",name)).iterator();
        try {
            while(cursor.hasNext()) {
                System.out.println(cursor.next().toJson());
            }
        } finally {
            cursor.close();
        }*/
        Document doc = collection.find(Filters.eq("nombre",name)).first();
        if(doc != null) ans = doc.get("id").toString();
        return ans;
    }
    public boolean confirm(int id,int quantity){
        Document doc = collection.find(Filters.and(Filters.eq("id",id), Filters.gte("cantidad",quantity))).first();
        if(doc!=null) return true;
        else return false;
    }
    public void update(int id,int quantity){
        Bson query  = Filters.eq("id",id);
        Bson updates  = Updates.inc("cantidad",-quantity);
        collection.updateOne(query, updates);
    }

    public static void main(String[] args) {
        AlmacenDB almacenDB = new AlmacenDB();
        System.out.println(almacenDB.find("platano"));
        System.out.println(almacenDB.confirm(2,5));
        System.out.println(almacenDB.confirm(2,50));
        almacenDB.update(2,5);

    }
                    /*
        String uri = "mongodb://localhost:27017";
        try (MongoClient mongoClient = MongoClients.create(uri)) {
            MongoDatabase database = mongoClient.getDatabase("productsdb");
            MongoCollection<Document> collection = database.getCollection("products");
            try(MongoCursor<Document> cursor = collection.find(Filters.and(Filters.gte("cantidad",22),Filters.eq("nombre","platano"))).iterator()){
                while(cursor.hasNext()) {
                    System.out.println(cursor.next().toJson());
                }
            }
            */

    //List<Document> databases = mongoClient.listDatabases().into(new ArrayList<>());
    //databases.forEach(db -> System.out.println(db.toJson()));
            /*try {
                Bson command = new BsonDocument("ping", new BsonInt64(1));
                Document commandResult = database.runCommand(command);
                System.out.println("Connected successfully to server.");
            } catch (MongoException me) {
                System.err.println("An error occurred while attempting to run a command: " + me);
            }
        }*/

}
