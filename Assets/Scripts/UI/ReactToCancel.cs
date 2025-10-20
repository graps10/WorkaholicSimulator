using System;
using System.Collections.Generic;
using Core.InputManager;

namespace UI
{
    public static class ReactToCancel
    {
        private static readonly Stack<Action> OnClickCancelActions = new();
        public static bool IsAnyActive => OnClickCancelActions.Count > 0;
        
        public static void SubscribeToCancel(Action cancelAction)
        {
            if (cancelAction == null 
                || OnClickCancelActions.Contains(cancelAction)) 
                return;

            OnClickCancelActions.Push(cancelAction);
            
            // if (OnClickCancelActions.Count == 1)
            //     InputManager.OnPauseCancel += InvokeCancelActions;
        }
        
        public static void UnsubscribeFromCancel(Action cancelAction)
        {
            if (cancelAction == null) 
                return;
            
            var stack = new Stack<Action>();
            while (OnClickCancelActions.Count > 0)
            {
                var top = OnClickCancelActions.Pop();
                if (top == cancelAction) break;
                stack.Push(top);
            }
            
            while (stack.Count > 0)
                OnClickCancelActions.Push(stack.Pop());
            
            // if (OnClickCancelActions.Count == 0)
            //     InputManager.OnPauseCancel -= InvokeCancelActions;
        }
        
        private static void InvokeCancelActions()
        {
            if (OnClickCancelActions.Count == 0) 
                return;
            
            var action = OnClickCancelActions.Pop();
            action?.Invoke();
            
            // if (OnClickCancelActions.Count == 0)
            //     InputManager.OnPauseCancel -= InvokeCancelActions;
        }
    }
}