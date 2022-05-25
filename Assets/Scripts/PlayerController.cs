using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] Transform m_body;

        [Header("Camera")]
        [SerializeField] Camera m_camera;
        [SerializeField] float m_cameraSensivity = 100;

        [Header("Movement")]
        [SerializeField] CharacterController m_controller;
        [SerializeField] float m_speed = 10;
        [SerializeField] float m_jumpPower = 10;

        float m_xRotation = 0;
        Vector3 m_gravityVelocity;

        private void Awake()
        {
            
        }

        private void Update()
        {
            Movement();
            CameraUpdate();
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

            if (isGrounded && m_gravityVelocity.y < 0) m_gravityVelocity.y = 0;

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 direction = transform.forward * z + transform.right * x;
            m_controller.Move(direction * m_speed * Time.deltaTime);

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
    }
}
