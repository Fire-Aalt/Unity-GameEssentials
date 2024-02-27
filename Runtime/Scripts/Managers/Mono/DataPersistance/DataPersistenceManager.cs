using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace RenderDream.GameEssentials
{
    public abstract class DataPersistenceManager<T1, T2> : Singleton<DataPersistenceManager<T1, T2>> where T1 : SettingsDataModel where T2 : GameDataModel
    {
        [Title("Debug")]
        [SerializeField] private bool _disableDataPersistence = false;

        [Title("Settings File Storage")]
        [SerializeField] private string _settingsFileName = "settings.data";

        [Title("Profile File/Files Storage")]
        [SerializeField] private bool _multipleProfiles = true;

        [InfoBox("{id} refers to profileId which will be used for lookup")]
        [SerializeField, ShowIf("_multipleProfiles"), ValidateInput("IsPatternValid")] 
        private string _profileDirectoryNamePattern = "profile{id}";
        [SerializeField, ShowIf("_multipleProfiles"), ValidateInput("IsPatternValid")]
        private string _profileFileNamePattern = "game{id}.data";

        [SerializeField, HideIf("_multipleProfiles")]
        private string _profileDirectoryName = "profile";
        [SerializeField, HideIf("_multipleProfiles")]
        private string _profileFileName = "game.data";

        [Title("Encryption")]
        [SerializeField] private bool _useEncryption = false;
        [SerializeField, ReadOnly] private string _encryptionKey = Guid.NewGuid().ToString();

        private T1 _settingsData;
        private T2 _gameData;
        private List<IDataPersistence<T1>> _settingsDataPersistenceObjects = new();
        private List<IDataPersistence<T2>> _gameDataPersistenceObjects = new();
        private SingleFileDataHandler<T1> _settingsDataHandler;
        private MultipleFilesDataHandler<T2> _gameDataHandler;

        private EventBinding<LoadGameEvent> _loadGameBinding;
        private EventBinding<SaveGameEvent> _saveGameBinding;

        private IEnumerable<MonoBehaviour> _loadedMonoBehaviors;
        private int _selectedProfileId;

        protected bool IsPatternValid(string pattern)
        {
            return pattern.Contains(MultipleFilesDataHandler<T2>.ID_LOOKUP);
        }

        [Title("Open Actions")]
        [Button(size: ButtonSizes.Large)]
        private void OpenSavesFolder()
        {
            string path = Path.GetFullPath(Application.persistentDataPath);
            ProcessStartInfo startInfo = new()
            {
                Arguments = path,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }


        [Button(size: ButtonSizes.Large)]
        private void OpenRecentSaveFile()
        {
            if (_gameDataHandler == null)
            {
                Init();
            }

            string filePath = Path.Combine(Application.persistentDataPath, _gameDataHandler.GetRelativePath(_selectedProfileId));
            if (File.Exists(filePath))
            {
                ProcessStartInfo processStartInfo = new("devenv.exe", $"/edit \"{filePath}\"");
                Process.Start(processStartInfo);
            }
        }

        protected virtual void Init()
        {
            string profileDirectoryPattern, profileFilePattern;
            if (_multipleProfiles)
            {
                _selectedProfileId = -1;
                profileDirectoryPattern = _profileDirectoryNamePattern;
                profileFilePattern = _profileFileNamePattern;
            }
            else
            {
                _selectedProfileId = 0;
                profileDirectoryPattern = _profileDirectoryName;
                profileFilePattern = _profileFileName;
            }

            _settingsDataHandler = new SingleFileDataHandler<T1>(Application.persistentDataPath,
                _encryptionKey, _useEncryption, _settingsFileName);
            _gameDataHandler = new MultipleFilesDataHandler<T2>(Application.persistentDataPath,
                _encryptionKey, _useEncryption, profileDirectoryPattern, profileFilePattern);

            if (_multipleProfiles)
            {
                _selectedProfileId = _gameDataHandler.GetMostRecentProfileId();
            }
        }

        public abstract T1 NewSettingsData();
        public abstract T2 NewGameData();

        public void LoadGame()
        {
            if (_disableDataPersistence) return;

            _settingsData = _settingsDataHandler.Load();
            if (_settingsData == null)
            {
                _settingsData = NewSettingsData();
            }
            _gameData = _gameDataHandler.Load(_selectedProfileId);
            if (_gameData == null && _selectedProfileId != -1)
            {
                _gameData = NewGameData();
            }

            var monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var monoBehaviorsToLoad = monoBehaviours.Except(_loadedMonoBehaviors);
            
            _settingsDataPersistenceObjects = FindDataPersistenceObjects<T1>(monoBehaviorsToLoad);
            if (_settingsData != null)
            {
                foreach (IDataPersistence<T1> dataPersistenceObj in _settingsDataPersistenceObjects)
                {
                    dataPersistenceObj.LoadData(_settingsData);
                }
            }
            _gameDataPersistenceObjects = FindDataPersistenceObjects<T2>(monoBehaviorsToLoad);
            if (_gameData != null)
            {
                foreach (IDataPersistence<T2> dataPersistenceObj in _gameDataPersistenceObjects)
                {
                    dataPersistenceObj.LoadData(_gameData);
                }
            }
            _loadedMonoBehaviors = monoBehaviours;
        }

        public void SaveGame()
        {
            if (_disableDataPersistence) return;

            if (_settingsData != null)
            {
                foreach (IDataPersistence<T1> dataPersistenceObj in _settingsDataPersistenceObjects)
                {
                    dataPersistenceObj.SaveData(_settingsData);
                }
                _settingsData.lastUpdated = DateTime.Now.ToBinary();
                _settingsDataHandler.Save(_settingsData);
            }
            if (_gameData != null)
            {
                foreach (IDataPersistence<T2> dataPersistenceObj in _gameDataPersistenceObjects)
                {
                    dataPersistenceObj.SaveData(_gameData);
                }
                _gameData.lastUpdated = DateTime.Now.ToBinary();
                _gameDataHandler.Save(_gameData, _selectedProfileId);
            }
        }

        public void DeleteProfileData(int profileId)
        {
            _gameDataHandler.DeleteSave(profileId);
        }

        public void ChangeSelectedProfileId(int newProfileId)
        {
            SaveGame();
            _selectedProfileId = newProfileId;
            LoadGame();
        }

        public Dictionary<int, T2> GetAllProfilesGameData() => _gameDataHandler.LoadAllProfiles();

        protected override void Awake()
        {
            base.Awake();

            if (_disableDataPersistence)
            {
                UnityEngine.Debug.LogWarning("DataPersistence is currently disabled!");
            }

            Init();
        }

        protected void OnEnable()
        {
            _saveGameBinding = new EventBinding<SaveGameEvent>(SaveGame);

            EventBus<SaveGameEvent>.Register(_saveGameBinding);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        protected void OnDisable()
        {
            EventBus<SaveGameEvent>.Deregister(_saveGameBinding);
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        protected void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoadGame();
        }

        protected void OnApplicationQuit()
        {
            SaveGame();
        }


        protected List<IDataPersistence<T>> FindDataPersistenceObjects<T>(IEnumerable<MonoBehaviour> monoBehaviours) where T : DataModel
        {
            IEnumerable<IDataPersistence<T>> dataPersistenceObjects =
                monoBehaviours.OfType<IDataPersistence<T>>();

            return new List<IDataPersistence<T>>(dataPersistenceObjects);
        }
    }
}