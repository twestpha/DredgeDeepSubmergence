using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using Winch.Core;
using Dummiesman;

namespace Winch.Util
{
    public static class ModelUtil
    {
        // Kind of a hack of Winch's TextureUtil combined with AssetLoader
        // Uses OBJLoader library to runtime load models.
        
        private static Dictionary<string, Mesh> ModelMap = new();
        
        public static void Initialize(){
            WinchCore.Log.Debug("Loading model assets...");
                        
            string[] modDirs = Directory.GetDirectories("Mods");
            foreach (string modDir in modDirs)
            {
                if(modDir.Contains("DeepSubmergence")){
                    string assetFolderPath = Path.Combine(Path.Combine(modDir, "Assets"), "Models");
                    
                    if (!Directory.Exists(assetFolderPath)){
                        continue;
                    }
                    
                    string[] modelFiles = Directory.GetFiles(assetFolderPath);
                    foreach (string file in modelFiles)
                    {
                        try
                        {
                            LoadModelFromFile(file);
                        }
                        catch(Exception ex)
                        {
                            WinchCore.Log.Error($"Failed to load texture file {file}: {ex}");
                        }
                    }
                }
            }
            
        }

        public static Mesh? GetModel(string key)
        {
            if (string.IsNullOrWhiteSpace(key)){
                return null;
            }

            if (ModelMap.TryGetValue(key, out Mesh texture)){
                return texture;
            } else {
                return null;
            }
        }

        internal static void LoadModelFromFile(string path)
        {
            Mesh mesh = new OBJLoader().Load(path);
            
            string fileName = Path.GetFileNameWithoutExtension(path);
            ModelMap[fileName] = mesh;
        }
    }
}