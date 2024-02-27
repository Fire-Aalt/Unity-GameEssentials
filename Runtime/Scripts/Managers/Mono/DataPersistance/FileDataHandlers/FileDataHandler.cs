using UnityEngine;
using System;
using System.IO;

namespace RenderDream.GameEssentials
{
    public abstract class FileDataHandler<T> where T : DataModel
    {
        protected readonly string dataDirPath;
        protected readonly string encryptionKey;
        protected readonly bool useEncryption;

        public FileDataHandler(string dataDirPath, string encryptionKey, bool useEncryption)
        {
            this.dataDirPath = dataDirPath;
            this.encryptionKey = encryptionKey;
            this.useEncryption = useEncryption;
        }

        protected T LoadFromFile(string relativePath)
        {
            string fullPath = Path.Combine(dataDirPath, relativePath);

            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";

                    using (FileStream stream = new(fullPath, FileMode.Open))
                    {
                        using StreamReader reader = new(stream);
                        dataToLoad = reader.ReadToEnd();
                    }

                    if (useEncryption)
                    {
                        dataToLoad = EncryptDecrypt(dataToLoad);
                    }
                    return JsonUtility.FromJson<T>(dataToLoad);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error occurred when trying to load data from file: " + fullPath + "\n" + e);
                    return default;
                }
            }

            return default;
        }

        protected void SaveToFile(T data, string relativePath)
        {
            string fullPath = Path.Combine(dataDirPath, relativePath);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                string dataToStore = JsonUtility.ToJson(data, true);

                if (useEncryption)
                {
                    dataToStore = EncryptDecrypt(dataToStore);
                }

                using (FileStream stream = new(fullPath, FileMode.Create))
                {
                    using StreamWriter writer = new(stream);
                    writer.Write(dataToStore);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
            }
        }

        protected string EncryptDecrypt(string data)
        {
            string modifiedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ encryptionKey[i % encryptionKey.Length]);
            }
            return modifiedData;
        }
    }
}
