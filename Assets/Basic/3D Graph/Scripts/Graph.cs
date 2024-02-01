using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Shared;

namespace Graph {
public class Graph : MonoBehaviour, IGraphCycleCurve {

	[OnChangedCall(nameof(EditorConstructGraph))]
    [SerializeField, Range(10, 120)] int resolution = 100;
	[OnChangedCall(nameof(EditorReescaleGraph))]
    [SerializeField, Range(0.01f, 1f)] float graphScale = 1/10f;
    [SerializeField, Range(-10, 10)] float timeScale = 1.0f;
    [SerializeField, Range(0.1f, 5f)] float transitionDuration = 1.0f;
	[SerializeField] GraphController controller;
    [SerializeField] Transform pointPrefab;
	[OnChangedCall(nameof(EditorInterpolateCurves))]
    [SerializeField] Curve.CurveName curveToPlot;

#region Curve parameters
    [Header("Curve parameters")]

    [OnChangedCall(nameof(EditorUpdateParameters))]
    [SerializeField, Range(-3, 3)] float frequency = -1.0f;
    [OnChangedCall(nameof(EditorUpdateParameters))]
    [SerializeField, Range(-3, 3)] float amplitude = 1.0f;
    [OnChangedCall(nameof(EditorUpdateParameters))]
    [SerializeField, Range(0, 8)] float radius = 1.0f;
    [OnChangedCall(nameof(EditorUpdateParameters))]
    [SerializeField, Range(0, 8)] float inner_radius = 0.5f;
    [OnChangedCall(nameof(EditorUpdateParameters))]
    [SerializeField, Range(-8, 8)] float anim_scale = 0.0f;
	[OnChangedCall(nameof(EditorUpdateParameters))]
	[SerializeField, Range(0, 15)] float v_ripple_frequency = 1.0f;
	[OnChangedCall(nameof(EditorUpdateParameters))]
	[SerializeField, Range(0, 15)] float h_ripple_frequency = 0.0f;
	[OnChangedCall(nameof(EditorUpdateParameters))]
	[SerializeField, Range(0, 15)] float v_ripple_frequency2 = 0.0f;
	[OnChangedCall(nameof(EditorUpdateParameters))]
	[SerializeField, Range(0, 15)] float h_ripple_frequency2 = 1.0f;
#endregion

    Transform[] dataPoints;
    Curve.CurveDelegate func;
    Curve.CurveDelegate nextFunc;
    Curve.CurveName currentCurveToPlot;
    Curve.CurveName nextCurveToPlot;
    int nPoints;
    int curvesListSize;
    bool transitioning;
    float transitionProgress;

    internal const int ORIGIN_X = -8;
    internal const int ORIGIN_Y = -8;
    internal const int GRAPH_BOUNDS = 16;
    internal const int GRAPH_SCALE = 8;

    void Awake(){
    	controller.SetGraph(this);
    	Curve.UpdateGlobalParameters(frequency, amplitude, radius, inner_radius, anim_scale, v_ripple_frequency, h_ripple_frequency, v_ripple_frequency2, h_ripple_frequency2);
    	currentCurveToPlot = curveToPlot;

    	nPoints = resolution*resolution;
    	float x = ORIGIN_X;
    	float y = ORIGIN_Y;
    	float step = GRAPH_BOUNDS/((float)resolution);
    	func = Curve.CurveGetter(currentCurveToPlot);
    	dataPoints = CreatePoints(x, y, step, nPoints);

		curvesListSize = Enum.GetValues(typeof(Curve.CurveName)).Length;
    }

    void Update(){
    	func = Curve.CurveGetter(currentCurveToPlot);
    	nextFunc = Curve.CurveGetter(nextCurveToPlot);

    	if(transitioning){
    		InterpolateCurves(func, nextFunc);
    	} else {
    		DrawCurve(func);
    	}
    }

    public void ConstructGraph() {
    	nPoints = resolution*resolution;
    	var newData = new Transform[nPoints];
    	int n = dataPoints.Length < nPoints ? dataPoints.Length : nPoints;

    	float u = ORIGIN_X;
    	float v = ORIGIN_Y;
    	float step = GRAPH_BOUNDS/((float)resolution);

    	// Reuse existing objects
    	for(int i = 0, uCount = 0; i < n; i++, uCount++){
    		if(uCount == resolution){
    			u = ORIGIN_X;
    			uCount = 0;
    			v += step;
    		}
    		newData[i] = dataPoints[i];
            newData[i].transform.localPosition = func(u, v, 0f);
    		newData[i].transform.localScale = new Vector3(graphScale, graphScale, graphScale);
    		u += step;
    	}

    	// Merge two arrays and dispose of leftover objects
    	if(dataPoints.Length < nPoints){
    		int missingPoints = nPoints-dataPoints.Length;
    		Array.Copy(CreatePoints(u, v, step, missingPoints), 0, newData, n, missingPoints);
    	} else {
    		for(int i = nPoints; i < dataPoints.Length; i++){
    			Destroy(dataPoints[i].gameObject);
    		}
    	}

    	dataPoints = newData;
    }

