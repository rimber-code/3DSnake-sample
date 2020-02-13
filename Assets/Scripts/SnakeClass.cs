//
//  SnakeClass.cs
//
//  Author:
//       Rimber <r1mb3r@gmail.com>
//
//  Copyright (c) 2015 Rimber
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeClass : MonoBehaviour
{
	[SerializeField] private Transform m_BodyPartPrefab; // Body part prefab.
	[Header("Move frequency")]
	[SerializeField] private float m_UpdateFrequency; // Move frequency.
    [SerializeField] private float m_RotationSpeed;

    private List<CellData> m_Body; // List of body parts.
	private PointData m_Position; // Current snake position.
	private PointData m_Direction; // Snakes heading direction.
	
	public PointData Position { get { return m_Position; } }
	
	void Awake()
	{
		m_Body = new List<CellData>();
		m_Direction = new PointData(0, 0);
		
		// Initial direction is up.
		MoveUp();
		
		// Start update coroutine that makes the snake to move.
		StartCoroutine(StartUpdate());
	}
	
	void OnDestroy()
	{
		StopAllCoroutines();
	}
	
	public bool OccupiesPosition(int face, PointData point)
	{
		for(int i = 0; i < m_Body.Count; i++)
		{
			if(m_Body[i].m_GridFace != face)
				continue;
							
			if(PointData.Equals(m_Body[i].m_Position, point))
				return true;
		}
		
		return false;
	}
	
	// Setup snakes initial position on the map.
	public void InitialPosition(PointData point, Vector3 position)
	{
		m_Position = point;
		transform.position = position;
	}
	
	// Update snake. Snake will move in intervals rather than with
	// a constant speed.
	private IEnumerator StartUpdate()
	{
		float countdown = m_UpdateFrequency;
		while(true)
		{
			// Game is paused.
			if(CoreClass.State == GameStateType.Paused)
			{
				yield return null;
				continue;
			}
		
			// Game has been stopped or game is over.
			if(CoreClass.State == GameStateType.None) yield break;
		
			countdown -= Time.deltaTime;
			if(countdown <= 0f)
			{
				MoveBody();
				
				// Check if there is a target at the new position.
				if(CoreClass.Instance.PointHasTarget(m_Position))
				{
					CoreClass.Instance.EatTarget();
					AddBodyPart();
				}
				
				countdown = m_UpdateFrequency;
			}
			
			yield return null;
		}
	}
	
	// Add a body part to the snakes body.
	// New part will take the snake heads position;
	private void AddBodyPart()
	{
		int face = CoreClass.Instance.CurrentFace;
		CellData part = new CellData();
		
		// Instantiate new game object.
		part.m_Object = Instantiate<Transform>(m_BodyPartPrefab);
		part.m_Object.position = transform.position;
		part.m_Position = m_Position;
		part.m_GridFace = face;
		
		// Add part to the body.
		m_Body.Add(part);
	}
	
	// Move the snake body.
	private void MoveBody()
	{
		// New position in the grid.
		PointData point = m_Position + m_Direction;
		int face = CoreClass.Instance.CurrentFace;
		
		// Crossing the border between faces.
		// Do this check before checking for the walls.
		if(CoreClass.Instance.Grid.CrossingBorder(point))
			point = CoreClass.Instance.Grid.CrossBorder(point);
		
		// If the point doesn't exist, that means we have hit a wall and game ends.
		if(!CoreClass.Instance.Grid.HasPoint(point))
		{
			// Sorry, that is a game over.
			CoreClass.Instance.GameOver("You hit a wall.");
			return;
		}
		
		// Check if there is a body part on the cell we want to move to.
		if(OccupiesPosition(CoreClass.Instance.CurrentFace, point))
		{
			// Sorry, that is a game over.
			CoreClass.Instance.GameOver("You ate your own tail.");
			return;
		}
			
		// Move the rest of the body before head.
		if(m_Body.Count > 0)
		{
			CellData data;
			// Simply copy the data from later added body parts.
			for(int i = 0; i < m_Body.Count-1; i++)
			{
				data = m_Body[i];
				// Copy the data of the next one.
				data.m_Position = m_Body[i+1].m_Position;
				data.m_GridFace = m_Body[i+1].m_GridFace;
				data.m_Object.position = CoreClass.Instance.GetPosition(data.m_GridFace, data.m_Position);//m_Body[i+1].m_Object.position;
				m_Body[i] = data;
			}
			
			// The earliest part takes the position of the head.
			data = m_Body[m_Body.Count-1];
			data.m_GridFace = face;
			data.m_Position = m_Position;
			data.m_Object.position = transform.position;
			m_Body[m_Body.Count-1] = data;
		}
	
		// Move the snake head.
		m_Position = point;
		transform.position = CoreClass.Instance.Grid.GetPosition(m_Position);
	}

    private void RotateToFaceDirection()
    {
        Vector3 direction = m_Direction * Camera.main.transform.rotation;
        Quaternion q = Quaternion.FromToRotation(this.transform.forward, direction);
        q *= this.transform.rotation;
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, q, m_RotationSpeed * Time.deltaTime);
    }

    public Coroutine DestroyBody()
	{
		return StartCoroutine(StartDestroyBody());
	}
	
	public IEnumerator StartDestroyBody()
	{
		while(m_Body.Count > 0)
		{
			Destroy(m_Body[0].m_Object.gameObject);
			m_Body.RemoveAt(0);
			yield return new WaitForSeconds(0.05f);
		}
		
		m_Body.Clear();
	}
	
	private void MoveUp()
	{
		m_Direction.X = 0;
		m_Direction.Y = 1;
	}
	
	private void MoveDown()
	{
		m_Direction.X = 0;
		m_Direction.Y = -1;
	}
	
	private void MoveLeft()
	{
		m_Direction.X = -1;
		m_Direction.Y = 0;
	}
	
	private void MoveRight()
	{
		m_Direction.X = 1;
		m_Direction.Y = 0;
	}

    // Update movement input
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveUp();
        if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveDown();
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveRight();
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLeft();

        // Do smooth rotations to face movement direction
        RotateToFaceDirection();
    }
}
