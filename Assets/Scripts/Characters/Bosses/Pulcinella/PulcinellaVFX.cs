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
        public VisualEffect minorEffect;
        public VisualEffect majorEffect;


        public void PlayRI_ImpactVFX()
        {
            ragefullImpactSlam.Play();
        }

        public void PlayCL_LandVFX()
        {
            cursedLeapLand.Play();
        }

        public void PlayMajorMinorEffect()
        {
            GameGlobals.MusicScale scale = GetComponent<EntityAttributes>().currentScale;

            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                minorEffect.Stop();
                majorEffect.Play();
                StartCoroutine(AnimateSmokeEffekt(majorEffect));
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                majorEffect.Stop();
                minorEffect.Play();
                StartCoroutine(AnimateSmokeEffekt(minorEffect));
            }
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        IEnumerator AnimateSmokeEffekt(VisualEffect effect)
        {
            int i = 0;

            while (i < 15)
            {
                effect.SetFloat("SetSize", i);
                i += 1;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(1f);
            effect.Stop();
        }
    }
}