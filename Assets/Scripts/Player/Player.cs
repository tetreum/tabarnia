using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;

public class Player : MonoBehaviour {

    public CameraControl cameraController;


    Transform headPosition;
    WheelVehicle nearestCar;
    UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl movementController;

    private void Awake() {
        headPosition = cameraController.lookAt;
        movementController = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        if (Input.GetButtonDown("EnterVehicle") && nearestCar != null) {
            nearestCar.IsPlayer = true;
            nearestCar.Handbrake = false;
            movementController.stop();
            
            cameraController.lookAt = nearestCar.cameraPosition;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.name == "EnterCarArea") {
            nearestCar = other.transform.parent.GetComponent<WheelVehicle>();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.name == "EnterCarArea") {
            nearestCar = null;
        }
    }
}
