using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;
using System.IO;

namespace DatasetGenerator.Server
{
    public class ServerMain : BaseScript
    {   
        
        public ServerMain()
        {
            Debug.WriteLine("Hi from DatasetGenerator.Server!");
            
            EventHandlers["saveData"] += new Action<string, int, string>(SaveData);
            
        }

        private void SaveData(string saveDir, int id, string metadata)
        {   

            string metadataFile = $"{saveDir}\\labels\\{id}.txt";
        
            File.WriteAllText(metadataFile, metadata);

            Debug.WriteLine($"{metadataFile} Saved");
        }

    }
}
