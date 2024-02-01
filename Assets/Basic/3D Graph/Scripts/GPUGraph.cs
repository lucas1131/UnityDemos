using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Shared;

namespace Graph {
public class GPUGraph : MonoBehaviour, IGraphCycleCurve {

    internal const int ORIGIN_X = -8;
    internal const int ORIGIN_Y = -8;
    internal const int GRAPH_BOUNDS = 16;
    internal const int GRAPH_SCALE = 8;

    private const int MaxResolution = 2000;

    [SerializeField, Range(10, MaxResolution)] int resolution = 200;
    [SerializeField, Range(1f, 15f)] float pointScale = 8f;
    [SerializeField, Range(-10, 10)] float timeScale = 1.0f;
    [SerializeField, Range(0.1f, 5f)] float transitionDuration = 1.0f;
    [SerializeField] ComputeShader curvesShader;
    [SerializeField] GraphController controller;
    [OnChangedCall(nameof(EditorInterpolateCurves))]
    [SerializeField] Curve.CurveName curveToPlot;
    [SerializeField] Curve.CurveName _curveToPlot;
    [SerializeField] Material pointMaterial;
    [SerializeField] Mesh pointMesh;

    ComputeBuffer positionsBuffer;
    Curve.CurveName nextCurveToPlot;
    int nPoints;
    int curvesListSize;
    bool transitioning;
    float transitionProgress;
    float transitionTime;

#region Curve parameters
    [Header("Curve parameters")]
    [SerializeField, Range(-5, 5)] float frequency = -1.0f;
    [SerializeField, Range(-1, 1)] float amplitude = 1.0f;
    [SerializeField, Range(0, 1)] float radius = 1.0f;
    [SerializeField, Range(0, 1)] float inner_radius = 0.5f;
    [SerializeField, Range(-8, 8)] float anim_scale = 0.0f;
	[SerializeField, Range(0, 15)] float v_ripple_frequency = 1.0f;
	[SerializeField, Range(0, 15)] float h_ripple_frequency = 0.0f;
	[SerializeField, Range(0, 15)] float v_ripple_frequency2 = 0.0f;
	[SerializeField, Range(0, 15)] float h_ripple_frequency2 = 1.0f;
#endregion

#region GPU properties
    static readonly int progressId = Shader.PropertyToID("_Progress");
    static readonly int positionsId = Shader.PropertyToID("_Positions");
    static readonly int pointScaleId = Shader.PropertyToID("_PointScale");
    static readonly int stepId = Shader.PropertyToID("_Step");
    static readonly int timeId = Shader.PropertyToID("_Time");
    static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    static readonly int g_frequencyId = Shader.PropertyToID("_G_frequency");
    static readonly int g_amplitudeId = Shader.PropertyToID("_G_amplitude");
    static readonly int g_radiusId = Shader.PropertyToID("_G_radius");
    static readonly int g_inner_radiusId = Shader.PropertyToID("_G_inner_radius");
    static readonly int g_anim_scaleId = Shader.PropertyToID("_G_anim_scale");
    static readonly int g_v_ripple_frequencyId = Shader.PropertyToID("_G_v_ripple_frequency");
    static readonly int g_h_ripple_frequencyId = Shader.PropertyToID("_G_h_ripple_frequency");
    static readonly int g_v_ripple_frequency2Id = Shader.PropertyToID("_G_v_ripple_frequency2");
    static readonly int g_h_ripple_frequency2Id = Shader.PropertyToID("_G_h_ripple_frequency2");
#endregion

    void Awake(){
        controller.SetGraph(this);
    	Curve.UpdateGlobalParameters(frequency, amplitude, radius, inner_radius, anim_scale, v_ripple_frequency, h_ripple_frequency, v_ripple_frequency2, h_ripple_frequency2);
		curvesListSize = Enum.GetValues(typeof(Curve.CurveName)).Length;
    }

    void OnEnable () {
        // 3 floats for Vector3
        positionsBuffer = new ComputeBuffer(MaxResolution*MaxResolution, 3 * sizeof(float));
    }

