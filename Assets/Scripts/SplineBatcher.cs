// Copyright (C) 2017 Simon Boily

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Aggregates and renders a series of spline
/// </summary>
public class SplineBatcher : MonoBehaviour
{
	public Material material;
    public float    width;

    const int       maxVertices = 65000;
    const int       maxControlPoints = 1000;

    static int      propWidth = -1;
    static int      propControlPoints = -1;
    bool            dirtyControlPoints = true;
    bool            dirtyWidth = true;

    List<Spline>	splines = new List<Spline>();
    List<Batch>  	batches = new List<Batch>();

    struct Spline
    {
        public int indexBatch;                  // which batch this spline belongs to
        public int indexControlPoints;          // location of the spline's control points in the control point array
        public int numControlPoints;
        public int numVertices;
    };

    class Batch
    {
        public Mesh                     mesh;
        public MaterialPropertyBlock    materialProperty = new MaterialPropertyBlock();
        public Vector4[]                controlPoints = new Vector4[maxControlPoints];
        public int                      numControlPoints = 0;
        public int						numVertices = 0;
        public int                      indexSplineStart = -1, indexSplineEnd = -1;

        public Batch(int indexSpline)
        {
            indexSplineStart = indexSpline;
        }

        public void AppendControlPoints(Vector2[] cp)
        {
            for (int i = 0; i < cp.Length; i++)
            {
                controlPoints[numControlPoints] = cp[i];
				numControlPoints++;
            }
        }
    }

	/// <summary>
	/// Add a spline to the batcher
	/// </summary>
	/// <returns>Reference to the spline</returns>
	/// <param name="cp">Control points.</param>
	/// <param name="numVertices">Number of vertical vertices.</param>
    public int Add(Vector2[] cp, int numVertices)
    {
		Batch curBatch = batches.Count != 0 ? batches[batches.Count-1] : null;		
		if (curBatch == null || (curBatch.numControlPoints + cp.Length) > maxControlPoints || (curBatch.numVertices + numVertices*2) > maxVertices)
        {
            if (curBatch != null)
            {
                // the current batch is full, generate the mesh
                GenerateMesh(batches[batches.Count-1]);
            }

            curBatch = new Batch(splines.Count);
            
            batches.Add(curBatch);
        }

		Spline spline;
		spline.indexControlPoints = curBatch.numControlPoints;
		spline.numControlPoints = cp.Length;
		spline.numVertices = numVertices;
		spline.indexBatch = batches.Count-1;
		splines.Add(spline);

        curBatch.AppendControlPoints(cp);
        curBatch.indexSplineEnd = splines.Count;
		curBatch.numVertices += numVertices*2;

        return splines.Count-1;
    }

	/// <summary>
	/// Prepare the batcher for rendering
	/// </summary>
    public void Generate()
    {
        if (batches.Count > 0 && batches[batches.Count-1].mesh == null)
        {
            GenerateMesh(batches[batches.Count-1]);
        }
    }

	/// <summary>
	/// Modify every single control points that was previously added in one single swoop
	/// </summary>
    public void Modify(Vector2[] cp)
    {
        int indexCP = 0;
        for (int i = 0; i < batches.Count; i++)
        {
            Batch batch = batches[i];

            for (int j = 0; j < batch.numControlPoints; j++)
            {
                batch.controlPoints[j] = cp[indexCP];
                indexCP++;
            }
        }

        dirtyControlPoints = true;
    }
    
    void GenerateMesh(Batch batch)
	{
        batch.mesh = new Mesh();

        // OPT: preallocate these
		List<Vector3> verts = new List<Vector3>();
        List<Color32> colors = new List<Color32>();
        List<int> tris = new List<int>();

		int vertCount = 0;
        int controlPointCount = 0;

		for (int k = batch.indexSplineStart; k < batch.indexSplineEnd; k++)
		{
            Spline entry = splines[k];

			for (int i = 0; i < entry.numVertices; i++)
			{
				float norm = (float) i / (float) entry.numVertices;

				float interval = norm * (entry.numControlPoints-3);
				float subInterval = interval - (int)interval;
                int index = controlPointCount + (int)interval;

				// x : V texture coordinate
				// y : Spline interval t [0..1]
				// z : Index ino the control point uniform
				verts.Add(new Vector3(norm, subInterval, index));
				verts.Add(new Vector3(norm, subInterval, index));

                // Red = 0 : left vertex
                // Red = 255 : right vertex
                colors.Add(new Color32(0, 0, 0, 0));
                colors.Add(new Color32(255, 0, 0, 0));
            }

			for (int i = 0; i < entry.numVertices-1; i++)
			{
				int vertIndex = vertCount + i*2;

				tris.Add(vertIndex);
				tris.Add(vertIndex+2);
				tris.Add(vertIndex+1);

				tris.Add(vertIndex+1);
				tris.Add(vertIndex+2);
				tris.Add(vertIndex+3);
			}

			vertCount += entry.numVertices*2;
            controlPointCount += entry.numControlPoints;
		}

		batch.mesh.vertices = verts.ToArray();
		batch.mesh.triangles = tris.ToArray();
        batch.mesh.colors32 = colors.ToArray();
        batch.mesh.UploadMeshData(true);

        batch.materialProperty = new MaterialPropertyBlock();
	}

	void Awake()
	{
		if (propControlPoints < 0)
			propControlPoints = Shader.PropertyToID("_ControlPoints");
		if (propWidth < 0)
			propWidth = Shader.PropertyToID("_Width");
	}

	void Update()
	{
		for (int i = 0; i < batches.Count; i++)
        {
        	// generate the mesh if it hasn't yet been done
        	if (batches[i].mesh == null)
        		GenerateMesh(batches[i]);

            if (dirtyControlPoints)
                batches[i].materialProperty.SetVectorArray(propControlPoints, batches[i].controlPoints);

            if (dirtyWidth)
                batches[i].materialProperty.SetFloat(propWidth, width);

  			Graphics.DrawMesh(batches[i].mesh, Matrix4x4.identity, material, 0, Camera.current, 0, batches[i].materialProperty, false, false, false);
        }

        dirtyControlPoints = false;
        dirtyWidth = false;
	}
}