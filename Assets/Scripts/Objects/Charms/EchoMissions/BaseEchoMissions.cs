using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public abstract class BaseEchoMissions : MonoBehaviour
    {
        public void CompletedMission()
        {
            GetComponent<BaseMask>().UnlockEcho();
            Disable();
        }

        public abstract void Enable();
        public abstract void Disable();
    }
}