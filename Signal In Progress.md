<h1> Signal In Progress </h1>
    <img src="Images/Signal in progress(In game menu).png" >
    <h2> <em> Game description </em> </h2>
        <p> 
            Signal In Progress is horror game about surviving a monster attack by keeping out of the room and calling for help.
        </p>
        <br>
    <h2> <em> My Responsibilities </em> </h2>
        <p>
            On the first day we divid up the work and thing we needed to be done to get a playable game.
            We desided early in the project that Audio was important for our game. Since i had some experience with it from Pogo Pirates nobody minded me being responsebul for it. <br>
            Since I was working on a Audio Mixer, I also took on making the Main and Settings Menu. <br>
        </p>
    <h2> <em> Audio System </em> </h2>
        <p> 
            I dicided early to try to make it easy to use Audio Manager that was easy to use. After some prototypes, some advice and wishes from my team the result was a Audio Manager that impluments: <br>
            - Audio dictionary where you stores audio clips and give that clip a key. <br>
            - Funtions that easy to use.
            - Object pooling for audio sources that reuses them as needed. <br>
        </p>
    <details>
        <summary><em> Code: Audio Manager. </em></summary>
  
```csharp
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

    private AudioSource GetPrefab(AudioType audioSourcePrefab)
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

    /// <summary>
    /// Method that spawns in a audio_Prefab and Play clip from audioDictionary.
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
        AudioClip clip = GetAudioClip(audioDictionaryKey);
        AudioSource audioSource = GetFormPool(GetPrefab(audioSourcePrefab), spawnPosition, false, clip.length);
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
    public AudioSource PlayAudioClip(Vector3 spawnPosition, string audioDictionaryKey, AudioType audioSourcePrefab, bool oneShot = true, float volume = 1, float pitch = 1)
    {
        AudioClip clip = GetAudioClip(audioDictionaryKey);
        AudioSource audioSource = GetFormPool(GetPrefab(audioSourcePrefab), spawnPosition, oneShot, clip.length);
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

<h2> <em> Main Menu </em> </h2>
        <p> 
            When it comes to UI it is something i do enjoy working with and I never had to setup a option menu was a intresting experiance. <br>
             <br>
            -  <br>
            -  <br>
        </p>
<details>
