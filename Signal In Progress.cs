using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioType
    {
        Audio2D,
        Audio3D
    }
    public static AudioManager Instance { get; private set; }
    private readonly Queue<AudioSource> audioPool = new();
    private byte poolLimit = 20;
    public AudioSource audio2D_Prefab;
    public AudioSource audio3D_Prefab;
    public AudioClip errorClip;

    // Custom class to set Key and Value in Inspector.
    public AudioDictionary audioDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Quick way to get Audio Clips form the Audio Managers Dictionary.
    /// Example: AudioClip audioClip = GetAudioClip("Monster_Attack");
    /// </summary>
    /// <param name="audioDictionaryKey"></param>
    /// <returns></returns>
    public AudioClip GetAudioClip(string audioDictionaryKey)
    {
        audioDictionary.ToDictionary().TryGetValue(audioDictionaryKey.ToLower(), out AudioClip audioClip);

        if (audioClip != null)
        {
            return audioClip;
        }
        else
        {
            Debug.LogError($"No clip found with Key: {audioDictionaryKey}. Did you misspell or forget to add it in Audio Manager?");
            return errorClip;
        }
    }

    /// <summary>
    /// Method that spawns in a audio_Prefab and Get a clip from audioDictionary.
    /// Example: PlayClip(transform.position, "Monster_Howl");
    /// </summary>
    /// <param name="spawnPosition"></param>
    /// <param name="audioDictionaryKey"></param>
    /// <param name="oneShot"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <returns></returns>
    public void PlayClip(Vector3 spawnPosition, string audioDictionaryKey, AudioType audioSourcePrefab, float volume = 1, float pitch = 1)
    {
        
        AudioSource audioSource;
        AudioClip clip = GetAudioClip(audioDictionaryKey);

        if (audioSourcePrefab == AudioType.Audio3D)
        {
            audioSource = GetFormPool(audio3D_Prefab, spawnPosition, false, clip.length);
        }
        else
        {
            audioSource = GetFormPool(audio2D_Prefab, spawnPosition, false, clip.length);
        }

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    /// <summary>
    /// Method that spawns in a audio_Prefab and also returns it so you can access it for modification.
    /// Example: AudioSource audioSource = PlayClip(transform.position, "Monster_Attack", "3d");
    /// </summary>
    /// <param name="spawnPosition"></param>
    /// <param name="audioDictionaryKey"></param>
    /// <param name="oneShot"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <returns></returns>
    public AudioSource PlayAudioClip(Vector3 spawnPosition, string audioDictionaryKey, AudioType audioSourcePref, bool oneShot = true, float volume = 1, float pitch = 1)
    {
        AudioSource audioSource;
        AudioClip clip = GetAudioClip(audioDictionaryKey);

        if (audioSourcePref == AudioType.Audio3D)
        {
            audioSource = GetFormPool(audio3D_Prefab, spawnPosition, oneShot, clip.length);
        }
        else
        {
            audioSource = GetFormPool(audio2D_Prefab, spawnPosition, oneShot, clip.length);
        }

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();

        return audioSource;
    }

    AudioSource GetFormPool(AudioSource audioSource, Vector3 spawnPosition, bool oneShot, float clipLength)
    {
        AudioSource poolObj = audioPool.Peek();

        if (poolObj != null)
        {
            audioPool.Dequeue();
            poolObj = audioSource;
            poolObj.transform.SetParent(null);
            poolObj.transform.position = spawnPosition;
            poolObj.gameObject.SetActive(true);
        }
        else
        {
            poolObj = Instantiate(audioSource, spawnPosition, Quaternion.identity);
        }

        if (oneShot == true)
            StartCoroutine(returnToPool(poolObj, clipLength));

        return poolObj;
        
    }

    IEnumerator returnToPool(AudioSource audioSource, float time)
    {
        yield return new WaitForSeconds(time);

        returnToPool(audioSource);
    }

    public void returnToPool(AudioSource audioSource)
    {
        if (audioPool.Count < poolLimit)
        {
            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
            audioSource.transform.SetParent(gameObject.transform, false);
            audioPool.Enqueue(audioSource);
        }
        else
            Destroy(audioSource);
    }
}
