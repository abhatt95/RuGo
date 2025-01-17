﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonGadget : Gadget
{
    public GameObject CannonBallPrefab;
    public float FireForce = 1.0f;

    private LineRenderer mTrajectory;
    private Transform mHeading;
    private Transform mBarrelTip;
    private Transform mBarrel;

    private float mCannonBallMass = 0.5f;

    private AudioSource mAudioData;
    private ParticleSystem mParticleSystem;

    new void Awake()
    {
        base.Awake();
        mBarrel = this.transform.Find("Wooden_pillow");
        mHeading = mBarrel.Find("Heading");
        mBarrelTip = mBarrel.Find("BarrelTip");

        mTrajectory = mBarrel.gameObject.GetComponent<LineRenderer>();
        if (mTrajectory == null) 
        {
            mTrajectory = mBarrel.gameObject.AddComponent<LineRenderer>();
            mTrajectory.enabled = false;
        }

        mTrajectory.material = new Material(Shader.Find("Unlit/Texture"));
        mTrajectory.startColor = Color.white;
        mTrajectory.endColor = Color.white;
        mTrajectory.startWidth = 0.01f;
        mTrajectory.endWidth = 0.01f;
       
        mAudioData = GetComponent<AudioSource>();
        mParticleSystem = mBarrelTip.GetChild(0).GetComponent<ParticleSystem>();

        mCannonBallMass = CannonBallPrefab.GetComponent<Rigidbody>().mass;
    }

    public override void PerformSwitchAction()
    {
        FireCannon();
    }

    new void Update()
    {
        base.Update();

        if (mTrajectory != null && mTrajectory.enabled) 
        {
            PlotTrajectory();   
        }
        if (Input.GetKeyDown(KeyCode.F) && this.GetPhysicsMode())
        {
            FireCannon(); 
        }
    }

    public override GadgetInventory GetGadgetType()
    {
        return GadgetInventory.Cannon;
    }

    private void PlotTrajectory()
    {
        Vector3 start = mBarrelTip.position;

        if (mTrajectory.positionCount == 0 || mTrajectory.GetPosition(0) != start)
        {
            List<Vector3> trajectory_points = new List<Vector3>();
            
            Vector3 initialVelocity = mBarrelTip.forward * FireForce / mCannonBallMass;
            
            Vector3 prev = start;
            int i;
            for (i = 0; i < 60; i++) {
                trajectory_points.Add(prev);
                float t = 0.01f * i;

                Vector3 pos = start + initialVelocity * t + Physics.gravity * t * t * 0.5f;
                
                if (!Physics.Linecast(prev,pos))
                {
                    prev = pos;
                } 
            }

            mTrajectory.positionCount = i;
            for (int j = 0; j < i; j++) 
            {
                mTrajectory.SetPosition(j, trajectory_points[j]);
            }
        }
    }

    public override void MakeSolid()
    {
        base.MakeSolid();
        mTrajectory.enabled = false;
    }

    public override void MakeTransparent(bool keepCollision = false)
    {
        base.MakeTransparent(keepCollision);
        if(mTrajectory != null)
        {
            if (CurrentGadgetState == GadgetState.InWorld || CurrentGadgetState == GadgetState.FirstPlacement)
            {
                mTrajectory.enabled = true;
            }
            else
            {
                mTrajectory.enabled = false;
            }
        }
    }

    private void FireCannon() 
    {
        mAudioData.Play(0);
        mParticleSystem.Play(true);

        GameObject cannonBall = Instantiate(CannonBallPrefab, mBarrelTip);
        cannonBall.transform.localPosition = mBarrelTip.localPosition;
        Rigidbody rigidBody = cannonBall.GetComponent<Rigidbody>();
        
        Vector3 barrelDirection = mBarrelTip.forward * FireForce;
        rigidBody.AddForce(barrelDirection, ForceMode.Impulse);

        IEnumerator coroutine = CleanCannon(cannonBall);
        StartCoroutine(coroutine);
    }

    private IEnumerator CleanCannon(GameObject cannonBall) 
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(cannonBall);
    }
}
