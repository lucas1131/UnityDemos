using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using Shared;

using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using float3x3 = Unity.Mathematics.float3x3;
using float3x4 = Unity.Mathematics.float3x4;
using quaternion = Unity.Mathematics.quaternion;

using Random = UnityEngine.Random;

namespace Fractal {
public class Fractal : MonoBehaviour {

	struct FractalPiece {
		public float3 worldPosition;
		public quaternion rotation;
		public quaternion worldRotation;
		public float spinAngle;
		public float maxSagAngle;
	}

	// lookup tables
	static quaternion[] rotations = {
		quaternion.identity,
		quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
		quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
	};

    static readonly int baseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int matricesId = Shader.PropertyToID("_Matrices");
    static readonly int sequenceValuesId = Shader.PropertyToID("_SequenceValues");
    static readonly int fractalColor1Id = Shader.PropertyToID("_FractalColor1");
    static readonly int fractalColor2Id = Shader.PropertyToID("_FractalColor2");
    static readonly int leavesColor1Id = Shader.PropertyToID("_LeavesColor1");
    static readonly int leavesColor2Id = Shader.PropertyToID("_LeavesColor2");

    static MaterialPropertyBlock mPropertyBlock;
    static Bounds drawBounds;

	private const int N_CHILDREN = 5;

	[SerializeField] private Mesh mesh;
	[SerializeField] private Mesh leafMesh;
	[SerializeField] private Material material;

	private int _internal_prev_depth;
	[SerializeField, Range(3, 10)] private int depth = 4;
	[SerializeField, Range(-5f, 5f)] private float animScale = 1f;
	[OnChangedCall("RecreateFractal")] [SerializeField, Range(0f, 90f)] private float maxSag1 = 15f;
	[OnChangedCall("RecreateFractal")] [SerializeField, Range(0f, 90f)] private float maxSag2 = 25f;
	[SerializeField] private Gradient fractalGradient1, fractalGradient2;
	[SerializeField] private Gradient leavesGradient1, leavesGradient2;

	// NativeArray is unmanaged memory so we need to free it properly later
	private NativeArray<FractalPiece>[] pieces;
	private NativeArray<float3x4>[] transformMats;
	private ComputeBuffer[] transformMatsBuffer;
	private FractalPiece RootPiece { get => pieces[0][0]; set => pieces[0][0] = value; }
	private Vector4[] sequenceValues;

	void Awake(){
		_internal_prev_depth = depth;
	}

