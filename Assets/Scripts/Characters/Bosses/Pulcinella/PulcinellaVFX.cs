using ProjectColombo.GameManagement.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.VFX
{
    public class PulcinellaVFX : MonoBehaviour
    {
        public VisualEffect ragefullImpactSlam;
        public VisualEffect cursedLeapLand;
        public VisualEffect minorEffectSmoke;
        public VisualEffect majorEffectSmoke;
        public VisualEffect cursedNoteMajorAnti;
        public VisualEffect cursedNoteMinorAnti;


        public void PlayRI_ImpactVFX()
        {
            ragefullImpactSlam.Play();
        }

        public void PlayCL_LandVFX()
        {
            cursedLeapLand.Play();
        }

        public void PlayerCN_AntiVFX()
        {
            GameGlobals.MusicScale scale = GetComponent<EntityAttributes>().currentScale;

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                cursedNoteMajorAnti.Play();
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                cursedNoteMinorAnti.Play();
            }
        }

        public void PlaySmokeEffect()
        {
            GameGlobals.MusicScale scale = GetComponent<EntityAttributes>().currentScale;

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                minorEffectSmoke.Stop();
                majorEffectSmoke.Play();
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                majorEffectSmoke.Stop();
                minorEffectSmoke.Play();
            }
        }
    }
}