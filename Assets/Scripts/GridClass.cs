//
//  GridClass.cs
//
//  Author:
//       Rimber <r1mb3r@gmail.com>
//
//  Copyright (c) 2015 Rimber
using UnityEngine;

// Class to handle position and direction in the game grid.
[System.Serializable]
public class PointData
{
	[SerializeField] private int m_X;
	[SerializeField] private int m_Y;
	
	public int X
	{
		get { return m_X; }
		set { m_X = value; }
	}
	public int Y
	{
		get { return m_Y; }
		set { m_Y = value; }
	}
	
	public PointData(int x, int y)
	{
		m_X = x;
		m_Y = y;
	}

    public static Vector3 operator *(PointData a, Quaternion rotation)
    {
        Vector3 v = new Vector3(a.X, a.Y, 0f);
        return rotation * v;
    }
	
	public static PointData operator +(PointData a, PointData b)
	{
		PointData d = new PointData(a.X, b.X);
		d.m_X = a.m_X + b.m_X;
		d.m_Y = a.m_Y + b.m_Y;
		return d;
	}
	
	public static bool Equals(PointData a, PointData b)
	{
		if((a == null) || (b == null)) return false;
		
		return ((a.X == b.X) && (a.Y == b.Y));
	}
	
	public override string ToString ()
	{
		return string.Format ("X={0}, Y={1}", X, Y);
	}
}

// Grid class holds information of the points within the grid.
// Inherit from the Mesh Creator and use the mesh from it to visualize.
public class GridClass : MeshCreatorClass
{
	private const int MESH_SEGMENT_COUNT = 2;

	private int m_GridSize;
	private Vector3[] m_Points;
	
	public bool HasPoint(PointData point)
	{
		if(point.Y < 0) return false;
		if(point.Y >= m_GridSize) return false;
		
		if(point.X < 0) return false;
		if(point.X >= m_GridSize) return false;
		
		return true;
	}
	
	public Vector3 GetPosition(PointData point)
	{
		return GetPosition(point.X, point.Y);
	}
	
	public Vector3 GetPosition(int x, int y)
	{
		return m_Points[(y*m_GridSize) + x];
	}
	
	// Assume the X-axis holds open borders and when passing
	// over the edge, the border is crossed.
	public bool CrossingBorder(PointData point)
	{
		if(point.X < 0)
			return true;
		if(point.X >= m_GridSize)
			return true;
			
		return false;
	}
	
	// When Snake crosses the border of the grid, it jumps
	// to the other end of the grid. At the same time 
	// current game face is changed thus Snake jumps to 
	// other grid.
	public PointData CrossBorder(PointData point)
	{
		// Exit on left side
		if(point.X < 0)
		{
			point.X = m_GridSize-1;
			CoreClass.Instance.DecreaseFace();
		}
		// Exit on right side
		if(point.X >= m_GridSize)
		{
			point.X = 0;
			CoreClass.Instance.IncreaseFace();
		}
		
		return point;
	}
	
	// Move grid points along normal for X-distance.
	public void TranslatePoints(float distance)
	{
		// Call parent function to move the created mesh.
		base.Translate(distance);
		
		for(int i = 0; i < m_Points.Length; i++)
			m_Points[i] = m_Points[i] + m_Normal*distance;
	}
	
	// Rotate grid points.
	// Use origin as pivot point.
	public void RotatePoints(float angle)
	{
		// Call parent to rotate the created mesh.
		base.Rotate(angle);
		
		for(int i = 0; i < m_Points.Length; i++)
			m_Points[i] = RotatePointAroundPivot(m_Points[i], Vector3.zero, Vector3.forward * angle);
	}
	
	// Helper function to rotate points around a pivot point.
	private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler(angles) * dir;
		point = dir + pivot;
		return point;
	}
	
	// Initialize grid
	public void Initialize(int gridsize, float cellsize)
	{
		m_GridSize = gridsize;
		m_Normal = Vector3.up;
		
		Vector3 position;
		m_Points = new Vector3[m_GridSize * m_GridSize];
		int half = Mathf.FloorToInt(m_GridSize/2f);

		for(int y = 0; y < m_GridSize; y++)
		{
			for(int x = 0; x < m_GridSize; x++)
			{
				position = Vector3.forward * cellsize * (y-half) + Vector3.right * cellsize * (x-half);
				m_Points[(y*m_GridSize) + x] = position;
			}
		}
		
		// Create mesh for the grid.
		// Set offset for mesh to be half cell lower. This way it will be on the bottom of the cell.
		CreateMesh(MESH_SEGMENT_COUNT, gridsize * cellsize, -Vector3.up * (cellsize/2f));
	}
}
