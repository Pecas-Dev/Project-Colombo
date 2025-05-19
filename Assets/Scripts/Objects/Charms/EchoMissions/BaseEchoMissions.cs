using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public abstract class BaseEchoMissions : MonoBehaviour
    {
        public void CompletedMission()
        {
            BaseMask myMask = GetComponent<BaseMask>();

            if (!myMask.echoUnlocked)
            {
                GetComponent<BaseMask>().UnlockEcho();
            }
            Disable();
        }

        public abstract void Enable();
        public abstract void Disable();

        public abstract void ResetProgress();
    }
}