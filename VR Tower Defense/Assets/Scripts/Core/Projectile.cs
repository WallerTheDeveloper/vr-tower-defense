using UnityEngine;

namespace Core
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        [SerializeField] private float lifetime = 3f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Initialize(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        
            Destroy(gameObject, lifetime);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Debug.Log("Hit an enemy!");
                Destroy(gameObject);
            }
        }
    }
}