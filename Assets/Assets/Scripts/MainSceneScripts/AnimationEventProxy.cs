using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    private Unit parentUnit;

    void Start()
    {
        // Üst objedeki Unit scriptini bul
        parentUnit = GetComponentInParent<Unit>();
    }

    // Animasyon klibi bu fonksiyonu tetikleyecek, bu da ana scriptteki iþlemi baþlatacak
    public void ProxyFireArrowEvent()
    {
        if (parentUnit != null)
        {
            parentUnit.OnAttackAnimationEvent();
        }
    }
}