using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingAnimation : MonoBehaviour
{
    public List<Animator> animations;
    bool flag = false;
    public void PlayAnimation()
    {
        foreach (var x in animations)
        {
            if (flag == false)
            {
                x.SetBool("Talking", true);
                Debug.Log("Started Talking");
                flag = true;
            }
            else
            {
                x.SetBool("Talking", false);
                Debug.Log("Stopped Talking");
                flag = false;
            }

        }
    }
}
