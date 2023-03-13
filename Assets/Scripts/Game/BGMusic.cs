using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class BGMusic : MonoBehaviour {
    public static BGMusic Instance;
    int reloads = 0;
    public AudioMixerGroup audioMixer;
    public List<AudioClip> audioClips;
    // LevelManager mgr;
    // public LevelManager lmgr;
    AudioSource audioSrc;
    public int currSong = 0;
    bool isPlaying = false;

    void Awake() {
        if (Instance != null && Instance.reloads != 0) return;
        if (Instance == null) Instance = this;
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.outputAudioMixerGroup = audioMixer;
        audioSrc.playOnAwake = false;
        audioSrc.loop = false;

        PlaySong(currSong);
        
        if (Instance.reloads == 0) DontDestroyOnLoad(gameObject);
        reloads++;
    }

    public void PlaySong(int index) {
        audioSrc.clip = audioClips[index];
        audioSrc.Play();
        isPlaying = true;
    }

    // Update is called once per frame
    void Update() {
        if (!isPlaying || audioSrc.isPlaying) return; // we done
        isPlaying = false;
        // if (LevelManager.Instance.currentLevel == LevelManager.Instance.lastLevel) currSong++;
        // currSong++;
        // currSong %= audioClips.Count;
        // Debug.Log(currSong);
        PlaySong(currSong);
    }
}