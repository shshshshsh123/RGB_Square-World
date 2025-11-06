using UnityEngine;
using UnityEngine.UI;

public enum CutInType
{
    Melee,
    Range,
    Magic
}

public class SkillCutInUI : MonoBehaviour
{
    public Image cutInImage;
    public Animator animator;
    string ANIM_TRIGGER_SHOW;
    string ANIM_TRIGGER_HIDE;

    public void ShowCutIn(CutInType cutInType, Sprite spriteImage = null)
    {
        gameObject.SetActive(true);
        if (cutInImage != null && spriteImage != null)
        {
            cutInImage.sprite = spriteImage;
        }
        if (animator != null)
        {
            switch(cutInType)
            {
                case CutInType.Melee:
                    ANIM_TRIGGER_SHOW = "Show_Melee";
                    ANIM_TRIGGER_HIDE = "Hide_Melee";
                    break;
                case CutInType.Range:
                    ANIM_TRIGGER_SHOW = "Show_Range";
                    ANIM_TRIGGER_HIDE = "Hide_Range";
                    break;
                case CutInType.Magic:
                    ANIM_TRIGGER_SHOW = "Show_Magic";
                    ANIM_TRIGGER_HIDE = "Hide_Magic";
                    break;
            }
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
