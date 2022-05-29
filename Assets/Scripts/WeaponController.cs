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
        [SerializeField] int m_maxBullet;   // 최대 총알 수 (-1 == infinity)
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

        public int MaxBullet => m_maxBullet;
        public int CurBullet => m_curBullet;

        public UnityEngine.Events.UnityEvent AttackEvent => m_attackEvent;

        #endregion

        #region Play animations

        public void PlayWalkAniamtion(float velocity)
        {
            m_animator.SetFloat("Walk", velocity);
        }

        public void PlayRunAnimation(bool active)
        {
            m_animator.SetBool("Run", active);
        }

        public void PlayAttackAnimation(bool active)
        {
            m_animator.SetBool("Attack", m_curBullet > 0 ? active : false);
        }

        public void PlayReloadAnimation(UnityEngine.Events.UnityAction callback = null)
        {
            if (m_curBullet >= m_maxBullet) return;

            m_animator.SetTrigger("Reloading");
            StartCoroutine(Co_WaitForAnimationComplete("Recharge", () =>
            {
                m_curBullet = m_maxBullet;
                callback?.Invoke();
            }));
        }

        #endregion

        public void Swap(WeaponController weapon, UnityEngine.Events.UnityAction callback = null)
        {
            m_animator.SetTrigger("WeaponChange");
            StartCoroutine(Co_WaitForAnimationComplete("Hide", () =>
            {
                gameObject.SetActive(false);
                weapon.gameObject.SetActive(true);

                callback?.Invoke();
            }));
        }

        public void Aiming(bool active)
        {
            m_animator.SetBool("Aiming", active);
        }

        public void Attack()
        {
            // 총알 체크
            if (--m_curBullet < 0)
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
            m_audioSource.PlayOneShot(m_shotSoundClip);
            GameManager.Instance.PlayVFX(m_particlePrefab, m_particlePooling, m_muzzle, Vector3.zero, Quaternion.identity);
        }

        private bool AnimationCompare(string animationName)
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        }

        private IEnumerator Co_WaitForAnimationComplete(string animationName, UnityEngine.Events.UnityAction callback = null)
        {
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
