using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace RenderDream.UnityManager
{
    public class DataPersistenceManager : MMSingleton<DataPersistenceManager>
    {
        [Header("Debugging")]
        [SerializeField] private bool initializeDataIfNull = false;

        [Header("File Storage Config")]
        [SerializeField] private string fileName;
        [SerializeField] private bool useEncryption;

        private GameData gameData;
        private List<IDataPersistence> dataPersistenceObjects = new();
        private FileDataHandler dataHandler;

        protected override void Awake()
        {
            base.Awake();

            dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        }

        [Button(size: ButtonSizes.Large)]
        public void OpenSaveFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            ProcessStartInfo processStartInfo = new("devenv.exe", $"/edit \"{filePath}\"");
            Process.Start(processStartInfo);
        }

        public void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            dataPersistenceObjects = FindAllDataPersistenceObjects();
            LoadGame(dataPersistenceObjects);
        }

        public void NewGame()
        {
            gameData = new GameData();
        }

        public void LoadGame(List<IDataPersistence> dataPersistenceObjects)
        {
            // load any saved data from a file using the data handler
            gameData = dataHandler.Load();

            // start a new game if the data is null and we're configured to initialize data for debugging purposes
            if (gameData == null && initializeDataIfNull)
            {
                NewGame();
            }

            // if no data can be loaded, don't continue
            if (!HasGameData())
            {
                Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
                return;
            }

            // push the loaded data to all other scripts that need it
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(gameData);
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
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(gameData);
            }

            // save that data to a file using the data handler
            dataHandler.Save(gameData);
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            IEnumerable<IDataPersistence> dataPersistenceObjects = 
                FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IDataPersistence>();

            return new List<IDataPersistence>(dataPersistenceObjects);
        }

        public bool HasGameData()
        {
            return gameData != null;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }
}