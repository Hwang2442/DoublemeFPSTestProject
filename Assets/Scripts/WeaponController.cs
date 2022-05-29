using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class WeaponController : MonoBehaviour
    {
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
        [SerializeField] List<ParticleSystem> m_particlePooling;

        #region Properties

        public KeyCode InstallKey => m_installKey;

        public bool IsWalking => AnimationCompare("Walk");
        public bool IsRunning => AnimationCompare("Run");
        public bool IsReloading => AnimationCompare("Recharge");

        #endregion


        private void Awake()
        {
            
        }

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

        public void PlayReloadAnimation()
        {
            if (m_curBullet >= m_maxBullet) return;

            m_animator.SetTrigger("Reloading");
            StartCoroutine(Co_WaitForAnimationComplete("Recharge", () =>
            {
                m_curBullet = m_maxBullet;
            }));
        }

        #endregion

        public void Swap(WeaponController weapon)
        {
            m_animator.SetTrigger("WeaponChange");
            StartCoroutine(Co_WaitForAnimationComplete("Hide", () =>
            {
                gameObject.SetActive(false);
                weapon.gameObject.SetActive(true);
            }));
        }

        public void Aiming(bool active)
        {
            m_animator.SetBool("Aiming", active);
        }

        public void Attack()
        {
            if (--m_curBullet <= 0)
            {
                m_curBullet = 0;
                m_animator.SetBool("Attack", false);

                return;
            }

            // Play fx.
            m_audioSource.PlayOneShot(m_shotSoundClip);
            PlayVFX();

            Debug.Log("Attack!!");
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

        private void PlayVFX()
        {
            ParticleSystem currentParticle = null;

            foreach (var particle in m_particlePooling)
            {
                if (!particle.isPlaying)
                {
                    currentParticle = particle;

                    break;
                }
            }

            if (currentParticle == null)
            {
                currentParticle = Instantiate(m_particlePrefab, m_muzzle);
                currentParticle.transform.localPosition = Vector3.zero;
                currentParticle.transform.localRotation = Quaternion.identity;

                m_particlePooling.Add(currentParticle);
            }

            currentParticle.Play();
        }
    }
}