    Transform[] CreatePoints(float startX, float startY, float step, int n){
    	var points = new Transform[n];

    	float u = startX;
    	float v = startY;
    	for(int i = 0, uCount = 0; i < n; i++, uCount++){
    		if(uCount == resolution){
    			u = startX;
    			uCount = 0;
    			v += step;
    		}
    		points[i] = Instantiate(pointPrefab, transform);
            points[i].transform.localPosition = func(u, v, 0f);
    		points[i].transform.localScale = new Vector3(graphScale, graphScale, graphScale);
    		u += step;
    	}
    	return points;
    }

    void DrawCurve(Curve.CurveDelegate curve) {
    	float t = Time.time*timeScale;
    	float step = GRAPH_BOUNDS/((float)resolution);
    	float u = ORIGIN_X;
    	float v = ORIGIN_Y;

    	for(int i = 0, uCount = 0; i < nPoints; i++, uCount++){
    		if(uCount == resolution){
    			u = ORIGIN_X;
    			uCount = 0;
    			v += step;
    		}
            dataPoints[i].transform.localPosition = curve(u, v, t);
    		u += step;
    	}
    }

    void InterpolateCurves(Curve.CurveDelegate from, Curve.CurveDelegate to){
    	// Just jump to next curve
    	if(timeScale == 0.0f){
			transitioning = false;
    		currentCurveToPlot = nextCurveToPlot;
			func = nextFunc;
    	}

		float t = Time.time*timeScale;
    	float step = GRAPH_BOUNDS/((float)resolution);
    	float u = ORIGIN_X;
    	float v = ORIGIN_Y;
    	for(int i = 0, uCount = 0; i < nPoints; i++, uCount++){
    		if(uCount == resolution){
    			u = ORIGIN_X;
    			uCount = 0;
    			v += step;
    		}

    		dataPoints[i].transform.localPosition = Curve.Morph(u, v, t, func, nextFunc, transitionProgress);
    		u += step;
    	}

    	transitionProgress += Time.deltaTime/transitionDuration * Mathf.Abs(timeScale);
		if(transitionProgress >= 1.0f) {
			transitioning = false;
			currentCurveToPlot = nextCurveToPlot;
			func = nextFunc;
		}
    }

    public void NextCurve(){
    	nextCurveToPlot = (int) curveToPlot < curvesListSize - 1 ? curveToPlot+1 : 0;
    	curveToPlot = nextCurveToPlot;
    	transitionProgress = 0f;
    	transitioning = true;
    }

#region Editor hooks
    public void EditorConstructGraph() {
    	#if UNITY_EDITOR
    	 if(!Application.isPlaying) {
    	 	 return;
    	 }
    	#endif
    	ConstructGraph();
    }

    public void EditorInterpolateCurves(){
    	#if UNITY_EDITOR
    	 if(!Application.isPlaying) {
    	 	 return;
    	 }
    	#endif
    	nextCurveToPlot = curveToPlot;
    	nextFunc = Curve.CurveGetter(nextCurveToPlot);
    	transitionProgress = 0f;
    	transitioning = true;
    	InterpolateCurves(func, nextFunc);
    }

    public void EditorUpdateParameters(){
    	#if UNITY_EDITOR
    	 if(!Application.isPlaying) {
    	 	 return;
    	 }
    	#endif
    	Curve.UpdateGlobalParameters(frequency, amplitude, radius, inner_radius, anim_scale, v_ripple_frequency, h_ripple_frequency, v_ripple_frequency2, h_ripple_frequency2);
    }

    public void EditorReescaleGraph(){
    	#if UNITY_EDITOR
    	 if(!Application.isPlaying) {
    	 	 return;
    	 }
    	#endif
    	foreach(var point in dataPoints){
    		point.transform.localScale = new Vector3(graphScale, graphScale, graphScale);
    	}
    }
#endregion
}}
