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
        [SerializeField] int m_damage = 2;
        [SerializeField] bool m_isAlert = false;

        [Header("SFX")]
        [SerializeField] AudioSource m_audioSource;
        [SerializeField] AudioClip m_clip;

        #region Properties

        public static EnemyManager Manager { get; set; }

        public bool Alert
        {
            get { return m_isAlert; }
            set
            {
                if (value)
                {
                    
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

            m_health.OnDamagedEvent.AddListener(Damaged);
            StartCoroutine(Co_EnemyUpdate());
        }

        private void Damaged()
        {
            // Dead
            if (m_health.curHP <= 0)
            {
                StopAllCoroutines();

                m_animator.ResetTrigger("Run");
                m_animator.ResetTrigger("Shoot");
                m_animator.SetTrigger("Dead");

                GetComponent<Collider>().enabled = false;
                StartCoroutine(Co_DeadAnimation());
            }
        }

        public void Attack()
        {
            m_audioSource.PlayOneShot(m_clip);

            Ray ray = new Ray(m_aimPoint.position, m_aimPoint.forward);
            GameManager.Instance.ShotCollision(new Ray(m_aimPoint.position, m_aimPoint.forward), m_range, m_damage);
        }

        private IEnumerator Co_EnemyUpdate()
        {
            PlayerController player = GameManager.Instance.Player;

            while (true)
            {
                yield return null;

                if (!m_isAlert)
                {
                    if (Vector3.SqrMagnitude(transform.position - player.transform.position) <= m_range * m_range)
                    {
                        Vector3 diff = transform.position - player.transform.position;

                        // 플레이어 발견 여부
                        if (Vector3.Dot(transform.forward, diff) < 0)
                        {
                            Alert = true;
                        }
                    }
                }
                else
                {
                    // 플레이거 거리 체크
                    if (Vector3.SqrMagnitude(transform.position - player.transform.position) > m_range * m_range)
                    {
                        // 플레이어 추격 완료까지 대기
                        yield return Co_ChasingPlayer();
                    }
                    else
                    {
                        if (!m_animator.GetBool("Shoot"))
                        {
                            m_animator.SetTrigger("Shoot");
                        }
                    }

                    transform.LookAt(player.transform, Vector3.up);
                }
            }
        }

        private IEnumerator Co_ChasingPlayer()
        {
            m_navigation.SetDestination(GameManager.Instance.Player.transform.position);

            m_animator.SetBool("Shoot", false);
            m_animator.SetTrigger("Run");

            while (true)
            {
                yield return null;

                // 플레이어 위치 실시간 반영
                m_navigation.destination = GameManager.Instance.Player.transform.position;

                yield return new WaitForSeconds(0.1f);

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

        private IEnumerator Co_DeadAnimation()
        {
            m_navigation.enabled = false;

            float t = 0;

            while (t < 1)
            {
                yield return null;

                t += Time.deltaTime;
                float x = Mathf.Lerp(0, -90, t);

                transform.rotation = Quaternion.Euler(x, transform.eulerAngles.y, transform.eulerAngles.z);
            }

            Manager.DestroyEnemy(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_aimPoint.transform.position, m_aimPoint.transform.position + m_aimPoint.forward * m_range);
        }
    }
}
