using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] string m_weaponName;
        [SerializeField] KeyCode m_installKey;
        [SerializeField] Animator m_animator;

        [Header("Status")]
        [SerializeField] int m_damage;      // 데미지
        [SerializeField] float m_distance;  // 사거리
        [SerializeField] int m_maxBullet;   // 최대 총알 수 (0 == infinity)
        [SerializeField] int m_curBullet;   // 현재 총알 수

        [Header("Sounds")]
        [SerializeField] AudioSource m_audioSource;
        [SerializeField] AudioClip m_shotSoundClip;

        [Header("VFX")]
        [SerializeField] Transform m_muzzle;
        [SerializeField] ParticleSystem m_particlePrefab;
        [SerializeField] List<ParticleSystem> m_particlePooling = new List<ParticleSystem>();

        [Space]
        [SerializeField] UnityEngine.Events.UnityEvent m_attackEvent;

        #region Properties

        public KeyCode InstallKey => m_installKey;

        public string WeaponName => m_weaponName;

        public bool IsWalking => AnimationCompare("Walk");
        public bool IsRunning => AnimationCompare("Run");
        public bool IsReloading => AnimationCompare("Recharge");
        public bool IsSwapping => AnimationCompare("Hide") || AnimationCompare("Get");

        public int MaxBullet => m_maxBullet;
        public int CurBullet => m_curBullet;

        public UnityEngine.Events.UnityEvent AttackEvent => m_attackEvent;

        #endregion

        #region Play animations

        /// <summary>
        /// 걷는 애니메이션 재생
        /// </summary>
        /// <param name="velocity">이동량</param>
        public void PlayWalkAniamtion(float velocity)
        {
            if (gameObject.activeSelf)
            {
                m_animator.SetFloat("Walk", velocity);
            }
        }

        /// <summary>
        /// 달리기 애니메이션 재생
        /// </summary>
        /// <param name="active"></param>
        public void PlayRunAnimation(bool active)
        {
            if (gameObject.activeSelf)
            {
                m_animator.SetBool("Run", active);
            }
        }

        /// <summary>
        /// 공격 애니메이션 재생
        /// </summary>
        /// <param name="active"></param>
        public void PlayAttackAnimation(bool active)
        {
            // 총알 갯수 확인 후 재생 여부 결정
            m_animator.SetBool("Attack", (m_curBullet > 0 || m_maxBullet <= 0) ? active : false);
        }

        /// <summary>
        /// 장전 애니메이션 재생
        /// </summary>
        /// <param name="callback"></param>
        public void PlayReloadAnimation(UnityEngine.Events.UnityAction callback = null)
        {
            // 현재 총알이 모두 채워져있는 경우 장전 불필요
            if (m_curBullet >= m_maxBullet) return;

            m_animator.SetTrigger("Reloading");
            StartCoroutine(Co_WaitForAnimationComplete("Recharge", () =>
            {
                RecoveryBullet(m_maxBullet - m_curBullet);
                callback?.Invoke();
            }));
        }

        #endregion

        /// <summary>
        /// 총알 충전
        /// </summary>
        /// <param name="amount">충전할 양</param>
        /// <param name="callback">충전 시 호출할 이벤트</param>
        public void RecoveryBullet(int amount, UnityEngine.Events.UnityAction callback = null)
        {
            m_curBullet = Mathf.Min(m_curBullet + amount, m_maxBullet);
            callback?.Invoke();
        }

        /// <summary>
        /// 무기 교체
        /// </summary>
        /// <param name="weapon">교체할 무기</param>
        /// <param name="callback">교체 완료 후 호출할 이벤트</param>
        public void Swap(WeaponController weapon, UnityEngine.Events.UnityAction callback = null)
        {
            m_animator.SetTrigger("WeaponChange");
            StartCoroutine(Co_WaitForAnimationComplete(() =>
            {
                weapon.gameObject.SetActive(true);
                gameObject.SetActive(false);

                callback?.Invoke();
            }));
        }

        public void Aiming(bool active)
        {
            m_animator.SetBool("Aiming", active);
        }

        public void Attack()
        {
            // 총알 제한이 있는 무기
            if (m_maxBullet > 0)
            {
                m_curBullet--;
            }

            // 총알 체크
            if (m_curBullet < 0)
            {
                m_curBullet = 0;
                m_animator.SetBool("Attack", false);

                return;
            }

            Debug.Log("Attack!!");

            // Raycast
            Camera camera = GameManager.Instance.Player.PlayerCamera;
            Vector3 position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            GameManager.Instance.ShotCollision(camera.ScreenPointToRay(position), m_distance, m_damage);

            m_attackEvent.Invoke();

            // Play fx.
            if (m_shotSoundClip != null && m_audioSource != null)
            {
                m_audioSource.PlayOneShot(m_shotSoundClip);
            }
            if (m_particlePrefab != null)
            {
                GameManager.Instance.PlayVFX(m_particlePrefab, m_particlePooling, m_muzzle, Vector3.zero, Quaternion.identity);
            }
        }

        private bool AnimationCompare(string animationName)
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        }

        private bool AnimationComplete()
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
        }

        private IEnumerator Co_WaitForAnimationComplete(UnityEngine.Events.UnityAction callback = null)
        {
            yield return new WaitForSeconds(0.25f);

            while (true)
            {
                yield return null;

                if (AnimationComplete())
                {
                    break;
                }
            }

            callback?.Invoke();
        }

        /// <summary>
        /// 애니메이션 완료 대기
        /// </summary>
        /// <param name="animationName">확인할 애니메이션 이름</param>
        /// <param name="callback">완료 후 콜백</param>
        /// <returns></returns>
        private IEnumerator Co_WaitForAnimationComplete(string animationName, UnityEngine.Events.UnityAction callback = null)
        {
            yield return new WaitForSeconds(0.25f);

            while (true)
            {
                yield return null;

                if (!AnimationCompare(animationName))
                {
                    break;
                }
            }

            callback?.Invoke();
        }
    }
}
