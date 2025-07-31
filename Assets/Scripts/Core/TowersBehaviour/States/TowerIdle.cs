using System;
using Core.StateMachine;
using UnityEngine;

namespace Core.TowersBehaviour.States
{
    public class TowerIdle : MonoBehaviour, IState
    {
        [SerializeField] private GameObject towerHead;
        [SerializeField] private Animator animator;
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        public void Enter(object enterObject)
        {
            IsStateActive = true;
            animator.enabled = false;
        }

        public void Tick()
        {
            if (!animator.enabled)
            { 
                towerHead.transform.rotation = Quaternion.RotateTowards(
                    towerHead.transform.rotation,
                    Quaternion.identity,
                    90f * Time.deltaTime);
            }
            
            if (towerHead.transform.rotation == Quaternion.identity && !animator.enabled)
            {
                animator.enabled = true;
                animator.Play("Tower Idle");
            }
        }

        public void FixedTick()
        {
        }

        public void Exit()
        {
            animator.enabled = false;
            IsStateActive = false;
        }
    }
}