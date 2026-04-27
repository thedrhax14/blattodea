using UnityEngine;

public class SimpleAnimation : MonoBehaviour
{
    [SerializeField]
    AnimationClip clip;
    [SerializeField]
    bool playAutomatically;
    [SerializeField]
    WrapMode wrapMode;
    new Animation animation;
    private void Awake()
    {
        if (clip != null)
        {
            animation = gameObject.AddComponent<Animation>();
            animation.AddClip(clip, clip.name);
            clip.legacy = true;
            clip.wrapMode = wrapMode;
            animation.clip = clip;
            if (playAutomatically)
            {
                animation.Play();
            }
        }
    }
    public Animation Animation => animation;
    public void Play()
    {
        animation.Play();
    }
    public void Stop()
    {
        animation.Stop();
    }
    public void SetAnimationClip(AnimationClip clip)
    {
        animation.clip = clip;
    }
}
