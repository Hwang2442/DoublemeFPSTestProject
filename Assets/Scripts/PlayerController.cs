using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FPS
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] Transform m_body;
        [SerializeField] Health m_health;

        [Header("Camera settings")]
        [SerializeField] Camera m_camera;
        [SerializeField] float m_cameraSensivity = 100;

        [Header("Movement settings")]
        [SerializeField] CharacterController m_controller;
        [SerializeField] float m_walkSpeed = 10;
        [SerializeField] float m_runSpeed = 15;
        [SerializeField] float m_jumpPower = 10;

        [Header("Weapon settings")]
        [SerializeField] int m_curWeaponIndex = 0;
        [SerializeField] WeaponController[] m_weapons;

        [Header("UI")]
        [SerializeField] RectTransform m_crosshair;
        [SerializeField] Text m_curHp;
        [SerializeField] Text m_curWeaponInfo;
        [SerializeField] Text m_bulletInfo;

        float m_xRotation = 0;
        Vector3 m_gravityVelocity;

        #region Properties

        public WeaponController CurrentWeapon => m_weapons[m_curWeaponIndex];

        public Camera PlayerCamera => m_camera;

        public Text BulletInfo => m_bulletInfo;

        #endregion

        private void Start()
        {
            m_curWeaponInfo.text = CurrentWeapon.WeaponName;
            m_bulletInfo.text = string.Format("{0} / {1}", CurrentWeapon.CurBullet.ToString("00"), CurrentWeapon.MaxBullet.ToString("00"));
            m_curHp.text = m_health.curHP.ToString();

            foreach (var weapon in m_weapons)
            {
                weapon.AttackEvent.AddListener(() =>
                {
                    m_bulletInfo.text = string.Format("{0} / {1}", CurrentWeapon.CurBullet.ToString("00"), CurrentWeapon.MaxBullet.ToString("00"));
                });
            }

            m_health.onRecoverEvent.AddListener(() =>
            {
                m_curHp.text = m_health.curHP.ToString();
            });
            m_health.OnDamagedEvent.AddListener(() =>
            {
                m_curHp.text = m_health.curHP.ToString();
            });
        }

        private void Update()
        {
            KeyboardInteraction();
            MouseInteraction();
        }

        private void MouseInteraction()
        {
            // attack start.
            if (Input.GetMouseButtonDown(0))
            {
                CurrentWeapon.PlayAttackAnimation(true);
            }
            // attack end.
            else if (Input.GetMouseButtonUp(0))
            {
                CurrentWeapon.PlayAttackAnimation(false);
            }

            CameraUpdate();
        }

        private void KeyboardInteraction()
        {
            Movement();

            // Reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                CurrentWeapon.PlayReloadAnimation(() =>
                {
                    m_bulletInfo.text = string.Format("{0} / {1}", CurrentWeapon.CurBullet.ToString("00"), CurrentWeapon.MaxBullet.ToString("00"));
                });
            }

            // Weapon change.
            for (int i = 0; i < m_weapons.Length; i++)
            {
                if (m_curWeaponIndex != i && Input.GetKeyDown(m_weapons[i].InstallKey))
                {
                    CurrentWeapon.Swap(m_weapons[i], () =>
                    {
                        m_bulletInfo.text = string.Format("{0} / {1}", CurrentWeapon.CurBullet.ToString("00"), CurrentWeapon.MaxBullet.ToString("00"));
                    });

                    m_curWeaponIndex = i;

                    break;
                }
            }
        }

        private void CameraUpdate()
        {
            float mouseX = Input.GetAxis("Mouse X") * m_cameraSensivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * m_cameraSensivity * Time.deltaTime;

            m_xRotation = Mathf.Clamp(m_xRotation - mouseY, -90, 45);

            transform.Rotate(Vector3.up, mouseX);
            m_body.localRotation = Quaternion.Euler(m_xRotation, m_body.localEulerAngles.y, 0);
        }

        private void Movement()
        {
            bool isGrounded = GroundCheck();
            bool isRun = Input.GetKey(KeyCode.LeftShift) && (CurrentWeapon.IsWalking || CurrentWeapon.IsRunning);

            if (isGrounded && m_gravityVelocity.y < 0) m_gravityVelocity.y = 0;

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            isRun = isRun && z > 0;

            // Play walk or run animation.
            CurrentWeapon.PlayWalkAniamtion(Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)));
            CurrentWeapon.PlayRunAnimation(isRun);

            // Move
            Vector3 direction = transform.forward * z + transform.right * x;
            m_controller.Move(direction * (isRun ? m_runSpeed : m_walkSpeed) * Time.deltaTime);

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                m_gravityVelocity.y = m_jumpPower;
            }

            // 중력 계산 및 적용
            m_gravityVelocity += Physics.gravity * Time.deltaTime;
            m_controller.Move(m_gravityVelocity * Time.deltaTime);
        }

        private bool GroundCheck()
        {
            return Physics.Raycast(transform.position, Vector3.down, 0.1f);
        }

        private void OnDrawGizmos()
        {
            // Ground check.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.1f);
        }
    }
}
