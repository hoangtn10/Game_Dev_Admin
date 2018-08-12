using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour {
    //Add waypoints
    public Vector3[] localWaypoints;
    public Vector3[] globalWaypoints;

    //Making this class a Singleton
    public static PlatformManager Instance = null;

    [SerializeField]
    GameObject FallingPlatformPrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

    }
    // Use this for initialization
    void Start()
    {
        //Convert the local waypoints found in the editor into global waypoints
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
            Instantiate(FallingPlatformPrefab, globalWaypoints[i], FallingPlatformPrefab.transform.rotation);
        }
    }


    IEnumerator SpawnPlatform(Vector3 spawnPosition)
    {
        yield return new WaitForSeconds(3f);
        Instantiate(FallingPlatformPrefab, spawnPosition, FallingPlatformPrefab.transform.rotation);
    }

    //Draws the the waypoint in the editor for a moving platform
    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.yellow;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
