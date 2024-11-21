// PlayerFireAttack.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFireAttack : MonoBehaviour
{
    #region Private Fields

    public GameObject BulletPrefab;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    #region Private Methods

    void Fire()
    {
        GameObject Bullet = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        
        Vector3 direction = (mousePosition - Bullet.transform.position).normalized;
        
        Bullet.GetComponent<Rigidbody2D>().velocity = direction * 20;
    }

    #endregion
}
