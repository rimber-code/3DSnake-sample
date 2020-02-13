//
//  CameraClass.cs
//
//  Author:
//       Rimber <r1mb3r@gmail.com>
//
//  Copyright (c) 2015 Rimber
using UnityEngine;
using System.Collections;

public class CameraClass : MonoBehaviour
{
	[SerializeField] private float m_LookDistance;
	[SerializeField] private float m_TurnSpeed;

	private Vector3 m_StartRotation;

	private void Awake()
	{
		m_StartRotation = transform.forward;
	}

	// Camera reset function turns camera to its original state.
	// Returns coroutine which can be used to make cool effect on game over.
	public Coroutine ResetCamera()
	{
		float angle = Vector3.Angle(m_StartRotation, transform.forward);
		
		// Add check to see which side we should turn to.
		if(Vector3.Angle(m_StartRotation, transform.right) > Vector3.Angle(m_StartRotation, -transform.right))
			angle *= -1f;
		
		return StartCoroutine(StartRotateCamera(0.5f, angle));
	}

	// Rotate camera along its axis.
	public void RotateCamera(float angle)
	{
		StartCoroutine(StartRotateCamera(m_TurnSpeed, angle));
	}
	
	// Coroutine to rotate the camera. Game will be paused during rotation.
	private IEnumerator StartRotateCamera(float time, float angle)
	{
		// If game is running, pause it so that the snake won't escape.
		if(CoreClass.State == GameStateType.Running)
			CoreClass.Pause();
			
		float maxtime = time;
		Vector3 forward = transform.forward;
		Vector3 target = Quaternion.Euler(angle * Vector3.forward) * forward;
		
		// Rotate the camera for a period of time.
		while(time >= 0f)
		{
			time -= Time.deltaTime;
			transform.rotation = Quaternion.LookRotation(Vector3.Lerp(target, forward, time/maxtime), Vector3.forward);
			transform.position = -transform.forward * m_LookDistance;
			yield return null;
		}
		
		// Return to the game.
		if(CoreClass.State == GameStateType.Paused)
			CoreClass.Resume();
	}
}
