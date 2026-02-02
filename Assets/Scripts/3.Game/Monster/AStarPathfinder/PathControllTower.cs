using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathControllTower : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform target;          // Player
    [SerializeField] private AStarPathfinder pathfinder;

    [Header("target")]
    [SerializeField] private GameObject player;
    public GameObject baseCamp;

    public bool hasToPlayerPath = true;


    private void Awake()
    {
        player = GameObject.FindWithTag("Character");
    }

    private void Start()
    {
        target = player.transform;
    }

    public void HasToPlayerPath()
    {
        hasToPlayerPath = pathfinder.TryFindPath(transform.position, target.position, out Vector3[] waypoints);
        if(hasToPlayerPath)
        { 
            pathfinder.pathResult = PathResult.Success;
        }
        else if(!hasToPlayerPath)
        {
            pathfinder.pathResult = PathResult.Blocked;
        }
    }
}
