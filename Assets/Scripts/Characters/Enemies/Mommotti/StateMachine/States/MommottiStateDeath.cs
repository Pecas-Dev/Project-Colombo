using ProjectColombo.GameManagement.Events;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateDeath : MommottiBaseState
    {
        float timer;
        float fadeTime = 1f;
        GameGlobals.MusicScale causedScale;

        Material fadeMaterial;
        Color baseColor;
        float startAlpha;

        public MommottiStateDeath(MommottiStateMachine stateMachine, GameGlobals.MusicScale scale) : base(stateMachine)
        {
            causedScale = scale;
        }

        public override void Enter()
        {
            stateMachine.myAnimator.SetTrigger("Death");
            //stateMachine.GetComponent<Collider>().enabled = false;
            stateMachine.tag = "Default";
            CustomEvents.EnemyDied(causedScale, stateMachine.gameObject);

            fadeMaterial = stateMachine.myFadeOutSkin.material;

            // Setup URP Lit material for transparency
            fadeMaterial.SetFloat("_Surface", 1); // Transparent
            fadeMaterial.SetFloat("_Blend", 0);   // Alpha blending
            fadeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fadeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fadeMaterial.SetInt("_ZWrite", 0);
            fadeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            fadeMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            baseColor = fadeMaterial.GetColor("_BaseColor");
            startAlpha = baseColor.a;

            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.DEAD);
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            // Fade out over fadeTime
            if (timer <= fadeTime)
            {
                float alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeTime);
                baseColor.a = alpha;

                if (fadeMaterial == null)
                {
                    Debug.Log("fade material was missing");

                    fadeMaterial = stateMachine.myFadeOutSkin.material;

                    // Setup URP Lit material for transparency
                    fadeMaterial.SetFloat("_Surface", 1); // Transparent
                    fadeMaterial.SetFloat("_Blend", 0);   // Alpha blending
                    fadeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    fadeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    fadeMaterial.SetInt("_ZWrite", 0);
                    fadeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    fadeMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    baseColor = fadeMaterial.GetColor("_BaseColor");
                    startAlpha = baseColor.a;
                }

                fadeMaterial.SetColor("_BaseColor", baseColor);
            }
            else
            {
                // Fully faded, destroy the entity
                stateMachine.myEntityAttributes.Destroy();
            }
        }

        public override void Exit()
        {
        }
    }
}
