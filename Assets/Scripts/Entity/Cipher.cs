using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cipher : Door
{
    [SerializeField] Animator innerRing;
    int innerRingHash = Animator.StringToHash("IsOpen");
    [SerializeField] Animator orb;
    int orbHash = Animator.StringToHash("IsOpen");
    [SerializeField] Animator outsideRing;
    int outsideRingHash = Animator.StringToHash("IsOpen");
    
    public override void OnOpen() => SetAnimator(true);

    public override void OnClose() => SetAnimator(false);

    void SetAnimator(bool isOpen )
    {
        innerRing.SetBool(innerRingHash, isOpen);
        orb.SetBool(orbHash, isOpen);
        outsideRing.SetBool(outsideRingHash, isOpen);
    }
}
