#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Game
{
    public static class AssetDatabaseUtils
    {
        public static string ToGuid(string path)
        {
            return AssetDatabase.GUIDFromAssetPath(path).ToString();
        }
        
        public static string ToGuid(Object asset)
        {
            return ToGuid(AssetDatabase.GetAssetPath(asset));
        }
        
        public static Unity.Entities.Hash128 ToGuidHash(Object asset)
        {
            return AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(asset));
        }
        
        public static void ShowMessageInSceneView(string message) 
        {
            foreach (SceneView scene in SceneView.sceneViews)
            {
                scene.ShowNotification(new GUIContent(message));
            }
        }

        public static void EnsureValidFolder(string assetPath)
        {
            var folderPath = GetFolderPath(assetPath);
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        private static string GetFolderPath(string path)
        {
            if (Directory.Exists(path))
            {
                return path;
            }
            var folderPath = Directory.GetParent(path).FullName;
            return "Assets" + folderPath[Application.dataPath.Length..];
        }

        public static void SaveAssetToDatabase(Object asset, string assetPath)
        {
            EnsureValidFolder(assetPath);
            
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static Object[] LoadAllAssetsAtPath(string path)
        {
            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }
            var guids = AssetDatabase.FindAssets("", new[] {path});
            var objectList = new Object[guids.Length];
            
            for (int index = 0; index < guids.Length; index++)
            {
                var guid = guids[index];
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                objectList[index] = asset;
            }

            return objectList;
        }
        
        public static T[] LoadAllAssetsOfType<T>(string optionalPath = "") where T : Object
        {
            string[] guids;
            if(optionalPath != "")
            {
                if(optionalPath.EndsWith("/"))
                {
                    optionalPath = optionalPath.TrimEnd('/');
                }
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}",new[] { optionalPath });
            }
            else
            {
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            }
            var objectList = new T[guids.Length];

            for (int index = 0; index < guids.Length; index++)
            {
                var guid = guids[index];
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
                objectList[index] = asset;
            }

            return objectList;
        }
        
        public static string[] GetAllAssetPathsAtPath(string path)
        {
            if(path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }
            
            var guids = AssetDatabase.FindAssets("", new string[] {path});
            var pathList = new string[guids.Length];
            
            for (int index = 0; index < guids.Length; index++)
            {
                pathList[index] = AssetDatabase.GUIDToAssetPath(guids[index]);
            }

            return pathList;
        }
    }
}
#endif