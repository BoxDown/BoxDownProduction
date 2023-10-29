using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Utility;
using Managers;

namespace Gun
{
    public static class GunModuleSpawner
    {

        public static void SpawnGunModule(string gunModuleName, Vector3 worldPos)
        {
            GunModule moduleToLoad = Resources.Load<GunModule>(GetGunModuleResourcesPath(gunModuleName));
            if (moduleToLoad == null)
            {
                throw new NullReferenceException("Module attempted to load is null");
            }
            moduleToLoad.Spawn(worldPos);
            GameManager.SpawnModuleVFX(moduleToLoad.e_moduleType, worldPos);
            AudioManager.PlayFmodEvent("SFX/Environment/Module_Crate_Spawn", worldPos);
        }

        public static void DeclareAllGunModules()
        {
            List<string> allGunModulesInProject = new List<string>();
            string gunModuleFolderDirectory = Directory.GetCurrentDirectory();
            gunModuleFolderDirectory += "\\Assets\\Resources\\GunModules";

            string[] directories = Directory.GetDirectories(gunModuleFolderDirectory);

            for (int i = 0; i < directories.Length; i++)
            {
                string[] filesInDirectory = Directory.GetFiles(directories[i]);
                for (int j = 0; j < filesInDirectory.Length; j++)
                {
                    if (filesInDirectory[j].Contains(".meta"))
                    {
                        continue;
                    }

                    //split name from GunModules Folder
                    string[] stringSplit = filesInDirectory[j].Split("GunModules\\");
                    //remove ".asset"
                    string moduleName = stringSplit[1].Remove(stringSplit[1].Length - 6);
                    if (moduleName.Contains("E_"))
                    {
                        continue;
                    }
                    //add name to list
                    allGunModulesInProject.Add(moduleName);
                }
            }

            TextDocumentReadWrite.FileWrite(Directory.GetCurrentDirectory() + "\\Assets" + "\\allGunModules.txt", allGunModulesInProject.ToArray());
        }
        

        public static string GetGunModuleResourcesPath(string gunModuleName)
        {
            // asset is "..\\..\\{gunModuleName}.asset"
            // resources.load requires a direct file path from resources folder
            // resources.load is templated and wants to be overloaded with the type of the asset, it doesn't read the file extension in the string

            string currentDirectory = Directory.GetCurrentDirectory();
            currentDirectory += "\\Assets\\Resources\\GunModules\\";

            string filePath = currentDirectory + gunModuleName;

            string[] pathSections = filePath.Split("Resources\\");

            filePath = pathSections[1];
            if (filePath.Contains(".asset.meta"))
            {
                filePath = filePath.Remove(filePath.Length - 11);
            }
            else if (filePath.Contains(".asset"))
            {
                filePath = filePath.Remove(filePath.Length - 6);
            }
            return filePath;
        }

        public static GunModule GetGunModule(string gunModuleName)
        {
            return Resources.Load<GunModule>(GetGunModuleResourcesPath(gunModuleName));
        }
    }
}
