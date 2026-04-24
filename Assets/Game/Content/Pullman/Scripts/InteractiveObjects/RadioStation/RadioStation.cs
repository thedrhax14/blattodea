using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;


public class RadioStation : MonoBehaviour
{

    [SerializeField]
    TextMeshPro textMesh;
    [SerializeField]
    AudioSource audioSourceEffects, audioSourceMusic;
    [SerializeField]
    RadioButton radioButtonNext, radioButtonPrev;
    [SerializeField]
    RadioKnob Volume;
    [SerializeField]
    SFXClass SFX;
    [SerializeField]
    Light Light;
    ControllerClass Controller;
    bool isTurned = false;
    private void Awake()
    {
        Controller = new ControllerClass(this);
        radioButtonNext.Init(() =>
        {
            trackNext();
        });
        radioButtonPrev.Init(() =>
        {
            trackPrev();
        });
        Volume.Init((val) =>
        {
            Controller.ChangeVolume(val);
            rotateVolumeObject();
            if (Controller.VolumeCurrent == 0)
            {
                turnOff();
            }
            else
            {
                turnOn();
                audioSourceMusic.volume = Controller.VolumeCurrent;
            }
        });
        turnOn();
        trackNext();
        audioSourceMusic.Play();
        rotateVolumeObject();
    }
    void rotateVolumeObject()
    {
        Volume.transform.localRotation = Quaternion.Euler(0, 0, 300 - Controller.VolumeCurrent * 300);
    }
    void trackNext()
    {
        if (!isTurned) { return; }
        Debug.Log("track next");
        Controller.MusicNext();
        changeTrack();
    }
    void trackPrev()
    {
        if (!isTurned) { return; }
        Controller.MusicPrev();
        changeTrack();
    }
    IEnumerator trackingCor = null;
    WaitForSeconds waiter = new WaitForSeconds(2);
    IEnumerator playTracking()
    {
        while (true)
        {
            yield return waiter;
            if (!audioSourceMusic.isPlaying)
            {
                trackNext();
            }
        }
    }
    void changeTrack()
    {
        playAudioClip(SFX.Change);
        textMesh.text = Controller.MusicCurrentName;
    }
    void turnOff()
    {
        if (!isTurned)
        {
            return;
        }
        isTurned = false;
        playAudioClip(SFX.TurnOffOn);
        Light.enabled = false;
        textMesh.enabled = false;
        StopCoroutine(trackingCor);
        trackingCor = null;
    }
    void turnOn()
    {
        if (isTurned)
        {
            return;
        }
        isTurned = true;
        playAudioClip(SFX.TurnOffOn);
        Light.enabled = true;
        textMesh.enabled = true;
        trackingCor = playTracking();
        StartCoroutine(trackingCor);
    }
    void playAudioClip(AudioClip clip)
    {
        audioSourceEffects.clip = clip;
        audioSourceEffects.Play();
    }
    public class ControllerClass
    {
        public int MusicIndexCurrent { get; private set; } = 0;
        public string MusicCurrentName { get; private set; }
        public float VolumeCurrent { get; private set; } = 1;
        RadioStation owner;
        public ControllerClass(RadioStation owner)
        {
            this.owner = owner;
        }
        public void MusicNext()
        {
            MusicIndexCurrent++;
            if (MusicIndexCurrent >= owner.SFX.Musics.Count)
            {
                MusicIndexCurrent = 0;
            }
            MusicSet(MusicIndexCurrent);
        }
        public void MusicPrev()
        {
            MusicIndexCurrent--;
            if (MusicIndexCurrent < 0)
            {
                MusicIndexCurrent = owner.SFX.Musics.Count - 1;
            }
            MusicSet(MusicIndexCurrent);
        }
        public void MusicSet(int index)
        {
            owner.audioSourceMusic.clip = owner.SFX.Musics[index];
            owner.audioSourceMusic.Play();
            MusicCurrentName = owner.audioSourceMusic.clip.name;
        }
        public void ChangeVolume(float delta)
        {
            VolumeCurrent += delta;
            VolumeCurrent = Mathf.Clamp(VolumeCurrent, 0, 1);
            Debug.Log(VolumeCurrent);
        }
    }

    [Serializable]
    public class SFXClass
    {
        [SerializeField]
        List<AudioClip> musics;
        [SerializeField]
        AudioClip change, buttonClick, turnOffOn;
        public AudioClip Change { get => change; }
        public AudioClip ButtonClick { get => buttonClick; }
        public AudioClip TurnOffOn { get => turnOffOn; }
        public IReadOnlyList<AudioClip> Musics { get => musics; }
    }

}