    void OnDisable () {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    void Update(){
        UpdateCurveGPU();

        if(_curveToPlot != curveToPlot){
            Debug.Log($"curve change from {_curveToPlot} to {curveToPlot}");
            _curveToPlot = curveToPlot;
        }
    }

    // transition not working properly yet, im lazy`
    public void NextCurve(){
        int next = (int) curveToPlot + 1;
        nextCurveToPlot = (Curve.CurveName) next;

    	transitionProgress = 0f;
        transitionTime = 0f;
    	transitioning = true;
        Debug.Log($"Starting transition from {curveToPlot} ({(int) curveToPlot}) to {nextCurveToPlot} ({(int) nextCurveToPlot + (int) curveToPlot*5})");
    }

    void UpdateComputeShaderValues(float step, int kernel){
        // int kernel = curvesShader.FindKernel("CurveKernel"); // just to know this exists
        curvesShader.SetBuffer(kernel, positionsId, positionsBuffer);
        curvesShader.SetInt(resolutionId, resolution);
        curvesShader.SetFloat(stepId, step);
        curvesShader.SetFloat(timeId, Time.time*timeScale);
        curvesShader.SetFloat(g_frequencyId, frequency);
        curvesShader.SetFloat(g_amplitudeId, amplitude);
        curvesShader.SetFloat(g_radiusId, radius);
        curvesShader.SetFloat(g_inner_radiusId, inner_radius);
        curvesShader.SetFloat(g_anim_scaleId, anim_scale);
        curvesShader.SetFloat(g_v_ripple_frequencyId, v_ripple_frequency);
        curvesShader.SetFloat(g_h_ripple_frequencyId, h_ripple_frequency);
        curvesShader.SetFloat(g_v_ripple_frequency2Id, v_ripple_frequency2);
        curvesShader.SetFloat(g_h_ripple_frequency2Id, h_ripple_frequency2);
    }

    void UpdatePointShaderMaterialValues(float step){
        pointMaterial.SetBuffer(positionsId, positionsBuffer);
        pointMaterial.SetFloat(pointScaleId, pointScale);
        pointMaterial.SetFloat(stepId, step);
    }

    void UpdateCurveGPU(){
        int kernelFuncIndex = (int) curveToPlot * 6;
        if (transitioning) {
            transitionTime += Time.deltaTime/transitionDuration * Mathf.Abs(timeScale);
            transitionProgress += Mathf.SmoothStep(0f, 1f,  transitionTime/transitionDuration);
            curvesShader.SetFloat(progressId, transitionProgress);
            kernelFuncIndex = (int) nextCurveToPlot;

            if(transitionProgress >= 1.0f){
                transitioning = false;
                int currentCurve = (int) curveToPlot+1 + (int) nextCurveToPlot*5;
                curveToPlot = (Curve.CurveName) (currentCurve%curvesListSize);
                kernelFuncIndex = currentCurve%30;
                Debug.Log($"finished transition, current curve is {curveToPlot} ({(int) curveToPlot}) | {kernelFuncIndex}");
            }
        }

        float step = 2f/resolution;
        int kernelFunc = kernelFuncIndex;
        Debug.Log("plotting kernel: " + kernelFunc);
        
        UpdateComputeShaderValues(step, kernelFunc);
        int groups = Mathf.CeilToInt(resolution / 8f);
        curvesShader.Dispatch(kernelFunc, groups, groups, 1);

        UpdatePointShaderMaterialValues(step);
        RenderParams rp = new RenderParams(pointMaterial);
        rp.worldBounds = new Bounds(transform.position, 20f*Vector3.one);
        rp.matProps = new MaterialPropertyBlock();
        Graphics.RenderMeshPrimitives(rp, pointMesh, 0, resolution*resolution);
    }

    public void EditorInterpolateCurves(){
        #if UNITY_EDITOR
         if(!Application.isPlaying) {
             return;
         }
        #endif

        // nextCurveToPlot = curveToPlot;
        // nextFunc = Curve.CurveGetter(nextCurveToPlot);
        // transitionProgress = 0f;
        // transitioning = true;
        // InterpolateCurves(func, nextFunc);
    }
}}

