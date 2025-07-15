using System.Collections.Generic;
using UnityEngine;

namespace Core.Commands
{
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager Instance { get; private set; }
    
        private Stack<ICommand> commandHistory = new();
    
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            commandHistory.Push(command);
        }
    
        public void UndoLastCommand()
        {
            if (commandHistory.Count > 0)
            {
                ICommand lastCommand = commandHistory.Pop();
                lastCommand.Undo();
            }
        }
    }
}