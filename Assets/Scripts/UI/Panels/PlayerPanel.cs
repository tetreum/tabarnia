using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
    public static PlayerPanel Instance;

    private void Awake() {
        Instance = this;
    }
}
