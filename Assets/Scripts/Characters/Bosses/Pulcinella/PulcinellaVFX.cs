using ProjectColombo.GameManagement.Events;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PulcinellaVFX : MonoBehaviour
    {
        public VisualEffect ragefullImpactSlam;
        public VisualEffect cursedLeapLand;


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