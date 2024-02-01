using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Mathf;
using static Graph.Graph;

namespace Graph {
public static class Curve {

	public static float g_frequency = 1.0f;
    public static float g_amplitude = 1.0f;
    public static float g_radius = 1.0f;
    public static float g_inner_radius = 0.5f;
    public static float g_anim_scale = 1.0f;
    public static float g_v_ripple_frequency = 1.0f;
    public static float g_h_ripple_frequency = 1.0f;
    public static float g_v_ripple_frequency2 = 1.0f;
    public static float g_h_ripple_frequency2 = 1.0f;

	public enum CurveName {
		Wave,
		MultiWave,
		Ripple,
		Sphere,
		Torus,
	}

    private static CurveDelegate[] curvesList = { 
    	Wave, 
    	MultiWave, 
    	Ripple,
    	Sphere,
    	Torus,
    };

    public static Vector3 Wave(float u, float v, float t){
    	t *= g_anim_scale;

    	Vector3 p;
    	p.x = u;
    	p.y = Sin(PI * ((u+v)*g_frequency + t))*g_amplitude;
    	p.z = v;
		return p;
    }

    public static Vector3 MultiWave(float u, float v, float t){
    	t *= g_anim_scale;

    	Vector3 p;
    	p.x = u;
		p.y =         g_amplitude * Sin(PI * ((u + 0.5f)*g_frequency * t));
		p.y += 0.5f * g_amplitude * Sin(2f * PI * (v*g_frequency + t));
		p.y +=        g_amplitude * Sin(PI * ((u + v + 0.25f)*g_frequency * t));
		p.y *= 1f / 2.5f;
    	p.z = v;

		return p;
	}

	public static Vector3 Ripple(float u, float v, float t){
    	t *= g_anim_scale;

		float dist = Sqrt(u*u + v*v);
		Vector3 p;
    	p.x = u;
    	p.y = Sin(4f * PI * (-1.0f*dist*g_frequency + t))*g_amplitude;
    	p.y /= (1f + dist);
    	p.z = v;

		return p;
	}

	public static Vector3 Sphere(float u, float v, float t){
		// normalize uv coordinates to our graph
		u = u/GRAPH_SCALE;
		v = v/GRAPH_SCALE;

		float uRipple = u*g_h_ripple_frequency;
		float vRipple = v*g_v_ripple_frequency;
		float r = g_radius + g_anim_scale * Sin(PI * (uRipple + vRipple + t)*g_frequency);
		float s = r * Cos(PI * 0.5f * v);

		Vector3 p;
		p.x = s * Sin(PI * u);
		p.y = r * Sin(PI * 0.5f * v);
		p.z = s * Cos(PI * u);
		return p; // This is more like engine scale
	}

	public static Vector3 Torus(float u, float v, float t){
		// normalize uv coordinates to our graph
		u = u/GRAPH_SCALE;
		v = v/GRAPH_SCALE;

		float uRipple = u*g_h_ripple_frequency;
		float vRipple = v*g_v_ripple_frequency;
		float uRipple2 = u*g_h_ripple_frequency2;
		float vRipple2 = v*g_v_ripple_frequency2;
		float r1 = g_radius       + g_anim_scale * Sin(PI * (uRipple  + vRipple  + t) * g_frequency);
		float r2 = g_inner_radius + g_anim_scale*0.5f * Sin(PI * (uRipple2 + vRipple2 + 2f*t) * g_frequency);
		float s = r1 + r2 * Cos(PI * v);

		Vector3 p;
		p.x = s  * Sin(PI * u);
		p.y = r2 * Sin(PI * v);
		p.z = s  * Cos(PI * u);
		return p; // This is more like engine scale
	}

    public static Vector3 Morph(float u, float v, float t, Curve.CurveDelegate from, Curve.CurveDelegate to, float progress){
    	return Vector3.LerpUnclamped(from(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress));
    }

    public static void UpdateGlobalParameters(
    	float frequency, 
    	float amplitude, 
    	float radius,
    	float inner_radius,
    	float anim_scale,
		float v_ripple_frequency,
		float h_ripple_frequency,
		float v_ripple_frequency2,
		float h_ripple_frequency2
	){
    	g_frequency = frequency;
		g_amplitude = amplitude;
		g_radius = radius;
		g_inner_radius = inner_radius;
		g_anim_scale = anim_scale;
		g_v_ripple_frequency = v_ripple_frequency;
		g_h_ripple_frequency = h_ripple_frequency;
		g_v_ripple_frequency2 = v_ripple_frequency2;
		g_h_ripple_frequency2 = h_ripple_frequency2;
    }

    public delegate Vector3 CurveDelegate(float u, float v, float t);
	public static CurveDelegate CurveGetter(Curve.CurveName name){
		return curvesList[(int) name];
	}
}}

   