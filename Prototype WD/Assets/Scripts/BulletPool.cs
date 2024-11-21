using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    //public static BulletPool Instance;
    public GameObject[] bulletPrefabs;
    public int poolSize = 20;
    private List<List<GameObject>> bullets;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        bullets = new List<List<GameObject>>();
        for(int k=0; k<bulletPrefabs.Length; k++){
            bullets.Add(new List<GameObject>());
            for (int i = 0; i < poolSize; i++)
                {
                    GameObject bullet = Instantiate(bulletPrefabs[k]);
                    bullet.SetActive(false);
                    bullets[k].Add(bullet);
                }
            }
    }

    public GameObject GetBullet(int id)
    {
        foreach (GameObject bullet in bullets[id])
        {
            if (!bullet.activeInHierarchy)
            {
                bullet.SetActive(true);
                return bullet;
            }
        }
        return null;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
    }
}