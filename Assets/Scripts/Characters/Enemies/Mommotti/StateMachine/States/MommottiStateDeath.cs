using ProjectColombo.GameManagement.Events;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace ProjectColombo.StateMachine.Mommotti
{
    public class MommottiStateDeath : MommottiBaseState
    {
        float timer;
        float fadeTime = 1f;
        GameGlobals.MusicScale causedScale;

        public MommottiStateDeath(MommottiStateMachine stateMachine, GameGlobals.MusicScale scale) : base(stateMachine)
        {
            causedScale = scale;
        }

        public override void Enter()
        {
            stateMachine.myAnimator.SetTrigger("Death");
            CustomEvents.EnemyDied(causedScale, stateMachine.gameObject);

            //// Create a new material instance using the URP shader
            //Material materialInstance = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            //if (materialInstance == null)
            //{
            //    Debug.LogError("URP Shader not found! Make sure URP is properly installed.");
            //    return;
            //}

            //// Assign the material instance to the renderer
            //stateMachine.myMajorMinorSkin.material = materialInstance;

            //// Ensure transparency works in URP
            //materialInstance.SetFloat("_Surface", 1); // 1 = Transparent, 0 = Opaque
            //materialInstance.SetFloat("_Blend", 0); // 0 = Alpha blending
            //materialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            //materialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            //materialInstance.SetInt("_ZWrite", 0); // Disable ZWrite for transparency
            //materialInstance.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            //materialInstance.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent; // Set proper render order

            stateMachine.SetCurrentState(MommottiStateMachine.MommottiState.DEAD);
            //Debug.Log("Mommotti entered Death State");
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;

            if (timer > fadeTime)
            {
                stateMachine.myEntityAttributes.Destroy();
            }

            //Material materialInstance = stateMachine.myMajorMinorSkin.material;
            //Color color = materialInstance.color;
            //float alpha = color.a;

            //alpha = Mathf.Max(0, alpha - deltaTime / fadeTime);

            //if (alpha <= 0)
            //{
            //    stateMachine.myEntityAttributes.Destroy();
            //}
            //else
            //{
            //    color.a = alpha;
            //    materialInstance.color = color;
            //}
        }

        public override void Exit()
        {
        }
    }
}
