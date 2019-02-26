using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]

public class CollisionDetection : MonoBehaviour {

    public AudioClip[] crashSounds;
    public AudioSource audio;

    private void OnCollisionEnter(Collision collision) {
        audio.clip = crashSounds[Random.Range(0, crashSounds.Length)];
        audio.Play();
    }
}
