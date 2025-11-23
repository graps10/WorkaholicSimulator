using System.Collections.Generic;
using Core.Interfaces;
using UI.CanvasCommands;
using UnityEngine;

namespace UI.CanvasReceivers
{
    public abstract class CanvasReceiver : System.IDisposable
    {
        public abstract GameObject Canvas { get; }

        private readonly List<CanvasCommand> _commands = new();

        public void RegisterCanvasCommand(CanvasCommand command)
        {
            if (!command.IsDisposed && !_commands.Contains(command))
                _commands.Add(command);
        }
        
        public void UnregisterCanvasCommand(CanvasCommand command)
        {
            if (!command.IsDisposed && _commands.Contains(command))
                _commands.Remove(command);
        }

        public void ForceUpdateCommands()
        {
            foreach (var command in _commands)
                if (command is IUpdatable updatableCommand)
                    updatableCommand.OnUpdate();
        }

        public virtual void Dispose() { }
    }
}