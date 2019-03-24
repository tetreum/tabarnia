using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour {

    Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    private void Start() {
        anim.SetFloat("Forward", 0.5f);
    }
}
