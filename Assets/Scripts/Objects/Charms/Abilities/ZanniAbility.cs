using ProjectColombo.StateMachine.Mommotti;
using UnityEngine;

namespace ProjectColombo.Objects.Masks
{
    public class ZanniAbility : BaseAbility
    {
        [Header("Zanni")]
        public float radiusOfEffect;

        public override void UseAbility()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject e in enemies)
            {
                float distance = Vector3.Distance(myPlayerStateMachine.transform.position, e.transform.position);

                if (distance < radiusOfEffect)
                {
                    e.GetComponent<MommottiStateMachine>().SetStaggered();
                }
            }
        }

        public override void EndAbility()
        {
            
        }
    }
}