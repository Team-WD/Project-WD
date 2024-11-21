using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfoUIController : MonoBehaviour
{
    public TextMeshProUGUI nickNameText;
    public Image characterImage;
    public Button weaponButton;
    public Image weaponImage;
    public GameObject readyStatus;
    public GameObject leaderStatus;
    
    public void SetNickName(string nickName)
    {
        nickNameText.text = nickName;
    }

    public void SetCharacterImage(Sprite sprite)
    {
        characterImage.sprite = sprite;
    }

    public void SetWeaponImage(Sprite sprite)
    {
        if (sprite == null)
            return;
        weaponImage.sprite = sprite;
        weaponImage.SetNativeSize();
        weaponButton.gameObject.SetActive(true);
    }

    public void SetReadyStatus(bool isReady)
    {
        readyStatus.SetActive(isReady);
        leaderStatus.SetActive(false);
    }

    public void SetAsLeader()
    {
        readyStatus.SetActive(false);
        leaderStatus.SetActive(true);
    }
}