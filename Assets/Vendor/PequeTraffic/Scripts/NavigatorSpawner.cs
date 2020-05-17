using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Peque.Traffic
{ 
    public class NavigatorSpawner : MonoBehaviour
    {
        public enum Direction
        {
            Both = 2,
            Normal = 0,
            Inverse = 1,
        }
        public GameObject[] prefabs;
        public int numberToSpawn = 5;
        [Tooltip("Number of attempts that spawner will try to instantiate the requested amount of prefabs.")]
        public int maxAttempts = 10;
        public Direction allowedDirection = Direction.Both;
        [Tooltip("Waypoint gameobjects will be removed at runtime to improve fps")]
        public bool optimizeOnRuntime = true;
        [Tooltip("Waypoint gameobjects removal at runtime will also be done in Editor")]
        public bool optimizeOnEditorToo = true;

        private List<WaypointData> waypoints;
        private List<WaypointNavigator> navigators;

        void Start() {
            getChildWaypoints();

            if (numberToSpawn > 0 && waypoints.Count > 0) {
                StartCoroutine(spawn());
                InvokeRepeating(nameof(manageNavigators), 5, 0.5f);
            }
        }

        private void getChildWaypoints() {
            waypoints = new List<WaypointData>();

            foreach (Waypoint waypoint in transform.GetComponentsInChildren<Waypoint>()) {
                waypoints.Add(waypoint.data);
            }
        }

        IEnumerator spawn () {
            int count = 0;
            int attempts = 0;
            navigators = new List<WaypointNavigator>();

            while (count < numberToSpawn) {
                WaypointData randomWaypoint = getRandomWaypoint();

                // seems like there are no available slots
                if (randomWaypoint == null) {
                    //Debug.Log("No available slots found for " + transform.name + " waiting a second");
                    attempts++;

                    if (attempts == maxAttempts) {
                        //Debug.Log("No available slots found for " + transform.name + ", stopping spawner.");
                        break;
                    }

                    yield return new WaitForSeconds(1);
                    continue;
                }
				

                GameObject obj = Instantiate(prefabs[Random.Range(0, prefabs.Length)]);

                setupNavigator(obj.GetComponent<WaypointNavigator>(), randomWaypoint);

                navigators.Add(obj.GetComponent<WaypointNavigator>());

                yield return new WaitForEndOfFrame();
                count++;
            }

            if (optimizeOnRuntime && (!Application.isEditor || (Application.isEditor && optimizeOnEditorToo))) {
                removeWaypointsGameobjects();
            }
        }

        void setupNavigator (WaypointNavigator navigator, WaypointData waypoint) {
            if (waypoint == null) {
                return;
            }

            // make sure this waypoint isn't reused for spawning more entities
            waypoint.occupied = true;

            Vector3 spawnPosition = waypoint.centerPosition;
            spawnPosition.y += 0.5f;

            navigator.gameObject.transform.position = spawnPosition;

            // Point spawned entities looking at their next waypoint
            Vector3 lookPos = spawnPosition;
            if (waypoint.nextWaypoint != null) {
                lookPos = waypoint.nextWaypoint.centerPosition;
            } else if (waypoint.previousWaypoint != null) {
                lookPos = waypoint.previousWaypoint.centerPosition;
            }

            lookPos.y = navigator.gameObject.transform.position.y;
            navigator.gameObject.transform.LookAt(lookPos);


            int direction;

            if (allowedDirection == Direction.Both) {
                direction = Mathf.RoundToInt(Random.Range(0f, 1f));
            } else {
                direction = (int)allowedDirection;
            }

            navigator.init(direction, waypoint);
        }

        void removeWaypointsGameobjects () {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
        }

        void manageNavigators() {
            Rigidbody rigid;
            VehicleNavigation vehicle;

            foreach (WaypointNavigator navigator in navigators) {
                rigid = navigator.GetComponent<Rigidbody>();

                // looks like went under the map
                if (rigid && rigid.velocity.y < -100) {
                    setupNavigator(navigator, getWaypointNearTarget(Player.Instance.transform));
                    continue;
                }

                if (Player.Instance != null && Vector3.Distance(navigator.transform.position, Player.Instance.transform.position) > 85f) {
                    vehicle = navigator.GetComponent<VehicleNavigation>();
                    // make cars work again whenever player leaves the "scene"
                    if (vehicle != null && vehicle.isDead) {
                        vehicle.isDead = false;
                    }

                    setupNavigator(navigator, getWaypointNearTarget(Player.Instance.transform));
                    continue;
                }
            }
        }

        WaypointData getRandomWaypoint (int attempt = 0) {
            WaypointData waypoint = waypoints[Random.Range(0, waypoints.Count - 1)];

            /* dont spawn on waypoints:
            * - occupied
            * - that have branches (to avoid intersecting existing traffic
            */
            if (waypoint.occupied || waypoint.branches.Count > 0 || (waypoint.nextWaypoint != null && waypoint.nextWaypoint.occupied) || (waypoint.previousWaypoint != null && waypoint.previousWaypoint.occupied) ||
                (waypoint.nextWaypoint != null && waypoint.nextWaypoint.branches.Count > 0) ||
                (waypoint.previousWaypoint != null && waypoint.previousWaypoint.branches.Count > 0)
                ) {
                attempt++;

                if (attempt == maxAttempts) {
                    return null;
                }

                return getRandomWaypoint(attempt);
            }

            return waypoint;
        }

        WaypointData getWaypointNearTarget (Transform target, float minDistance = 50, float maxDistance = 70, int attempt = 0) {

            if (attempt == maxAttempts) {
                return null;
            }

            WaypointData waypoint = waypoints[Random.Range(0, waypoints.Count - 1)];

            /* dont spawn on waypoints:
            * - occupied
            * - that have branches (to avoid intersecting existing traffic
            */
            if (waypoint.occupied || waypoint.branches.Count > 0 || (waypoint.nextWaypoint != null && waypoint.nextWaypoint.occupied) || (waypoint.previousWaypoint != null && waypoint.previousWaypoint.occupied) ||
                (waypoint.nextWaypoint != null && waypoint.nextWaypoint.branches.Count > 0) ||
                (waypoint.previousWaypoint != null && waypoint.previousWaypoint.branches.Count > 0)
                ) {
                attempt++;
                return getWaypointNearTarget(target, minDistance, maxDistance, attempt);
            }

            float distance = Vector3.Distance(target.position, waypoint.centerPosition);

            if (distance < minDistance || distance > maxDistance) {
                attempt++;
                return getWaypointNearTarget(target, minDistance, maxDistance, attempt);
            }
            return waypoint;
        }
    }
}