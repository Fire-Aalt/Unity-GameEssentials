using UnityEngine;
using System;
using System.IO;

namespace KrasCore.Essentials
{
    public abstract class FileDataHandler<T> where T : DataModel
    {
        protected readonly string backupExtension = ".bak";

        protected readonly string dataDirPath;
        protected readonly string encryptionKey;
        protected readonly bool useEncryption;

        public FileDataHandler(string dataDirPath, string encryptionKey, bool useEncryption)
        {
            this.dataDirPath = dataDirPath;
            this.encryptionKey = encryptionKey;
            this.useEncryption = useEncryption;
        }

        protected T LoadFromFile(string relativePath, bool allowRestoreFromBackup = true)
        {
            string fullPath = Path.Combine(dataDirPath, relativePath);

            T loadedData = null;
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
                    loadedData = JsonUtility.FromJson<T>(dataToLoad);
                }
                catch (Exception e)
                {
                    if (allowRestoreFromBackup)
                    {
                        Debug.LogWarning("Failed to load data file. Attempting to rollback.\n" + e);
                        bool rollbackSuccess = AttemptRollback(fullPath);
                        if (rollbackSuccess)
                        {
                            loadedData = LoadFromFile(fullPath, allowRestoreFromBackup: false);
                        }
                        else
                        {
                            Debug.LogError("Backup failed");
                            Debug.LogError("Data was null. Deleting the save");
                            DeleteFile(relativePath);
                        }
                    }
                    else
                    {
                        Debug.LogError("Error occured when trying to load data at path: " + fullPath + " and backup did not work.\n" + e);
                        Debug.LogError("Data was null. Deleting the save");
                        DeleteFile(relativePath);
                    }
                }
            }

            return loadedData;
        }

        protected void SaveToFile(T data, string relativePath)
        {
            string fullPath = Path.Combine(dataDirPath, relativePath);
            string backupFilePath = fullPath + backupExtension;
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

                T verifiedGameData = LoadFromFile(relativePath);
                if (verifiedGameData != null)
                {
                    File.Copy(fullPath, backupFilePath, true);
                }
                else
                {
                    throw new Exception("Save file could not be verified and backup could not be created.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
            }
        }

        protected void DeleteFile(string relativePath)
        {
            string fullPath = Path.Combine(dataDirPath, relativePath);
            string parentDirectoryPath = Path.GetDirectoryName(fullPath);
            try
            {
                if (File.Exists(fullPath))
                {
                    if (parentDirectoryPath != Path.GetFullPath(dataDirPath))
                    {
                        Directory.Delete(parentDirectoryPath, true);
                    }
                    else
                    {
                        File.Delete(fullPath);
                    }
                }
                else
                {
                    Debug.LogWarning("Tried to delete data, but it was not found at path: " + fullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to delete data at path: " + fullPath + "\n" + e);
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

        protected bool AttemptRollback(string fullPath)
        {
            bool success = false;
            string backupFilePath = fullPath + backupExtension;
            try
            {
                if (File.Exists(backupFilePath))
                {
                    File.Copy(backupFilePath, fullPath, true);
                    success = true;
                    Debug.LogWarning("Had to rollback to backup file at: " + backupFilePath);
                }
                else
                {
                    throw new Exception("Tried to rollback, but no backup file exists to rollback to.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to roll back to backup file at: " + backupFilePath + "\n" + e);
            }
            return success;
        }
    }
}
