using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class BulletAttack : NetworkBehaviour
{
    [SerializeField] int Damage;
    [SerializeField] GameObject bulletPrefab; // Assign your bullet prefab in the inspector

    [SerializeField] InputAction attack;
    [SerializeField] InputAction attackLocation;

    [SerializeField] float bulletSpeed = 10f;

    private bool isAttacking = false; // Flag to prevent multiple attacks per frame

    private void OnEnable()
    {
        attack.Enable();
        attackLocation.Enable();
    }

    private void OnDisable()
    {
        attack.Disable();
        attackLocation.Disable();
    }

    void Update()
    {
        if (!HasStateAuthority) return;

        if (attack.triggered && !isAttacking)
        {
            isAttacking = true;

            Vector2 attackLocationInScreenCoordinates = attackLocation.ReadValue<Vector2>();

            var camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(attackLocationInScreenCoordinates);
            ray.origin += camera.transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.transform.gameObject;
                Debug.Log("Raycast hit: name=" + hitObject.name + " tag=" + hitObject.tag + " collider=" + hit.collider);
                if (hitObject.TryGetComponent<Health>(out var health))
                {
                    Debug.Log("Dealing damage");
                    health.DealDamageRpc(Damage);

                    // If the hit object has a ScoreManager component, add score to the player who shot the bullet
                    ScoreManager scoreManager = GetComponent<ScoreManager>();
                    if (scoreManager != null)
                    {
                        scoreManager.AddScoreRpc(1); // Increase score by 1 point
                    }
                }

                // Calculate direction to the hit point
                Vector3 directionToHit = (hit.point - ray.origin).normalized;

                // Instantiate bullet at player's position
                GameObject bullet = Instantiate(bulletPrefab, ray.origin, Quaternion.identity);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

                // Set bullet direction and speed
                bulletRb.velocity = directionToHit * bulletSpeed;

                // Destroy bullet after reaching the hit point
                Destroy(bullet, hit.distance / bulletSpeed);
            }
            else
            {
                // If no hit, just destroy the bullet after a certain time
                GameObject bullet = Instantiate(bulletPrefab, ray.origin, Quaternion.identity);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

                // Set bullet direction and speed
                bulletRb.velocity = ray.direction * bulletSpeed;

                // Destroy bullet after a certain time
                Destroy(bullet, 2f); // Adjust the time as needed
            }
        }
        else if (!attack.triggered)
        {
            isAttacking = false;
        }
    }
}
