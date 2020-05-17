using Peque.Traffic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Peque
{
    public class Vehicle : MonoBehaviour
    {
        public Transform[] enterPositions;
        public Transform[] seatPositions;
        public bool isParked = false;

        private VehicleBehaviour.WheelVehicle vehicleController;
        private List<int> takenPositions = new List<int>();
        private VehicleNavigation vehicleNavigation;

        private void Awake() {
            vehicleController = GetComponent<VehicleBehaviour.WheelVehicle>();
            vehicleNavigation = GetComponent<VehicleNavigation>();

            if (isParked) {
                vehicleNavigation.enabled = false;
            }
        }

        public void enter() {
            // look for available seat
            Transform seat = null;
            int pos = 0;
            for (pos = 0; pos < seatPositions.Length; ++pos) {
                if (takenPositions.Contains(pos)) {
                    continue;
                }

                seat = seatPositions[pos];
                break;
            }

            if (!seat) {
                return;
            }
            vehicleNavigation.enabled = false;

            takenPositions.Add(pos);

            Player.Instance.transform.position = seat.position;
            Player.Instance.transform.SetParent(seat);
            Player.Instance.transform.rotation = seat.rotation;

            Player.Instance.currentVehicle = this;
            vehicleController.IsPlayer = true;
            vehicleController.Handbrake = false;
        }

        public void exit() {
            vehicleController.IsPlayer = false;
            vehicleController.Handbrake = false;
            
            Player.Instance.currentVehicle = null;
            takenPositions.Remove(0);
        }
    }
}