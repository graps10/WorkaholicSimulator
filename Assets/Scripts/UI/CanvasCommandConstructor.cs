using System;
using Core.ObjectPool;
using Core.Utilities;
using Entities.Constructors;
using UI.CanvasCommands;
using UI.CanvasReceivers;
using UnityEngine;

namespace UI
{
    public class CanvasCommandConstructor : ObjectConstructor<CanvasCommand, string>
    {
        private static CanvasCommandConstructor instance = new();
        public static CanvasCommandConstructor Instance => instance;

        private readonly CanvasCommandData[] _commandsData = {
            new(typeof(WarningCanvasCommand), WarningCanvasCommand.Path, WarningCanvasReceiver.Instance),
            new(typeof(DebugCanvasCommand), DebugCanvasCommand.Path, DebugCanvasReceiver.Instance),
            new(typeof(CompassArrowCanvasCommand), CompassArrowCanvasCommand.Path, PlayerCanvasReceiver.Instance),
            new(typeof(InteractionCanvasCommand), InteractionCanvasCommand.Path, PlayerCanvasReceiver.Instance),
            new(typeof(EditModeCanvasCommand), EditModeCanvasCommand.Path, PlayerCanvasReceiver.Instance)
        };

        public T Load<T>() where T : CanvasCommand
        {
            LoadImmediately(null, null, out T result);
            return result;
        }

        public override void LoadImmediately<T>(string path, Transform transform, out T result)
        {
            result = null;
            var type = typeof(T);

            foreach (var data in _commandsData)
            {
                if (data.CommandType != type) continue;

                result = (T)GetPrefab(path ?? data.CommandPath);
                result.Initialize(data.CanvasReceiver);
                break;
            }

            if (result == null)
                Debug.LogError("No CanvasCommandData for CanvasCommand of type " + type);
        }

        private PooledGameObject GetPrefab(string path)
        {
            AssetUtils.TryLoadAsset<PrefabPoolInfo>(path, out var prefabPoolInfo);
            return ObjectPooler.TakePooledGameObject(prefabPoolInfo);
        }

        private struct CanvasCommandData
        {
            public Type CommandType;
            public string CommandPath;
            public CanvasReceiver CanvasReceiver;

            public CanvasCommandData(Type commandType, string commandPath, CanvasReceiver canvasReceiver)
            {
                CommandType = commandType;
                CommandPath = commandPath;
                CanvasReceiver = canvasReceiver;
            }
        }
    }
}