	void OnEnable(){
		pieces = new NativeArray<FractalPiece>[depth];
		transformMats = new NativeArray<float3x4>[depth];
		transformMatsBuffer = new ComputeBuffer[depth];
		sequenceValues = new Vector4[depth];
		int stride = 3*4*sizeof(float); // matrix size in bytes
		int depth_size = 1;
		for (int i = 0; i < pieces.Length; i++) {
			pieces[i] = new NativeArray<FractalPiece>(depth_size, Allocator.Persistent);
			transformMats[i] = new NativeArray<float3x4>(depth_size, Allocator.Persistent);
			transformMatsBuffer[i] = new ComputeBuffer(depth_size, stride);
			sequenceValues[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
			depth_size *= 5;
		}

		float scale = 1.0f;
		pieces[0][0] = CreateFractalPiece(0, 0, scale);
		for(int i = 1; i < pieces.Length; i++){
			scale *= 0.5f;
			for (int j = 0; j < pieces[i].Length; j++){
				pieces[i][j] = CreateFractalPiece(i, j%N_CHILDREN, scale);
			}
		}

		mPropertyBlock ??= new MaterialPropertyBlock();
	}

	void OnDisable(){
		for (int i = 0; i < transformMatsBuffer.Length; i++) {
			transformMatsBuffer[i].Release();
			pieces[i].Dispose();
			transformMats[i].Dispose();
		}
		pieces = null;
		transformMats = null;
		transformMatsBuffer = null;
		sequenceValues = null;
	}

	void OnValidate () {
		if (pieces != null && enabled && (_internal_prev_depth != depth)) {
			_internal_prev_depth = depth;
			OnDisable();
			OnEnable();
		}
	}

	public void RecreateFractal(){
		OnDisable();
		OnEnable();
	}

	void Update(){
		TransformFractalParallel();
		DrawFractalGPU();
	}

	FractalPiece CreateFractalPiece(int pieceDepth, int idx, float scale){
		return new FractalPiece() { 
			rotation = rotations[idx],
			maxSagAngle = radians(Random.Range(maxSag1, maxSag2)),
		};
	}

	void TransformFractalParallel(){
		float spinAngleDelta = animScale * Time.deltaTime;

		FractalPiece root = RootPiece;
		root.spinAngle += spinAngleDelta;
		root.worldRotation = transform.rotation * root.rotation * quaternion.Euler(0f, root.spinAngle, 0f);
		root.worldPosition = transform.position;
		RootPiece = root;

		float fractalScale = transform.lossyScale.x;
		float3x3 rotationAndScale = float3x3(root.worldRotation) * fractalScale;
		transformMats[0][0] = float3x4(
			rotationAndScale.c0, 
			rotationAndScale.c1, 
			rotationAndScale.c2, 
			root.worldPosition
		);

		float scale = fractalScale;
		JobHandle handle = default;
		for(int i = 1; i < pieces.Length; i++){
			scale *= 0.5f;
			
			handle = new UpdateFractalLevelJob {
				spinAngleDelta = spinAngleDelta,
				scale = scale,
				pieces = pieces[i],
				parents = pieces[i-1],
				transformMats = transformMats[i],
			}.ScheduleParallel(pieces[i].Length, 8, handle);

		}
		handle.Complete();

		for(int i = 1; i < pieces.Length; i++){
			transformMatsBuffer[i].SetData(transformMats[i]);
		}
	}

	void DrawFractalGPU(){
		float fractalScale = transform.lossyScale.x;
		RenderParams rp = new RenderParams(material);
        rp.worldBounds = new Bounds(RootPiece.worldPosition, 3f*fractalScale*Vector3.one); // fractal converge to 3 units in size
        rp.matProps = mPropertyBlock;
		for (int i = 0; i < transformMatsBuffer.Length; i++) {

			ComputeBuffer buffer = transformMatsBuffer[i];
			buffer.SetData(transformMats[i]);

			Mesh instanceMesh;
			float interpolator = i / (transformMatsBuffer.Length - 2f); // dont count leaves as depth
			Color instanceColor1, instanceColor2;
			bool isLeaf = (i == transformMatsBuffer.Length - 1);
			if(isLeaf) {
				interpolator = i / (transformMatsBuffer.Length - 1f);
				instanceColor1 = leavesGradient1.Evaluate(interpolator);
				instanceColor2 = leavesGradient2.Evaluate(interpolator);
				instanceMesh = leafMesh;
			} else {
				instanceColor1 = fractalGradient1.Evaluate(interpolator);
				instanceColor2 = fractalGradient2.Evaluate(interpolator);
				instanceMesh = mesh;
			}

			mPropertyBlock.SetBuffer(matricesId, buffer);
			mPropertyBlock.SetColor(fractalColor1Id, instanceColor1);
			mPropertyBlock.SetColor(fractalColor2Id, instanceColor2);
			mPropertyBlock.SetVector(sequenceValuesId, sequenceValues[i]);
	        
	        Graphics.RenderMeshPrimitives(rp, instanceMesh, 0, transformMatsBuffer[i].count);
		}
	}


	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously=true)]
	struct UpdateFractalLevelJob : IJobFor {
		public float spinAngleDelta;
		public float scale;
		public float sagginess;

		public NativeArray<FractalPiece> pieces;
		[ReadOnly] public NativeArray<FractalPiece> parents;
		[WriteOnly] public NativeArray<float3x4> transformMats;

		public void Execute(int idx){
			FractalPiece piece = pieces[idx];
			FractalPiece parent = parents[idx/5];

			// Rotation with sagging 
			float3 pieceUpAxis = mul(mul(parent.worldRotation, piece.rotation), up());
			float3 sagAxis = normalize(cross(up(), pieceUpAxis));

			quaternion baseRotation = parent.worldRotation;
			float sagMagnitude = length(sagAxis);
			if(sagMagnitude != 0) {
				sagAxis /= sagMagnitude;
				quaternion sagRotation = quaternion.AxisAngle(sagAxis, PI * piece.maxSagAngle * sagMagnitude);
				baseRotation = mul(sagRotation, parent.worldRotation);
			}
			piece.spinAngle += spinAngleDelta;
			piece.worldRotation = mul(baseRotation, mul(piece.rotation, quaternion.RotateY(piece.spinAngle)));
			
			// Position offset by scale
			float3 scaleOffset = float3(0f, 1.5f * scale, 0f);
			piece.worldPosition = parent.worldPosition + mul(piece.worldRotation, scaleOffset);

			float3x3 rotationAndScale = float3x3(piece.worldRotation) * scale;
			transformMats[idx] = float3x4(
				rotationAndScale.c0, 
				rotationAndScale.c1, 
				rotationAndScale.c2, 
				piece.worldPosition
			);
			pieces[idx] = piece;
		}
	}
}}
