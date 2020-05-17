using Peque;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;

public class Player : MonoBehaviour {

    public static Player Instance;
    public CameraControl cameraController;

    [HideInInspector]
    public Vehicle currentVehicle {
        get {
            return _currentVehicle;
        }
        set {
            if (value == null) {
                Player.Instance.lookToCharacter();
                Player.Instance.transform.SetParent(null);
                Player.Instance.unfreeze();
                animator.SetBool("IsDriving", false);
                capsuleCollider.enabled = true;
            } else {
                Player.Instance.freeze();
                Player.Instance.cameraController.lookAt = nearestCar.cameraPosition;
                animator.SetBool("IsDriving", true);
                capsuleCollider.enabled = false;
            }
            _currentVehicle = value;
        }
    }
    private Vehicle _currentVehicle;
    public bool insideCar {
        get {
            return currentVehicle != null;
        }
    }

    Transform headPosition;
    WheelVehicle nearestCar;
    UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl movementController;
    private Animator animator;
    private CapsuleCollider capsuleCollider;

    private void Awake() {
        Instance = this;
        headPosition = cameraController.lookAt;
        movementController = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        if (Input.GetButtonDown("EnterVehicle")) {
            if (insideCar) {
                currentVehicle.exit();
            } else if (nearestCar != null) {
                nearestCar.GetComponent<Peque.Vehicle>().enter();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void lookToCharacter () {
        cameraController.lookAt = headPosition;
    }

    public void unfreeze () {
        GetComponent<Rigidbody>().isKinematic = false;
        movementController.start();
    }
    public void freeze () {
        GetComponent<Rigidbody>().isKinematic = true;
        movementController.stop();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.name == "EnterCarArea" && !insideCar) {
            nearestCar = other.transform.parent.GetComponent<WheelVehicle>();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.name == "EnterCarArea" && !insideCar) {
            nearestCar = null;
        }
    }
}
