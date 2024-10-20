using UnityEngine;

namespace ProjectColombo.Core
{
    public class ActionSchedueler : MonoBehaviour
    {
        IAction currentAction;

        public void StartAction(IAction action)
        {
            if(currentAction == action) return;

            if(currentAction != null)
            {
                currentAction.CancelAction();
            }

            currentAction = action;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }
    }
}


