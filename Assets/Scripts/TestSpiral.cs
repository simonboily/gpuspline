// Copyright (C) 2017 Simon Boily

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SplineBatcher example usage
/// Generates high polycount spirals and morphs them to circles back and forth
/// </summary>
public class TestSpiral : MonoBehaviour
{
	public float numSpirals = 10;
	public int numVerticesPerSpiral = 1000;
	public SplineBatcher batcher;

	int numControlPointsPerShape;
	Vector2[] target, srcCircle, srcSpiral;

	public AnimationCurve animationCurve;

	void GenerateCircle(List<Vector2> points, Vector2 origin)
	{
		for (float theta=0.0f; theta < Mathf.PI * 16.0f; theta += 0.4f)
		{
			points.Add(new Vector2((float)Mathf.Cos(theta/7.6f), (float)Mathf.Sin(theta/7.6f)) + origin);
		}
	}

	void GenerateSpiral(List<Vector2> points, Vector2 origin, float variability)
	{
		for (float theta=0.0f; theta < Mathf.PI * 16.0f; theta += 0.4f)
		{
			float d = (float)Mathf.Pow(variability, theta) * 0.1f;

			points.Add(new Vector2(d * (float)Mathf.Cos(theta), d * (float)Mathf.Sin(theta)) + origin);
		}
	}

	void Awake()
	{
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 0;

		List<Vector2> pointPool0 = new List<Vector2>();
		List<Vector2> pointPool1 = new List<Vector2>();
		List<Vector2> points0 = new List<Vector2>();
		List<Vector2> points1 = new List<Vector2>();

		Vector2 anchor = new Vector2(-2.0f, -3.0f);
		Vector2 range = new Vector2(4.0f, 6.0f);

		int rowX = Mathf.Max (Mathf.CeilToInt(Mathf.Sqrt(numSpirals)), 2);
		int rowY = rowX;

		int curSpiralCount = 0;
		int totalControlPoints = 0;

		bool done = false;

		for (int y = 0; y < rowY; y++)
		{
			for (int x = 0; x < rowX; x++)
			{
				points0.Clear();
				points1.Clear();

				Vector2 origin = anchor + new Vector2(Mathf.Lerp(0, range.x, x / (float)(rowX-1)), Mathf.Lerp(0, range.y, y / (float)(rowY-1)));
	
				GenerateCircle(points0, origin);
				GenerateSpiral(points1, origin, 1.04f + (x+y)/(float)(16*rowX+16*rowY));

				pointPool0.AddRange(points0);
				pointPool1.AddRange(points1);

				// this should remain constant accross all shapes
				numControlPointsPerShape = points0.Count;
				totalControlPoints += numControlPointsPerShape;

				batcher.Add(points0.ToArray(), numVerticesPerSpiral);

				curSpiralCount++;
				if (curSpiralCount >= numSpirals)
				{
					done = true;
					break;
				}
			}

			if (done)
				break;
		}

		srcCircle = pointPool0.ToArray();
		srcSpiral = pointPool1.ToArray();
		target = new Vector2[totalControlPoints];
		batcher.Generate();
	}

	void Update ()
	{
		const float speed = 0.25f;

		float time = Mathf.Repeat(Time.time * speed, 1.0f);
		float blendFactor = 0;
		int prevSpline = -1;

		for (int i = 0; i < target.Length; i++)
		{
			int indexSpline = i / numControlPointsPerShape;
			if (prevSpline != indexSpline)
			{
				float curTime = Mathf.Repeat(time + (indexSpline % numSpirals) / (float)numSpirals, 1.0f);
				blendFactor = animationCurve.Evaluate(curTime);

				prevSpline = indexSpline;
			}

			target[i] = Vector2.Lerp(srcCircle[i], srcSpiral[i], blendFactor);
		}

		batcher.Modify(target);
	}
}