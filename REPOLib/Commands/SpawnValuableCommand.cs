﻿using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using REPOLib.Modules;
using REPOLib.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace REPOLib.Commands
{
    public static class SpawnValuableCommand
    {
        static bool initialized = false;

        private static readonly Dictionary<string, GameObject> valuablePrefabs = [];

        [REPOLibCommandInitializer]
        public static void Initialize()
        {
            Logger.LogInfo("Initializing spawn valuable command");

            CacheValuables();

            initialized = true;
        }

        public static void CacheValuables()
        {
            valuablePrefabs.Clear();
            if (RunManager.instance == null)
            {
                Logger.LogError($"Failed to cache LevelValuables. RunManager instance is null.");
                return;
            }
            foreach (var level in RunManager.instance.levels)
            {
                foreach (var valuablePreset in level.ValuablePresets)
                {
                    if (valuablePreset.TryGetCombinedList(out var valuables))
                    {
                        foreach (var valuable in valuables)
                        {
                            Logger.LogInfo($"SpawnValuable caching \"{valuable.name}\".");
                            valuablePrefabs.TryAdd(valuable.name.ToLower().Substring(valuable.name.LastIndexOf('/')+1), valuable);
                        }
                    }
                }
            }
        }

        [REPOLibCommandExecution(
            "Spawn Valuable",
            "Spawn an instance of a valuable with the specified (case-insensitive) name. You can optionally leave out \"Valuable \" from the prefab name.",
            requiresDeveloperMode:true
            )]
        [REPOLibCommandAlias("spawnvaluable")]
        [REPOLibCommandAlias("spawnval")]
        [REPOLibCommandAlias("sv")]
        public static void Execute(string args)
        {
            Logger.LogInfo($"Running spawn command with args \"{args}\"", extended: true);
            if (args == null || args.Length == 0)
            {
                Logger.LogWarning("No args provided to spawn command.");
                return;
            }
            if (!initialized)
            {
                Logger.LogError("Spawn command not initialized!");
                return;
            }
            if (PlayerAvatar.instance == null)
            {
                Logger.LogWarning("Can't spawn anything, player avatar is not initialized.");
                return;
            }
            if (ValuableDirector.instance == null)
            {
                Logger.LogError("ValuableDirector not initialized.");
                return;
            }
            //TODO: Add checks to restrict command to gameplay and not lobby

            SpawnValuableByName(args);
        }

        private static void SpawnValuableByName(string name)
        {
            Vector3 spawnPos = PlayerAvatar.instance.transform.position + PlayerAvatar.instance.transform.forward * 1f;
            GameObject valuablePrefab;
            Logger.LogInfo($"Trying to spawn \"{name}\" at {spawnPos}...", extended:true);
            if (!valuablePrefabs.TryGetValue(name.ToLower(), out valuablePrefab))
            {
                if (!valuablePrefabs.TryGetValue("valuable " + name.ToLower(), out valuablePrefab))
                {
                    Logger.LogWarning($"Spawn command failed. Unknown valuable with name \"{name}\".");
                    return;
                }
            }
            try
            {
                if (GameManager.instance?.gameMode == 0)
                {
                    Logger.LogInfo($"Locally spawning {valuablePrefab.name} at {spawnPos}.");
                    UnityEngine.Object.Instantiate(valuablePrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    string valuablePath = ResourcesHelper.GetValuablePrefabPath(valuablePrefab.GetComponent<ValuableObject>());
                    if (valuablePath == string.Empty)
                    {
                        Logger.LogError($"Failed to get the path of valuable \"{valuablePrefab.name ?? "null"}\"");
                        return;
                    }
                    Logger.LogInfo($"Network spawning {valuablePath} at {spawnPos}.");
                    PhotonNetwork.InstantiateRoomObject(valuablePath, spawnPos, Quaternion.identity);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to spawn \"{name}\":\n{e}");
            }
        }
    }
}
