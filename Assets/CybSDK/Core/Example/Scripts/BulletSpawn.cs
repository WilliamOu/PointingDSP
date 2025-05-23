using UnityEngine;

namespace CybSDK
{
    public class BulletSpawn : MonoBehaviour
    {
        // Bullet Instanciation
        public GameObject BulletPrefab;

        // Bullet Setup
        public float BulletSpeed = 100.0f;

        // Behavior
        public const float BulletShootTime = 5.0f;
        private float bulletShootTimer = 0.0f;

        // Bullet parent
        public Transform bulletSpawnParent = null;


        // Update is called once per frame
        void Update()
        {
            // Shoot bullet every X (BulletShootTime)
            bulletShootTimer -= Time.deltaTime;
            if (bulletShootTimer < 0.0f)
            {
                ShootBullet();
                bulletShootTimer = BulletShootTime;
            }
        }

        void ShootBullet()
        {
            // Check Prefab
            if (BulletPrefab != null)
            {
                GameObject bulletGO = Instantiate(BulletPrefab, transform.position, transform.rotation, this.bulletSpawnParent);
                if (bulletGO != null)
                {
                    // Speed of bullet
                    Rigidbody bulletRb = bulletGO.GetComponent<Rigidbody>();
                    if (bulletRb != null)
                        bulletRb.AddForce(transform.up * BulletSpeed);

                    // Max-Duration of bullet
                    GameObject.Destroy(bulletGO, 10.0f);
                }
            }
            else
            {
                Debug.LogError("BulletSpawn.ShootBullet() - Error, no bullet prefab set");
            }
        }
    }
}


