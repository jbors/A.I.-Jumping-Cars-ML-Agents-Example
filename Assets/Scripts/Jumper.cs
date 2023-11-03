using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class Jumper : Agent
{
    [SerializeField] private float jumpForce;
    [SerializeField] private KeyCode jumpKey;
    
    private bool jumpIsReady = true;
    private Rigidbody rBody;
    private Vector3 startingPosition;
    private int score = 0;
    public event Action OnReset;
    
    
    private void Jump()
    {
        if (jumpIsReady)
        {
            rBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.VelocityChange);
            jumpIsReady = false;
        }
    }

    private void Reset()
    {
        score = 0;
        jumpIsReady = true;
        
        //Reset Movement and Position
        transform.position = startingPosition;
        rBody.velocity = Vector3.zero;
        
        OnReset?.Invoke();
    }

    private void OnCollisionEnter(Collision collidedObj)
    {
        
        if (collidedObj.gameObject.CompareTag("Street")){
            jumpIsReady = true;
        }
        
        else if (collidedObj.gameObject.CompareTag("Mover") || collidedObj.gameObject.CompareTag("DoubleMover")){
            AddReward(-1.0f);
            Reset();
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider collidedObj)
    {
        if (collidedObj.gameObject.CompareTag("score"))
        {
            AddReward(0.05f);
            score++;
            ScoreCollector.Instance.AddScore(score);
            if(score > 100){
                AddReward(1f);
                EndEpisode();
            }
        }
    }

    public override void Initialize(){
        rBody = GetComponent<Rigidbody>();
        startingPosition = transform.position;
    }

    public override void OnActionReceived(ActionBuffers actions){
        var Actions = actions.DiscreteActions;
        if (Mathf.FloorToInt(Actions[0]) == 1){
            Jump();  
        }  
    }

    public override void OnEpisodeBegin(){

    }

    public override void Heuristic(in ActionBuffers actionsOut){
        var Actions = actionsOut.DiscreteActions;

        Actions[0] = 0;

        if (Input.GetKey(jumpKey)){
            Actions[0] = 1;  
        }
    }

    private void FixedUpdate()  
    {  
        if(jumpIsReady){     
            RequestDecision();  
        }
    }
}
