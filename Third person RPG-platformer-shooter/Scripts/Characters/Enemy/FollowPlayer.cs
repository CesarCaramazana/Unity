using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class FollowPlayer : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float enemySpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float minPlayerDistance;

    [SerializeField] private GameObject enemyVisuals;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowardsPlayer();
        
    }


    private void MoveTowardsPlayer()
    {

        Vector3 enemyMoveDirection = (transform.position - Player.Instance.transform.position);
        enemyMoveDirection.y = 0f;

        float distanceToPlayer = enemyMoveDirection.magnitude;

        if (distanceToPlayer >= minPlayerDistance)
        {
            //Move towards player
            transform.position = Vector3.Scale(Vector3.MoveTowards(transform.position, Player.Instance.transform.position, enemySpeed * Time.deltaTime), new Vector3(1,0,1));
        }        

        //Rotate towards player (only the visuals so the orbiting point remains unchanged)
        enemyVisuals.transform.forward = Vector3.Slerp(enemyVisuals.transform.forward, enemyMoveDirection, rotateSpeed * Time.deltaTime);

    }
}
