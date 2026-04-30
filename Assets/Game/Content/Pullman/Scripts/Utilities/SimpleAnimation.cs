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
        if(animation == null) Debug.LogError("No animation clip assigned to " + gameObject.name, this);
        else animation.Play();
    }
    public void Stop()
    {
        if(animation == null) Debug.LogError("No animation clip assigned to " + gameObject.name, this);
        else animation.Stop();
    }
    public void SetAnimationClip(AnimationClip clip)
    {
        if(animation == null) Debug.LogError("No animation component found on " + gameObject.name, this);
        else animation.clip = clip;
    }
}
