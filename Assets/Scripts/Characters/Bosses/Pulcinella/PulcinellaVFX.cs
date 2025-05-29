using ProjectColombo.GameManagement.Events;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PlayVFXOnEvent : MonoBehaviour
    {
        public VisualEffect majorSlash;
        public VisualEffect minorSlash;
        public VisualEffect ragefullImpactSlam;
        public VisualEffect cursedLeapLand;

        public void PlaySlashVFX()
        {

        }

        public void PlayRI_ImpactVFX()
        {
            ragefullImpactSlam.Play();
        }

        public void PlayCL_LandVFX()
        {
            cursedLeapLand.Play();
        }
    }
}