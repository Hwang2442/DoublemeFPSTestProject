using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] Animator m_animator;
        [SerializeField] Health m_health;
        [SerializeField] Transform m_aimPoint;
        [SerializeField] UnityEngine.AI.NavMeshAgent m_navigation;

        [Header("Status")]
        [SerializeField] float m_speed = 3;
        [SerializeField] float m_range = 5;
        [SerializeField] bool m_isAlert = false;

        #region Properties

        public static EnemyManager Manager { get; set; }

        public bool Alert
        {
            get { return m_isAlert; }
            set
            {
                if (value)
                {
                    StartCoroutine(Co_ChasingPlayer());
                }

                m_isAlert = value;
            }
        }

        public Health Health => m_health;

        #endregion


        private void Start()
        {
            m_navigation.speed = m_speed;
            m_navigation.stoppingDistance = m_range;

            Alert = true;

            m_health.OnDamagedEvent.AddListener(Damaged);
        }

        private void Update()
        {
            
        }

        private IEnumerator Co_ChasingPlayer()
        {
            m_navigation.SetDestination(GameManager.Instance.Player.transform.position);
            m_animator.SetTrigger("Run");

            while (true)
            {
                yield return null;

                // 플레이어 위치 실시간 반영
                m_navigation.destination = GameManager.Instance.Player.transform.position;

                

                yield return null;
                yield return null;

                

                // 플레이어와의 거리 체크
                if (m_navigation.remainingDistance <= m_navigation.stoppingDistance)
                {
                    // 경로가 없거나 Agent의 이동이 0인 경우
                    if (!m_navigation.hasPath || m_navigation.velocity.sqrMagnitude <= 0)
                    {
                        // Navigation complete
                        m_animator.SetBool("Shoot", true);

                        break;
                    }
                }
            }
        }

        private void Damaged()
        {
            // Dead
            if (m_health.curHP <= 0)
            {
                m_animator.ResetTrigger("Run");
                m_animator.ResetTrigger("Shoot");
                m_animator.SetTrigger("Dead");

                GetComponent<Collider>().enabled = false;
                StartCoroutine(Co_DeadAnimation());
            }
        }

        private IEnumerator Co_DeadAnimation()
        {
            float t = 0;

            while (t < 1)
            {
                yield return null;

                t += Time.deltaTime;
                float x = Mathf.Lerp(0, -90, t);

                transform.rotation = Quaternion.Euler(x, transform.eulerAngles.y, transform.eulerAngles.z);
            }
        }
    }
}
