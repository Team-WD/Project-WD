using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public abstract class MultiWeaponBase : NetworkBehaviour
{
    // PUBLIC MEMBERS

    public Transform FireTransform => _fireTransform;

    // PRIVATE MEMBERS

    [SerializeField]
    private Transform _fireTransform;
    [SerializeField]
    private AudioClip _fireClip;
    [SerializeField]
    private Transform _fireSoundSourcesRoot;

    private AudioSource[] _fireSoundSources;

    // PUBLIC METHODS

    public abstract void Fire();

    // PROTECTED METHODS

    protected void PlayFireEffect()
    {
        // In multipeer mode fire sounds are played only for visible runner
        if (Runner.GetVisible() == false)
            return;

        if (_fireSoundSources == null)
        {
            _fireSoundSources = _fireSoundSourcesRoot.GetComponentsInChildren<AudioSource>();
        }

        // Find free audio source and play fire sound
        for (int i = 0; i < _fireSoundSources.Length; i++)
        {
            var source = _fireSoundSources[i];

            if (source.isPlaying == true)
                continue;

            source.clip = _fireClip;
            source.Play();
            return;
        }

        Debug.LogWarning("No free fire sound source", gameObject);
    }
}
