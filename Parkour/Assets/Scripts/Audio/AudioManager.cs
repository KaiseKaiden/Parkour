using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource myBackgroundMusic;


    [SerializeField] AudioClip[] myStepSFX;
    [SerializeField] AudioClip[] myBreathSFX;
    [SerializeField] AudioClip[] myJumpSFX;
    [SerializeField] AudioClip[] myLandSFX;
    [SerializeField] AudioClip[] myRollSFX;
    [SerializeField] AudioClip[] myHardLandSFX;
    [SerializeField] AudioClip[] mySlideSFX;
    [SerializeField] AudioClip[] myHitSFX;
    [SerializeField] AudioClip[] myKickBoostSFX;
    [SerializeField] AudioClip[] myVaultSFX;
    [SerializeField] AudioClip[] myWallRunVSFX;
    [SerializeField] AudioClip[] myWallRunHSFX;
    [SerializeField] AudioClip[] myWallClimbSFX;



    [SerializeField] GameObject myAudioPrefab;

    public enum eSound
    {
        Step,
        Breath,
        Jump,
        Land,
        Roll,
        HardLand,
        Slide,
        Hit,
        KickBoost,
        Vault,
        WallRunV,
        WallRunH,
        WallClimb
    }

    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("AudioManager").Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Instance = this;
    }

    private void Start()
    {
        myBackgroundMusic = GetComponent<AudioSource>();
    }

    public void PlaySound(eSound aSound)
    {
        // Create Sound
        GameObject audio = Instantiate(myAudioPrefab);

        AudioSource audioSource = audio.GetComponent<AudioSource>();
        audioSource.clip = GetAudioClip(aSound);
        //audioSource.volume = (float)PlayerPrefs.GetInt("vSound") / 10f;
        audioSource.Play();
    }

    public void PlaySound(eSound aSound, Vector3 aPosition)
    {
        // Create Sound
        GameObject audio = Instantiate(myAudioPrefab);
        audio.transform.position = aPosition;

        AudioSource audioSource = audio.GetComponent<AudioSource>();
        audioSource.clip = GetAudioClip(aSound);
        audioSource.spatialBlend = 1.0f;
        //audioSource.volume = (float)PlayerPrefs.GetInt("vSound") / 10f;
        audioSource.Play();
    }

    AudioClip GetAudioClip(eSound aSound)
    {
        switch (aSound)
        {
            case eSound.Step:
                {
                    return myStepSFX[Random.Range(0, myStepSFX.Length)];
                }
            case eSound.Breath:
                {
                    return myBreathSFX[Random.Range(0, myBreathSFX.Length)];
                }
            case eSound.Jump:
                {
                    return myJumpSFX[Random.Range(0, myJumpSFX.Length)];
                }
            case eSound.Land:
                {
                    return myLandSFX[Random.Range(0, myLandSFX.Length)];
                }
            case eSound.Roll:
                {
                    return myRollSFX[Random.Range(0, myRollSFX.Length)];
                }
            case eSound.HardLand:
                {
                    return myHardLandSFX[Random.Range(0, myHardLandSFX.Length)];
                }
            case eSound.Slide:
                {
                    return mySlideSFX[Random.Range(0, mySlideSFX.Length)];
                }
            case eSound.Hit:
                {
                    return myHitSFX[Random.Range(0, myHitSFX.Length)];
                }
            case eSound.KickBoost:
                {
                    return myKickBoostSFX[Random.Range(0, myKickBoostSFX.Length)];
                }
            case eSound.Vault:
                {
                    return myVaultSFX[Random.Range(0, myVaultSFX.Length)];
                }
            case eSound.WallRunV:
                {
                    return myWallRunVSFX[Random.Range(0, myWallRunVSFX.Length)];
                }
            case eSound.WallRunH:
                {
                    return myWallRunHSFX[Random.Range(0, myWallRunHSFX.Length)];
                }
            case eSound.WallClimb:
                {
                    return myWallClimbSFX[Random.Range(0, myWallClimbSFX.Length)];
                }
        }

        return null;
    }
}
