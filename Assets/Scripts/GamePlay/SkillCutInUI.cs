using UnityEngine;
using UnityEngine.UI;

public class SkillCutInUI : MonoBehaviour
{
    public Image cutInImage;
    public Animator animator;

    private const string ANIM_TRIGGER_SHOW = "Show";
    private const string ANIM_TRIGGER_HIDE = "Hide";

    public void ShowCutIn(Sprite spriteImage = null)
    {
        gameObject.SetActive(true);
        if (cutInImage != null && spriteImage != null)
        {
            cutInImage.sprite = spriteImage;
        }
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_SHOW);
        }
    }

    public void HideCutIn()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_HIDE);
        }
        gameObject.SetActive(false);
    }
}
