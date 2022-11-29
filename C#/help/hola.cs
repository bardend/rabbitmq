using System;
using System.Collections.Generic;
class Geeks {

    // Main Method
    static void Main(string[] args){
    List<string> fr = new List<string>();

    Console.WriteLine("Name : ");
    String read_user = Console.ReadLine();
    fr.Add(read_user);
    
    Console.WriteLine("id : ");
    string read_id = Console.ReadLine();
    fr.Add(read_id);
        
        
    Console.WriteLine("cantidad : ");
    string read_cnt = Console.ReadLine();
    fr.Add(read_cnt);    
                
      
    int go = fr.Count;
    string ret = "";

    for(int i=0;i<go;i++){
        ret+=(fr[i]);
        if(i!=go-1)
            ret+=" ";
    }
    
    Console.WriteLine(ret);    

    }
}