using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class Item : MonoBehaviour
    {
        public enum Kind
        {
            Health,
            Bullet
        }

        [SerializeField] Kind m_kind;

        [Space]
        [SerializeField] float m_rotationSpeed = 30;
        [SerializeField] int m_amount = 10;

        private void Update()
        {
            transform.Rotate(Vector3.up, m_rotationSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag != "Player") return;

            switch (m_kind)
            {
                case Kind.Health:
                    {
                        Health health = other.GetComponent<Health>();
                        if (health != null)
                        {
                            if (health.curHP < health.MaxHP)
                            {
                                health.OnRecovered(m_amount);

                                Destroy(gameObject);
                            }
                        }
                    }
                    break;
                case Kind.Bullet:
                    {
                        PlayerController player = GameManager.Instance.Player;

                        if (player.CurrentWeapon.CurBullet < player.CurrentWeapon.MaxBullet)
                        {
                            player.CurrentWeapon.RecoveryBullet(m_amount, () =>
                            {
                                player.BulletInfo.text = string.Format("{0} / {1}", player.CurrentWeapon.CurBullet.ToString("00"), player.CurrentWeapon.MaxBullet.ToString("00"));
                            });

                            Destroy(gameObject);
                        }
                    }
                    break;
            }
        }
    }
}
