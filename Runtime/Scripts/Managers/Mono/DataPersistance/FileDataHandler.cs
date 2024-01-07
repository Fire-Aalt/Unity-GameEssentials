using UnityEngine;
using System;
using System.IO;

namespace RenderDream.GameEssentials
{
    public class FileDataHandler<T> where T : IGameData
    {
        private readonly string dataDirPath = "";
        private readonly string dataFileName = "";
        private readonly bool useEncryption = false;
        private readonly string encryptionCodeWord = "gskbnoskpvewjahjndcdfjwsa";

        public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
        {
            this.dataDirPath = dataDirPath;
            this.dataFileName = dataFileName;
            this.useEncryption = useEncryption;
        }

        public T Load(T template)
        {
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            T loadedData = template;

            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";

                    using (FileStream stream = new(fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }

                    if (useEncryption)
                    {
                        dataToLoad = EncryptDecrypt(dataToLoad);
                    }

                    // Use overwrite for deserialization
                    JsonUtility.FromJsonOverwrite(dataToLoad, loadedData);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error occurred when trying to load data from file: " + fullPath + "\n" + e);
                    return default;
                }
            }

            return loadedData;
        }

        public void Save(T data)
        {
            // use Path.Combine to account for different OS's having different path separators
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            try
            {
                // create the directory the file will be written to if it doesn't already exist
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                // serialize the C# game data object into Json

                string dataToStore = JsonUtility.ToJson(data, true);
                // optionally encrypt the data
                if (useEncryption)
                {
                    dataToStore = EncryptDecrypt(dataToStore);
                }

                // write the serialized data to the file
                using (FileStream stream = new(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
            }
        }

        // the below is a simple implementation of XOR encryption
        private string EncryptDecrypt(string data)
        {
            string modifiedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
            }
            return modifiedData;
        }
    }
}
