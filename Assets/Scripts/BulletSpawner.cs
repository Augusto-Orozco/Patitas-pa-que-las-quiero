using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [Header("Bala")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;

    [Header("Disparo")]
    public float minShootTime = 0.5f;
    public float maxShootTime = 1.5f;

    [Header("Dirección")]
    public float minAngle = 25f;
    public float maxAngle = 65f;

    private float timer;
    private float nextShootTime;

    private void Start()
    {
        SetNextShootTime();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextShootTime)
        {
            Shoot();
            timer = 0f;

            SetNextShootTime();
        }
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.identity
        );

        float angle = Random.Range(minAngle, maxAngle);

        Vector2 direction = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        rb.velocity = direction * bulletSpeed;
    }

    private void SetNextShootTime()
    {
        nextShootTime = Random.Range(minShootTime, maxShootTime);
    }
}