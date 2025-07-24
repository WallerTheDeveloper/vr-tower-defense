using System;
using Core.StateMachine;
using UnityEngine;

namespace Core.TowersBehaviour.States
{
    public class TowerIdle : MonoBehaviour, IState
    {
        [SerializeField] private Animator animator;
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        public void Enter()
        {
            IsStateActive = true;
            animator.enabled = true;
            animator.Play("Tower Idle");
        }

        public void Tick()
        {
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