<h1> Signal In Progress </h1>
    <img src="Images/Signal In Progress Menu.png"
    <h3> >>> <a href="https://github.com/Erik2333/Resume_ErikBerglund/blob/main/README.md"> Back to Main Page </a> <<<  <h3> 
    <h2> <em> Game description </em> </h2>
        <p> 
            Signal In Progress is horror game about surviving a monster attack by keeping out of the room and calling for help.
        </p>
        <br>
    <h2> <em> My Responsibilities </em> </h2>
        <p>
            On the first day we divided up the work and thing we needed to be done to get a playable game.
            We decided early in the project that Audio was important for our game. Since I had some experience with it from Pogo Pirates nobody minded me being responsible for it.
        </p>
        <p> I also took on making the Main Menu and Settings since i needed to setup an audio mixer. </p>
        <p> Also helped out with testing, finding bugs, identify the root of the problem. After that, I'll talk to the person how made the bug if he wanted me to try to fix it. </p>
    <h2> <em> Audio System </em> </h2>
        <p> 
            I decided early to try to make it easy to use Audio Manager that was easy to use. After some prototypes, some advice and wishes from my team the result was an Audio Manager that implements: <br>
            <ul>
            <li> Audio dictionary where you store audio clips and give that clip a key use to fetch the clip in code </li>
            <li> Functions that easy to use and gives Audio Sources where sound need to be played. </li>
            <li> Object pooling that reuse inactive Audio Sources when needed. </li>
            </ul>
        </p> <br>
        <img src="Images/AudioManager.png" >
        <img src="Images/AudioExample.png" >
    <details>
        <summary><h3><em> Code: Audio Manager. </em></h3></summary>
  
```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioType
    {
        Audio2D,
        AudioMaster,
        AudioMusic,
        AudioSFX,
        AudioAmbience
    }

    public static AudioManager Instance { get; private set; }
    private readonly Queue<AudioSource> audioPool = new();
    private byte poolLimit = 20;
    public AudioSource audio2D_Prefab, audio3D_Master, audio3D_Music, audio3D_SFX, audio3D_Ambience;
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
    
    public AudioClip GetAudioClip(string audioDictionaryKey)
    {
        audioDictionary.ToDictionary().TryGetValue(audioDictionaryKey.ToLower(), out AudioClip audioClip);

        if (audioClip != null)
        {
            return audioClip;
        }
        else
        {
            Debug.LogError($"No clip found with Key: {audioDictionaryKey}. Did you add it to the Audio Manager?");
            return errorClip;
        }
    }

    AudioSource GetPrefab(AudioType audioSourcePrefab)
    {
        switch (audioSourcePrefab)
        {
            case AudioType.Audio2D: return audio2D_Prefab;
            case AudioType.AudioMaster: return audio3D_Master;
            case AudioType.AudioMusic: return audio3D_Music;
            case AudioType.AudioSFX: return audio3D_SFX;
            case AudioType.AudioAmbience: return audio3D_Ambience;
            default:
                Debug.LogError($"audioSourcePrefab not found. Default to {audio3D_Master}");
                return audio3D_Master;
        }
    }

    public void PlayAudioClip(Vector3 spawnPosition, string audioDictionaryKey, AudioType audioSourcePrefab = AudioType.AudioSFX, float volume = 1, float pitch = 1)
    {
        AudioClip clip = GetAudioClip(audioDictionaryKey);
        AudioSource audioSource = GetFormPool(GetPrefab(audioSourcePrefab), spawnPosition, true, clip.length);
        audioSource.clip = clip;
        audioSource.name += $" ({audioDictionaryKey})";
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }
    
    public AudioSource GetAudioSource(Vector3 spawnPosition, string audioDictionaryKey, AudioType audioSourcePrefab = AudioType.AudioSFX, float volume = 1, float pitch = 1)
    {
        AudioClip clip = GetAudioClip(audioDictionaryKey);
        AudioSource audioSource = GetFormPool(GetPrefab(audioSourcePrefab), spawnPosition, false, clip.length);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        return audioSource;
    }

    AudioSource GetFormPool(AudioSource audioSource, Vector3 spawnPosition, bool oneShot, float clipLength)
    {
        AudioSource poolObj;


        if (audioPool.Count != 0)
        {
            poolObj = audioPool.Dequeue();
            poolObj.name = audioSource.name;
            poolObj.transform.SetParent(null);
            poolObj.transform.position = spawnPosition;
            poolObj.gameObject.SetActive(true);
        }
        else
        {
            poolObj = Instantiate(audioSource, spawnPosition, Quaternion.identity);
            poolObj.name = audioSource.name;
        }

        if (oneShot == true)
            StartCoroutine(ReturnToPool(poolObj, clipLength));

        return poolObj;
        
    }

    IEnumerator ReturnToPool(AudioSource audioSource, float time)
    {
        yield return new WaitForSeconds(time);

        ReturnAudio(audioSource);
    }

    public void ReturnAudio(AudioSource audioSource)
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

[Serializable]
public class AudioDictionary
{
    [SerializeField]
    DictionaryItem[] dictionary;

    public Dictionary<string, AudioClip> ToDictionary()
    {
        Dictionary<string, AudioClip> newDict = new();

        foreach (DictionaryItem item in dictionary)
        {
            newDict.Add(item.key.ToLower(), item.audioClip);
        }

        return newDict;
    }
}

[Serializable]
public class DictionaryItem
{
    [SerializeField]
    public string key;

    [SerializeField]
    public AudioClip audioClip;
}

```

</details>

<h2> <em> Menu And Settings </em> </h2>
        <p> 
            When it comes to UI it is something i do enjoy working with and I never had made an option menu before was a intresting experiance. <br>
            <br>
            I starded by researching what common functionalities menu needs and then deciding what we needed to implements. <br>
            Then made some UI prefabs so I could quickly update them if required, I didn't have to change one piece at the time. <br>
            <br>
            When it comes to settings, I did encounter some interesting logic problems and some bugs reinforced why testing build versions of the project is so important. <br>
            <br>
            Overall I did enjoy working with UI and hope to work more with it in the future.
        </p>
<img width="800" height="600" src="Images/LongVideo-ezgif.com-video-to-gif-converter.gif" style="max-width: 100%; display: inline-block;" data-target="animated-image.originalImage">

