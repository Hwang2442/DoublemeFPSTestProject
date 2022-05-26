using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] Animator m_animator;

        [Header("Status")]
        [SerializeField] int m_damage;      // 데미지
        [SerializeField] float m_distance;  // 사거리
        [SerializeField] int m_maxBullet;   // 최대 총알 수 (-1 == infinity)
        [SerializeField] int m_curBullet;   // 현재 총알 수

        private void Awake()
        {
            
        }

        public void Walk(float velocity)
        {
            m_animator.SetFloat("Walk", velocity);
        }

        public void Run(bool active)
        {
            m_animator.SetBool("Run", active);
        }
    }
}
