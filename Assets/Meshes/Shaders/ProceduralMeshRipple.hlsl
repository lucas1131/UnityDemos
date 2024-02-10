void Ripple_float(
	float3 PositionIn,
	float3 Origin,
	float Period,
	float Speed,
	float Amplitude,
	out float3 PositionOut,
	out float3 NormalOut,
	out float3 TangentOut
){

	float3 p = PositionIn - Origin;
	float d = length(p);
	float f = 2.0 * PI * Period * (d - Speed * _Time.y);
	PositionOut = PositionIn + float3(0.0, Amplitude * sin(f), 0.0);

	float2 derivatives = (2.0 * PI * Amplitude * Period * cos(f) / max(d, 0.00001)) * p.xz;
	TangentOut = float3(1.0, derivatives.x, 0.0);
	NormalOut = cross(float3(0.0, derivatives.y, 1.0), TangentOut);
}

void Ripple_half(
	half3 PositionIn,
	half3 Origin,
	half Period,
	half Speed,
	half Amplitude,
	out half3 PositionOut,
	out half3 NormalOut,
	out half3 TangentOut
){
	half3 p = PositionIn - Origin;
	half d = length(p);
	half f = 2.0 * PI * Period * (d - Speed*_Time.y);

	PositionOut = PositionIn + half3(0.0, Amplitude * sin(f), 0.0);

	half2 derivatives = (2.0 * PI * Amplitude * Period * cos(f) / max(d, 0.00001)) * p.xz;
	TangentOut = half3(1.0, derivatives.x, 0.0);
	NormalOut = cross(half3(0.0, derivatives.y, 1.0), TangentOut);
}
