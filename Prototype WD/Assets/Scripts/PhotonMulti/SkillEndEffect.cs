using UnityEngine;

public class SkillEndEffect : MonoBehaviour
{
    void Start()
    {
        // 애니메이션 길이를 가져와서 해당 시간 후에 오브젝트 파괴
        Animator animator = GetComponent<Animator>();
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animationLength);
    }
}