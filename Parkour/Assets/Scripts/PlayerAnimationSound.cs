using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationSound : MonoBehaviour
{
    public void StepSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Step, transform.position);
    }

    public void WallRunHSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.WallRunH, transform.position);
    }

    public void WallRunVSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.WallRunV, transform.position + Vector3.up * 1.5f + transform.forward * 0.5f);
    }

    public void WallClimbSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.WallClimb, transform.position + Vector3.up * 1.5f + transform.forward * 0.5f);
    }

    public void VaultSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Vault, transform.position + Vector3.up);
    }

    public void SlideSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Slide, transform.position);
    }

    public void RollSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Roll, transform.position);
    }

    public void LandSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.Land, transform.position);
    }

    public void HardSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.eSound.HardLand, transform.position);
    }
}
