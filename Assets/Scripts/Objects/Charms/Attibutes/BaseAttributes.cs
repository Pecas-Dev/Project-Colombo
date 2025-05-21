using System.Collections;
using UnityEngine;

namespace ProjectColombo.Objects.Charms
{
    public abstract class BaseAttributes : MonoBehaviour
    {
        protected bool eventHandled = false;

        protected IEnumerator ResetEventHandled()
        {
            yield return new WaitForEndOfFrame();
            eventHandled = false;
        }

        public abstract void Enable();
        public abstract void Disable();
    }
}