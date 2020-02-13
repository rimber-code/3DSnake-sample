//
//  CoreClass.cs
//
//  Author:
//       Rimber <r1mb3r@gmail.com>
//
//  Copyright (c) 2015 Rimber
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CellData
{
	public Transform m_Object;
	public int m_GridFace;
	public PointData m_Position;
};

public enum GameStateType
{
	None,
	Running,
	Paused,
}

public class CoreClass : MonoBehaviour
{
	// Make the Core a singleton.
	private static CoreClass _instance;
	public static CoreClass Instance
	{
		private set { _instance = value; }
		get
		{
			if(_instance == null)
				_instance = FindObjectOfType<CoreClass>();
				
			return _instance;
		}
	}
	
	public static GameStateType State { private set; get; }
	public static void Pause() { State = GameStateType.Paused; }
	public static void Resume() { State = GameStateType.Running; }

	[Header("Prefab")]
	[SerializeField] private SnakeClass m_Snake; // Snake prefab
	[SerializeField] private Transform m_TargetObject;
	[SerializeField] private GridClass m_GridPiece;
	[Header("Level Specifications")]
	[SerializeField] private int m_GridSize;
	[SerializeField] private float m_CellSize;
	[SerializeField] private int m_NumberOfFaces;
	[SerializeField] private PointData m_StartPoint; // Snake start position
	[Header("Camera")]
	[SerializeField] private CameraClass m_Camera;
	
	private int m_CurrentFace;
	private GridClass[] m_GameGrid;
	private CellData m_Target;
	
	public System.Action OnGameStart;
	public System.Action<string> OnGameOver;
	
	public SnakeClass Snake { get; private set; }
	public GridClass Grid { get { return m_GameGrid[m_CurrentFace]; } }
	public int CurrentFace { get { return m_CurrentFace; } }
	
	public Vector3 GetStartPosition()
	{
		return m_GameGrid[0].GetPosition(m_StartPoint);
	}
	
	public Vector3 GetPosition(int face, PointData point)
	{
		return m_GameGrid[face].GetPosition(point);
	}
	
	private void Awake()
	{
		Instance = this;
		State = GameStateType.None;
		
		// Initialize game grid.
		m_GameGrid = new GridClass[m_NumberOfFaces];
		for(int i = 0; i < m_NumberOfFaces; i++)
		{
			m_GameGrid[i] = Instantiate(m_GridPiece);
			m_GameGrid[i].Initialize(m_GridSize, m_CellSize);
			m_GameGrid[i].RotatePoints(-360f/m_GameGrid.Length*i);
			m_GameGrid[i].TranslatePoints(Mathf.CeilToInt(m_GridSize/2f)*m_CellSize);
		}
	}
	
	// Start game
	public void StartGame()
	{
		// Initialize steps.
		m_CurrentFace = 0;
		State = GameStateType.Running;
		
		// Create snake.
		Snake = Instantiate(m_Snake);
		Snake.InitialPosition(m_StartPoint, GetStartPosition());
		
		// Spawn first target.
		SpawnTarget();
		
		if(OnGameStart != null)
			OnGameStart();
	}
	
	// Game over function takes game over reason.
	public void GameOver(string reason)
	{
		Debug.LogFormat("{0}", reason);
		State = GameStateType.None;
		
		if(OnGameOver != null)
			OnGameOver(reason);
		
		StartCoroutine(StartGameOver());
	}
	
	// Game over coroutine to cleanup all the dirt from the level.
	// Just a nice visual for the player.
	private IEnumerator StartGameOver()
	{
		yield return Snake.DestroyBody();
		
		Destroy(Snake.gameObject);
		
		Destroy(m_Target.m_Object.gameObject);
		
		yield return m_Camera.ResetCamera();
	}
	
	// Chane game grid face to 'right'.
	public void IncreaseFace()
	{
		m_CurrentFace = (m_CurrentFace+1) % m_GameGrid.Length;
		m_Camera.RotateCamera(-360f / m_NumberOfFaces);
	}
	
	// Change game grid face to 'left'.
	public void DecreaseFace()
	{
		m_CurrentFace = m_CurrentFace-1;
		if(m_CurrentFace < 0)
			m_CurrentFace = m_GameGrid.Length-1;
		
		m_Camera.RotateCamera(360f / m_NumberOfFaces);
	}
	
	// Check if the target lies on the given point.
	public bool PointHasTarget(PointData point)
	{
		if(m_CurrentFace != m_Target.m_GridFace) return false;
		
		return PointData.Equals(m_Target.m_Position, point);
	}
	
	// Spawn target object for snake to eat.
	public void SpawnTarget()
	{
		// Pick up the free cells.
		CellData[] freecells = FreeCells();
		
		// No more free slots, game won!
		if(freecells.Length == 0)
		{
			GameOver("Game won!");
			return;
		}
		
		// Pick a randomm point from the free cells.
		m_Target = freecells[Random.Range(0, freecells.Length)];
		m_Target.m_Object = Instantiate(m_TargetObject);
		m_Target.m_Object.position = m_GameGrid[m_Target.m_GridFace].GetPosition(m_Target.m_Position);
	}
	
	// Snake eats the target.
	public void EatTarget()
	{
		// Destroy the target object.
		Destroy(m_Target.m_Object.gameObject);
		
		SpawnTarget();
	}
	
	// Map all the free cells in the game grid.
	private CellData[] FreeCells()
	{
		CellData cell;
		List<CellData> freecells = new List<CellData>();
		
		// Loop through all the grids.
		for(int i = 0; i < m_GameGrid.Length; i++)
		{
			// Loop X and Y.
			for(int y = 0; y < m_GridSize; y++)
			{
				for(int x = 0; x < m_GridSize; x++)
				{
					PointData point = new PointData(x, y);
					
					// Free slots won't have snake parts in them.
					if(!Snake.OccupiesPosition(i, point))
					{
						cell = new CellData();
						cell.m_GridFace = i;
						cell.m_Position = point;
						freecells.Add(cell);
					}
				}
			}
		}
		
		// Snakes head has to be counted as well.
		cell = new CellData();
		cell.m_GridFace = m_CurrentFace;
		cell.m_Position = Snake.Position;
		freecells.Add(cell);
		
		// Return as array.
		return freecells.ToArray();
	}
	
	// Debug function helps to visualize while playing in editor mode.
	void OnDrawGizmos()
	{
		if(m_GameGrid == null) return;
		if(m_GameGrid.Length == 0) return;
		
		for(int i = 0; i < m_GameGrid.Length; i++)
		{
			for(int y = 0; y < m_GridSize; y++)
			{
				for(int x = 0; x < m_GridSize; x++)
					Gizmos.DrawWireCube(m_GameGrid[i].GetPosition(y, x), Vector3.one * m_CellSize);
			}
		}
	}
}
