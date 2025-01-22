//using UnityEngine;
//using System.Collections.Generic;


//public class MomettiBehaviour : MonoBehaviour
//{
//    List<GameObject> myGroup;
//    public int amountAttackersAtTheSameTime;
//    public int amountCurrentlyAttacking = 0;
//    public bool everyoneAlerted = false;

//    private void Start()
//    {
//        myGroup = GetComponent<EnemySpawnPoint>().spawnGroup;
//    }

//    private void Update()
//    {
//        SortListWithDistanceToPlayer();
//        amountCurrentlyAttacking = 0;

//        foreach (GameObject mometti in myGroup)
//        {
//            if (mometti.GetComponent<EntityAttributes>().health <= 0)
//            {
//                myGroup.Remove(mometti.gameObject);
//                Destroy(mometti);
//                break;
//            }

//            //if someone is chasing notify the others
//            if (mometti.GetComponent<EntityAttributes>().currentState == EntityAttributes.EntityState.CHASE)
//            {
//                everyoneAlerted = true;
//            }

//            //if notified start chasing
//            if (everyoneAlerted && mometti.GetComponent<EntityAttributes>().currentState != EntityAttributes.EntityState.CHASE && mometti.GetComponent<EntityAttributes>().currentState != EntityAttributes.EntityState.ATTACK)
//            {
//                mometti.GetComponent<EnemyAttributes>().SwitchCircle();
//            }

//            //number of currently attacking
//            if (mometti.GetComponent<EntityAttributes>().currentState == EntityAttributes.EntityState.ATTACK)
//            {
//                amountCurrentlyAttacking++;

//                if (amountCurrentlyAttacking > amountAttackersAtTheSameTime)
//                {
//                    mometti.GetComponent<EnemyAttributes>().SwitchCircle();
//                }
//            }
//        }

//        if (amountCurrentlyAttacking < amountAttackersAtTheSameTime && amountCurrentlyAttacking < myGroup.Count && everyoneAlerted)
//        {

//            for (int i = 0; i < myGroup.Count; i++)
//            {
//                if (myGroup[i].GetComponent<EntityAttributes>().currentState != EntityAttributes.EntityState.ATTACK)
//                {
//                    myGroup[i].GetComponent<EnemyAttributes>().SwitchAttack();
//                    break;
//                }
//            }
//        }
//    }

//    private void SortListWithDistanceToPlayer()
//    {
//        for (int i = 1; i < myGroup.Count; i++)
//        {
//            for (int j = 0; j < i; j++)
//            {
//                if (myGroup[i].GetComponent<EnemyAttributes>().currentDistanceToPlayer < myGroup[j].GetComponent<EnemyAttributes>().currentDistanceToPlayer)
//                {
//                    (myGroup[j], myGroup[i]) = (myGroup[i], myGroup[j]);
//                }
//            }
//        }
//    }
//}
