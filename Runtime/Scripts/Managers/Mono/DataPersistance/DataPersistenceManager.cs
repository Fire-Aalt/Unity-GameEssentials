using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace RenderDream.GameEssentials
{
    public abstract class DataPersistenceManager<T> : Singleton<DataPersistenceManager<T>> where T : IGameData
    {
        [Header("Debugging")]
        [SerializeField] private bool initializeDataIfNull = true;

        [Header("File Storage Config")]
        [SerializeField] private string fileName = "GameData.data";
        [SerializeField] private bool useEncryption = false;

        private T _gameData;
        private T _gameDataTemplate;
        private List<IDataPersistence<T>> _dataPersistenceObjects = new();
        private FileDataHandler<T> _dataHandler;
        private EventBinding<SaveGameEvent> _saveGameBinding;

        [Button(size: ButtonSizes.Large)]
        public void OpenSaveFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            ProcessStartInfo processStartInfo = new("devenv.exe", $"/edit \"{filePath}\"");
            Process.Start(processStartInfo);
        }

        protected override void Awake()
        {
            base.Awake();

            _gameDataTemplate = NewGameData();
            _dataHandler = new FileDataHandler<T>(Application.persistentDataPath, fileName, useEncryption);

            _dataPersistenceObjects = FindAllDataPersistenceObjects();
            LoadGame(_dataPersistenceObjects);
        }

        public abstract T NewGameData();

        public void NewGame()
        {
            _gameData = NewGameData();
            _gameData.ResetToDefault();
        }

        public void LoadGame(List<IDataPersistence<T>> dataPersistenceObjects)
        {
            // load any saved data from a file using the data handler
            _gameData = _dataHandler.Load(_gameDataTemplate);

            // start a new game if the data is null and we're configured to initialize data for debugging purposes
            if (_gameData == null && initializeDataIfNull)
            {
                NewGame();
                Debug.Log("NewGame");
            }

            // if no data can be loaded, don't continue
            if (!HasGameData())
            {
                Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
                return;
            }

            // push the loaded data to all other scripts that need it
            foreach (IDataPersistence<T> dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(_gameData);
            }
        }

        public void SaveGame()
        {
            // if we don't have any data to save, log a warning here
            if (!HasGameData())
            {
                Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
                return;
            }

            // pass the data to other scripts so they can update it
            foreach (IDataPersistence<T> dataPersistenceObj in _dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(_gameData);
            }

            // save that data to a file using the data handler
            _dataHandler.Save(_gameData);
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private List<IDataPersistence<T>> FindAllDataPersistenceObjects()
        {
            IEnumerable<IDataPersistence<T>> dataPersistenceObjects = 
                FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IDataPersistence<T>>();

            return new List<IDataPersistence<T>>(dataPersistenceObjects);
        }

        public bool HasGameData() => _gameData != null;

        public void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _dataPersistenceObjects = FindAllDataPersistenceObjects();
            LoadGame(_dataPersistenceObjects);
        }

        private void OnEnable()
        {
            _saveGameBinding = new EventBinding<SaveGameEvent>(SaveGame);

            SceneManager.sceneLoaded += HandleSceneLoaded;
            EventBus<SaveGameEvent>.Register(_saveGameBinding);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            EventBus<SaveGameEvent>.Deregister(_saveGameBinding);
        }
    }
}