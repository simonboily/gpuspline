// Copyright (C) 2017 Simon Boily

using UnityEngine;
using System.Collections;

/// <summary>
/// SplineBatcher example usage
/// Randomly generates control points and animates them
/// </summary>
public class TestSpline : MonoBehaviour
{
	public int SplineCount = 80;
	public int ControlPointsPerSpline = 9;
	public int VerticesPerSpline = 80;

	public SplineBatcher batcher;

	public AnimationCurve animationX;
	public AnimationCurve animationY;

	private Vector2[] originalControlPoints;
	private Vector2[] frameControlPoints;

	void Awake () {
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 0;

		InitControlPoints();
	}

	void InitControlPoints()
	{
		Vector2[] templateCP = new Vector2[ControlPointsPerSpline];

		if (ControlPointsPerSpline == 9)
		{
			templateCP[0] = new Vector2(0, 0);
			templateCP[2] = new Vector2(1, 0);
			templateCP[3] = new Vector2(1, 2);
			templateCP[4] = new Vector2(0, 2);
			templateCP[5] = new Vector2(-1, 3);
			templateCP[6] = new Vector2(-2, 3);
			templateCP[7] = new Vector2(-2, 3);
			templateCP[8] = new Vector2(-4, 3);
		}
		else
		{
			for (int i = 0; i < ControlPointsPerSpline; i++)
			{
				templateCP[i] = new Vector2(UnityEngine.Random.Range(-1.0f, -1.0f), UnityEngine.Random.Range(-1.0f, 1.0f));
			}
		}

		Vector2[] instanceCP = new Vector2[ControlPointsPerSpline];
		originalControlPoints = new Vector2[ControlPointsPerSpline*SplineCount];
		frameControlPoints = new Vector2[originalControlPoints.Length];

		for (int i = 0; i < SplineCount; i++)
		{
			Vector2 disp = new Vector2(UnityEngine.Random.Range(-2.0f, 2.0f), UnityEngine.Random.Range(-4.0f, 2.0f));

			for (int j = 0; j < instanceCP.Length; j++)
			{
				instanceCP[j] = templateCP[j] + disp;
				originalControlPoints[i*instanceCP.Length+j] = instanceCP[j];
			}
			
			batcher.Add(instanceCP, VerticesPerSpline);
		}

		batcher.Generate();
	}

	void Update()
	{
		// animate the splines
		float time = (Time.time * 0.25f) - (int)(Time.time * 0.25f);

		for (int i = 0; i < originalControlPoints.Length; i++)
		{
			float curTime = Mathf.Repeat(time + (i % 5) / (float)5, 1.0f);
			float dispX = animationX.Evaluate(curTime);
			float dispY = animationY.Evaluate(curTime);

			frameControlPoints[i] = originalControlPoints[i] + new Vector2(dispX, dispY);
		}

		batcher.Modify(frameControlPoints);
	}
}
