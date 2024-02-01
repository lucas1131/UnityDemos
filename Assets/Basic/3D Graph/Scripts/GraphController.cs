using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph {
public class GraphController : MonoBehaviour {
	[SerializeField] private IGraphCycleCurve graphCycler;
    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Return)){
        	graphCycler.NextCurve();
        }
    }

    public void SetGraph(IGraphCycleCurve graph){
    	graphCycler = graph;
    }
}}
