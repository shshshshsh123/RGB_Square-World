using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;
    private AudioSource bgmPlayer;
    private AudioSource[] sfxPlayers;
    [SerializeField] private int channels = 10;

    public enum Bgm { test };
    public enum Sfx { test };

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitAudioManager();
        PlayBgm(Bgm.test);
    }

    private void Update()
    {
        //// 배경음 볼륨 조절
        //bgmPlayer.volume = DataManager.Instance.BGMVolume;
        //// 효과음 볼륨 조절
        //for (int index = 0; index < channels; index++)
        //{
        //    sfxPlayers[index].volume = DataManager.Instance.SFXVolume;
        //}
    }

    void InitAudioManager()
    {
        // 배경음 초기화
        GameObject bgmObject = new GameObject("Bgm");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;  // 배경음은 자동재생 안함
        bgmPlayer.loop = true;  // 배경음은 무한재생
        //bgmPlayer.volume = DataManager.Instance.BGMVolume;  // 배경음 볼륨 초기화

        // 효과음 초기화
        GameObject sfxObject = new GameObject("Sfx");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels]; // 오디오소스 개수만큼
        for (int index = 0; index < channels; index++)  // 채널개수만큼 반복
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].loop = false;
            //sfxPlayers[index].volume = DataManager.Instance.SFXVolume;  // 효과음 볼륨 초기화
        }
    }

    public void PlayBgm(Bgm bgm)
    {
        bgmPlayer.Stop();
        bgmPlayer.clip = bgmClips[(int)bgm];
        bgmPlayer.Play();
    }

    public void PlaySfx(Sfx sfx)
    {
        // 빈 채널을 찾아서 소리 재생
        for (int index = 0; index < channels; index++)
        {
            if (sfxPlayers[index].isPlaying) continue;  // 현재 재생중이면 다음 채널로 넘어감
            else
            {
                // 빈 채널에 소리 재생
                sfxPlayers[index].clip = sfxClips[(int)sfx];
                sfxPlayers[index].Play();
                break;
            }
        }
    }
}
