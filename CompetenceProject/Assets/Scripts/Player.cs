using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
	public PlayerSO _playerRef;
	public Gun _gunRef;
	public Transform gun;
	public bool canFire = true;
    [SerializeField] private int currentHealth;


    private void Start()
    {
	    gameObject.name = _playerRef.playerName;
    }

	private void Update()
	{
		Vector3 newPos = transform.position + new Vector3(-Input.GetAxis("Vertical"), 0.0f, Input.GetAxis("Horizontal")) * _playerRef.speed;
		//Vector3 newDir = transform.position + new Vector3(-Input.GetAxis("Vertical"), 0.0f, Input.GetAxis("Horizontal")) * _playerRef.speed;
        transform.LookAt(Vector3.Lerp(transform.position + transform.forward, newPos, _playerRef.turnTime));
        transform.position = newPos;
	}

	private void FixedUpdate()
	{
		if (Input.GetMouseButton(0) && canFire)
		{
            Vector3 dir = Input.mousePosition;
            for (int i = 0; i < _gunRef.stats.spread; i++)
            {
	            GameObject bullet = Instantiate(_gunRef.bullet, gun.position, Quaternion.identity);
	            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
	            bulletRigidbody.AddForce(transform.forward * _gunRef.bulletSpeed * 1000);
            }
			StartCoroutine(triggerDelay());
        }
    }
	void SpawnBulletECS(Vector3 rotation)
	{

	}

	void SpawnBulletSpreadECS(Vector3 rotation)
	{
		
	}

	private IEnumerator triggerDelay()
	{
		canFire = false;
        yield return new WaitForSeconds(1 / _gunRef.stats.firingRate);
		canFire = true;
		StopCoroutine(triggerDelay());
	}
}