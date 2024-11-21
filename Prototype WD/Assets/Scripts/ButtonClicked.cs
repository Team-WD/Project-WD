using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClicked : MonoBehaviour
{
    public GameObject beforeImage;
    public GameObject afterImage;

    public void click()
    {
        beforeImage.SetActive(false);
        afterImage.SetActive(true);
    } 
}
