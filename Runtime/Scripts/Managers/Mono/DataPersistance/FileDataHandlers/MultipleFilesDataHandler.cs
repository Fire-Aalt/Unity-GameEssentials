using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KrasCore.Essentials
{
    public class MultipleFilesDataHandler<T> : FileDataHandler<T> where T : DataModel
    {
        public const string ID_LOOKUP = "{id}";

        protected readonly string profileDirectoryNamePattern;
        protected readonly string profileFileNamePattern;

        public MultipleFilesDataHandler(string dataDirPath, string encryptionKey, bool useEncryption, string profileDirectoryNamePattern, string profileFileNamePattern) :
            base(dataDirPath, encryptionKey, useEncryption)
        {
            this.profileDirectoryNamePattern = profileDirectoryNamePattern;
            this.profileFileNamePattern = profileFileNamePattern;
        }

        public T Load(int profileId)
        {
            return LoadFromFile(GetRelativePath(profileId));
        }

        public void Save(T data, int profileId)
        {
            SaveToFile(data, GetRelativePath(profileId));
        }

        public void DeleteSave(int profileId)
        {
            DeleteFile(GetRelativePath(profileId));
        }

        public Dictionary<int, T> LoadAllProfiles()
        {
            var profileDictionary = new Dictionary<int, T>();

            IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
            foreach (var dirInfo in dirInfos)
            {
                int profileId = GetProfileId(dirInfo.Name);

                string fullPath = Path.Combine(dataDirPath, GetRelativePath(profileId));
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                T profileData = Load(profileId);
                if (profileData != null)
                {
                    profileDictionary.Add(profileId, profileData);
                }
                else
                {
                    Debug.LogWarning($"Profile data exists but something went wrong. Result: null. ProfileId: {profileId}");
                }
            }

            return profileDictionary;
        }

        public int GetMostRecentProfileId()
        {
            int mostRecentProfileId = -1;
            DateTime mostRecentDateTime = DateTime.MinValue;

            var profilesGameData = LoadAllProfiles();
            foreach (var pair in profilesGameData)
            {
                int profileId = pair.Key;
                T profileData = pair.Value;

                var newDateTime = DateTime.FromBinary(profileData.lastUpdated);
                if (mostRecentProfileId == -1)
                {
                    mostRecentProfileId = profileId;
                    mostRecentDateTime = newDateTime;
                }
                else
                {
                    if (newDateTime > mostRecentDateTime)
                    {
                        mostRecentProfileId = profileId;
                        mostRecentDateTime = newDateTime;
                    }
                }
            }
            return mostRecentProfileId;
        }

        public string GetRelativePath(int profileId)
        {
            string directoryName = profileDirectoryNamePattern.Replace(ID_LOOKUP, profileId.ToString());
            string fileName = profileFileNamePattern.Replace(ID_LOOKUP, profileId.ToString());
            return Path.Combine(directoryName, fileName);
        }

        public int GetProfileId(string directoryName)
        {
            int profileId = -1, idLength = 0;
            for (int i = directoryName.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(directoryName[i]))
                {
                    int digit = directoryName[i] - '0';
                    if (profileId == -1)
                    {
                        profileId = digit;
                    }
                    else
                    {
                        profileId += digit * 10 * idLength;
                    }
                    idLength++;
                }
                else
                {
                    break;
                }
            }
            return profileId;
        }
    }
}
