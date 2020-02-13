//
//  MeshCreatorClass.cs
//
//  Author:
//       Rimber <r1mb3r@gmail.com>
//
//  Copyright (c) 2015 Rimber
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshCreatorClass : MonoBehaviour
{
	private int m_X, m_Y;
	protected Vector3 m_Normal; // Mesh normal
	
	public Vector3 GetNormal()
	{
		return m_Normal;
	}
	
	protected void Rotate(float angle)
	{
		// Rotate grid normal as well.
		m_Normal = Quaternion.Euler(angle * Vector3.forward) * m_Normal;
		
		// Rotate our mesh
		transform.Rotate(Vector3.forward, angle);
	}
	
	protected void Translate(float distance)
	{
		transform.position = m_Normal * distance;
	}
	
	// Create plane mesh.
	// Segments defines how many sections the plane is cut in. This will always be square.
	// Size defines mesh size.
	// Offset can be provided to create offset for the mesh points.
	protected void CreateMesh(int segments, float size, Vector3 offset)
	{
		// Must have at least 2 lines!
		if(segments < 2)
			segments = 2;
			
		m_X = segments;
		m_Y = segments;
		
		Vector3[] points;
		points = new Vector3[m_X * m_Y];
		float half = size/2f;
		float sizex = size/(m_X-1);
		float sizey = size/(m_Y-1);
		 
		// Create the mesh points.
		for(int y = 0; y < m_Y; y++)
		{
			for(int x = 0; x < m_X; x++)
			{
				points[x + (y * m_X)] = new Vector3(x*sizex - half, 0f, y*sizey-half) + offset;
			}
		}
		
		// Get Mesh
		MeshFilter filter = this.GetComponent<MeshFilter>();
		Mesh mesh = filter.mesh;
		mesh.Clear();
		
		// Create mesh vertices, normals, uvs and finally trianglestrip.
		mesh.vertices = points;
		mesh.normals = CreateNormals(points);
		mesh.uv = CreateUV(points);
		mesh.triangles = CreateVertices(points);
		
		// Recalc
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
	}
	
	// Helper functioon to creates the vertices.
	private int[] CreateVertices(Vector3[] points)
	{
		// Init
		int count = (m_X-1) * (m_Y-1);
		int[] triangles = new int[count*6];
		int t = 0;
		
		// Go through our list of points
		for(int y = 0; y < count; y++)
		{
			// Retrieve lower left corner from face ind
			int i = y % (m_X - 1) + (y / (m_Y - 1) * m_X);
			
			triangles[t++] = i + m_X;
			triangles[t++] = i + 1;
			triangles[t++] = i;
			
			triangles[t++] = i + m_X;	
			triangles[t++] = i + m_X + 1;
			triangles[t++] = i + 1; 
		}
		
		return triangles;
	}
	
	// Helper function to creates the UV coordinates for the mesh.
	private Vector2[] CreateUV(Vector3[] points)
	{
		Vector2[] uvs = new Vector2[ points.Length ];
		for(int v = 0; v < m_Y; v++)
		{
			for(int u = 0; u < m_X; u++)
			{
				uvs[ u + v * m_X ] = new Vector2( (float)u / (m_X - 1), (float)v / (m_Y - 1) );
			}
		}
		
		return uvs;
	}
	
	// Helper function to create normals for the mesh.
	private Vector3[] CreateNormals(Vector3[] points)
	{
		Vector3[] normals = new Vector3[ points.Length ];
		for( int n = 0; n < normals.Length; n++ )
			normals[n] = m_Normal;
			
		return normals;
	}
}

