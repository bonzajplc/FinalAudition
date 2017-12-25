float4 quatMul1( in float4 a, in float4 b)
{
	return float4(
		+ a.x * b.w + a.y*b.z - a.z*b.y + a.w*b.x,
		- a.x * b.z + a.y*b.w + a.z*b.x + a.w*b.y,
		+ a.x * b.y - a.y*b.x + a.z*b.w + a.w*b.z,
		- a.x * b.x - a.y*b.y - a.z*b.z + a.w*b.w );
}

float4 quatMul2( in float4 p, in float4 q )
{
	float4 r;
	r.xyz = p.w * q.xyz + q.w * p.xyz + cross(p.xyz, q.xyz);
	r.w = p.w * q.w - dot(p.xyz, q.xyz);
	return r;
}

#ifndef __GLSL__
float4 quatMul3( in float4 a, in float4 b)
{
	//return float4(
	//+ a.x * b.w + a.y*b.z - a.z*b.y + a.w*b.x,
	//- a.x * b.z + a.y*b.w + a.z*b.x + a.w*b.y,
	//+ a.x * b.y - a.y*b.x + a.z*b.w + a.w*b.z,
	//- a.x * b.x - a.y*b.y - a.z*b.z + a.w*b.w );

	const float4 A = a.wxyz;
	const float4 B = b.wxyz;
	float4 t12m, t03;
	{
		float4 a1123 = A.yyzw;
		float4 a2231 = A.zzwy;
		float4 b1000 = B.yxxx;
		float4 b2312 = B.zwyz;

		float4 t12 = mad( a1123, b1000, a2231 * b2312 );
		t12m = float4( -t12.x, t12.yzw );
	}

	{
		float4 a3312 = A.wwyz;
		float4 b3231 = B.wzwy;
		float4 a0000 = A.xxxx;

		float4 t3 = a3312 * b3231;
		float4 t0 = a0000 * B;

		t03 = t0 - t3;
	}
	return ( t03 + t12m ).yzwx;
}
#endif // !__GLSL__

float4 picoQuatMul( in float4 a, in float4 b)
{
	// surprisingly, most basic and straightforward version compiles best with 2.0 pssl compiler
	//
	return quatMul1( a, b );
}

float3 picoQuatRotateVector1( float4 q, float3 vec )
{
	float4 v = float4( vec, 0 );
	float4 qv = picoQuatMul( q, v );
	float4 qConj = float4( -q.xyz, q.w );
	float4 qvqConj = picoQuatMul( qv, qConj );
	return qvqConj.xyz;
}

float3 picoQuatFastRotate( in float4 unitQuat, in float3 v ) // former fastRotate
{
	const float3 t = cross( unitQuat.xyz, v ) * 2.f;
	return v + unitQuat.w * t + cross( unitQuat.xyz, t );
}

float3 picoQuatRotateVector( float4 q, float3 vec )
{
	// turns out, that picoQuatRotateVector1 generates shortest and uses the least registers with 2.0 pssl compiler
	//
	return picoQuatRotateVector1( q, vec );
	//return picoQuatFastRotate( q, vec );
}

float4 picoQuatAxisAngleToQuat( in float3 axis, in float angle )
{
	float sine, cosine;
	sincos( angle*0.5, sine, cosine );
	return normalize( float4( axis * sine, cosine ) );
}

float4 picoQuatRotationX( in float radians )
{
	return picoQuatAxisAngleToQuat( float3(1, 0, 0), radians );
}

float4 picoQuatRotationY( in float radians )
{
	return picoQuatAxisAngleToQuat( float3(0, 1, 0), radians );
}

float4 picoQuatRotationZ( in float radians )
{
	return picoQuatAxisAngleToQuat( float3(0, 0, 1), radians );
}

float3 picoQuatTransform( in float4 unitQuat, in float3 translation, in float3 pt )
{
	return translation + picoQuatRotateVector( unitQuat, pt );
}

// Constructs quaternion that rotates vector "from" to vector "to".
// Watch out for degenerate case when dot(from, to) ~= -1.
// http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors
float4 picoQuatFromTwoVectorsUnsafe( float3 to, float3 from )
{
	float m = sqrt(2.0 + 2.0 * dot(from, to));
	float3 w = (1.0 / m) * cross(from, to);
	return float4(w.x, w.y, w.z, 0.5 * m);
}

// http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
float4 createBasisQuatZAxis( float3 from )
{
	if ( from.z < - 0.9999999f )
	{
		// Matrix3( Vector3( 0.f, -1.f, 0.f ), Vector3( -1.f, 0.f, 0.f ), n );
		// this quat corresponds to a matrix above
		//
		return float4(0.70710671, -0.70710671, 0.0, 0.0);
	}
	float m = sqrt(2.0 + 2.0 * from.z);
	float3 w = (1.0 / m) * float3(-from.y, from.x, 0.0);
	return float4(w.x, w.y, w.z, 0.5 * m);
}
