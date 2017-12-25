#define EULER		2.71828182845904523536
#define LOG2E		1.44269504088896340736
#define LOG10E		0.434294481903251827651
#define LN2			0.693147180559945309417
#define LN10		2.30258509299404568402
#define PI			3.1415926535897932384626433
#define PI_2		1.57079632679489661923
#define PI_4		0.785398163397448309616
#define INV_PI		0.318309886183790671538
#define INV_2PI		0.15915494309189533577
#define INV_4PI		0.07957747154594766788
#define SQRT2		1.41421356237309504880
#define SQRT3		1.732050807568877293528
#define TAU			(2*PI)
#define PHI			(sqrt(5)*0.5 + 0.5)

#define max3(x, y, z) ( max(max(x, y), z) )
#define min3(x, y, z) ( min(min(x, y), z) )
#define med3(x, y, z) ( max(min(x, y), min(max(x, y), z)) ) // 

float square(float x)
{
	return x*x;
}

float2 square(float2 x)
{
	return x*x;
}

float3 square(float3 x)
{
	return x*x;
}

float4 square(float4 x)
{
	return x*x;
}

float lengthSquared(float3 x)
{
	return dot(x, x);
}

float lengthSquared(float2 x)
{
	return dot(x, x);
}

float lengthPow(float3 x, float exponent )
{
	return pow(length(x), exponent);
}

// [Low-Level Thinking in High-Level Shading Languages]
// Remaps x from range <s0, e0> to range <s1, e1>. If ranges are constant, compiler should optimize it to two mads (+ potential movs).
float remapRange(float x, float s0, float e0, float s1, float e1)
{
	float slope0 = 1.0f/(e0-s0);
	float slope1 = (e1-s1);
	float offsetAtZero = -s0/(e0-s0);
	return (x * slope0 + offsetAtZero) * slope1 + s1;
}

float2 remapRange(float2 x, float2 s0, float2 e0, float2 s1, float2 e1)
{
	float2 slope0 = 1.0f/(e0-s0);
	float2 slope1 = (e1-s1);
	float2 offsetAtZero = -s0/(e0-s0);
	return (x * slope0 + offsetAtZero) * slope1 + s1;
}

float3 remapRange(float3 x, float3 s0, float3 e0, float3 s1, float3 e1)
{
	float3 slope0 = 1.0f/(e0-s0);
	float3 slope1 = (e1-s1);
	float3 offsetAtZero = -s0/(e0-s0);
	return (x * slope0 + offsetAtZero) * slope1 + s1;
}

float4 remapRange(float4 x, float4 s0, float4 e0, float4 s1, float4 e1)
{
	float4 slope0 = 1.0f/(e0-s0);
	float4 slope1 = (e1-s1);
	float4 offsetAtZero = -s0/(e0-s0);
	return (x * slope0 + offsetAtZero) * slope1 + s1;
}

float remapRangePrecomputed(float x, float slope0, float slope1, float offsetAtZero, float s1)
{
	return (x * slope0 + offsetAtZero) * slope1 + s1;
}

// http://iquilezles.org/www/articles/functions/functions.htm
float almostIdentity( float x, float m, float n )
{
	if ( x > m )
		return x;

	const float a = 2.0*n - m;
	const float b = 2.0*m - 3.0*n;
	const float t = x/m;

	return (a*t + b)*t*t + n;
}

float lumaBT709(float3 rgb)
{
	return dot(rgb, float3(0.2126, 0.7152, 0.0722));
}

float lumaBT601(float3 rgb)
{
	return dot(rgb, float3(0.2990, 0.5870, 0.1140));
}

float lumaHSP(float3 rgb)
{
	return sqrt( lumaBT601(square(rgb)) );
}

uint3 index1Dto3D(uint i, uint2 dimensionsXY)
{
	return uint3(
		i % dimensionsXY.x,
		(i / dimensionsXY.x) % dimensionsXY.y,
		i / (dimensionsXY.x * dimensionsXY.y));
}

float picoDot43( float4 a, float3 b )
{
	float r = a.x * b.x + a.w;
	r = a.y * b.y + r;
	r = a.z * b.z + r;
	return r;
}

float picoDot33( float3 a, float3 b )
{
	float r = a.x * b.x;
	r = a.y * b.y + r;
	r = a.z * b.z + r;
	return r;
}

// 3FR + 1QR (add, mac, rcp, mul) instead of 1FR + 2QR for normal pow (log, exp, mul).
float powFast( float x, float y )
{
	return x / (y - x*y + x);
}

// http://en.wikipedia.org/wiki/Dirac_delta_function
float dirac( float x, float a )
{
	//float y = exp( -(x*x)/(a*a) ) / (a * sqrt(3.14));
	float oneByA = 1.0 / a;
	float oneByASq = - oneByA * oneByA;
	float oneBySqrtPi = 1.0 / 1.772453851; // sqrt(PI) == 1.772453851f
	float denom = oneByA * oneBySqrtPi;
	float y = exp( x*x*oneByASq ) * denom;
	return y;
}

float linearstep( in float a, in float b, in float t )
{
	const float tClamped = max( a, min( b, t ) );
	return ( tClamped - a ) * rcp( b - a );
}

float cubicpulse( float center, float width, float x )
{
	x = abs( x - center );
	const float x1 = x / width;
	return ( x > width ) ? 0.f : 1.0f - x1*x1*( 3.0f - 2.0f*x1 );
}

float2 mirrorUV( float2 uv )
{
	uv = abs(uv);
	float2 fractional = frac(uv);
	return uv % 2.0 > 1.0 ? 1.0 - fractional : fractional;
}

float3 mirrorUV( float3 uvw )
{
	uvw = abs(uvw);
	float3 fractional = frac(uvw);
	return uvw % 2.0 > 1.0 ? 1.0 - fractional : fractional;
}

float3 planesIntersect( float4 p1, float4 p2, float4 p3 )
{
	float denom = dot( p1.xyz, cross( p2.xyz, p3.xyz ) );
	return rcp( -denom ) * (
			cross( p2.xyz, p3.xyz ) * p1.w +
			cross( p3.xyz, p1.xyz ) * p2.w +
			cross( p1.xyz, p2.xyz ) * p3.w );
}

// TODO: Optimize.
bool raySphereIntersectFull(float3 rayOrigin, float3 rayDir, float3 position, float radiusSquared, in out float t)
{
	bool r = false;
	float3 d = rayOrigin - position;
	float a = dot(rayDir, rayDir);
	float b = 2 * dot(rayDir, d);
	float c = dot(d, d) - radiusSquared;

	float discr = b*b - 4*a*c;
	if (discr >= 0)
	{
		float sqrtDiscr = sqrt(discr);
		float q = b > 0 ? -0.5 * (b+sqrtDiscr) : -0.5 * (b-sqrtDiscr);
		float tmp0 = q / a;
		float tmp1 = c / q;
		float t0 = min(tmp0, tmp1);
		float t1 = max(tmp0, tmp1);
		t = t0 > 0 ? t0 : t1;
		r = true;
	}
	return r;
}

// Doesn't properly handle rays originating from inside the sphere.
bool raySphereIntersect(float3 rayOrigin, float3 rayDir, float3 position, float radius, out float t)
{
	bool r = false;
	float3 d = rayOrigin - position;
	float  b = dot(rayDir, d);
	float  c = dot(d, d) - radius*radius;
	t = b*b-c;
	if (t > 0.0)
	{
		t = sqrt(t);
		t = -b-t > 0.0 ? -b-t : -b+t;
		r = (t > 0.0);
	}
	return r;
}

bool raySphereIntersect(float3 rayOrigin, float3 rayDir, float3 position, float radius)
{
	float t;
	return raySphereIntersect(rayOrigin, rayDir, position, radius, t);
}

float3 rayPlaneIntersect(float3 rayOrigin, float3 rayDir, float3 planeOrigin, float3 planeNormal)
{
    float t = dot(planeNormal, planeOrigin - rayOrigin) / dot(planeNormal, rayDir);
    return rayOrigin + rayDir * t;
}

float3 rayPlaneIntersect(float3 rayOrigin, float3 rayDir, float3 planeOrigin, float3 planeNormal, out float t)
{
    t = dot(planeNormal, planeOrigin - rayOrigin) / dot(planeNormal, rayDir);
    return rayOrigin + rayDir * t;
}

// From Bart.
float bilateralBlurQuadSwizzle(float key, float signal, float weightingMultiplier)
{
#if __PSSL__
	float otherSignal[3];
	float keyDiff[3];

	keyDiff[0] = saturate(1.0 - weightingMultiplier * abs(QuadSwizzle(key,1,0,3,2)-key));
	keyDiff[1] = saturate(1.0 - weightingMultiplier * abs(QuadSwizzle(key,2,3,0,1)-key));
	keyDiff[2] = saturate(1.0 - weightingMultiplier * abs(QuadSwizzle(key,3,2,1,0)-key));

	otherSignal[0] = QuadSwizzle(signal,1,0,3,2);
	otherSignal[1] = QuadSwizzle(signal,2,3,0,1);
	otherSignal[2] = QuadSwizzle(signal,3,2,1,0);

	float finalVal = signal;
	finalVal += keyDiff[0] * otherSignal[0];
	finalVal += keyDiff[1] * otherSignal[1];
	finalVal += keyDiff[2] * otherSignal[2];
	return finalVal * rcp(1.0f + keyDiff[0] + keyDiff[1] + keyDiff[2]);
#else
	return signal;
#endif
}

#ifdef __PIXEL__
float bilateralBlurDDXDDY(int2 positionSS, float key, float signal, float weightingMultiplier)
{
	float w1 = saturate(1.0 - weightingMultiplier * abs(ddx(key)));
	float w2 = saturate(1.0 - weightingMultiplier * abs(ddy(key)));

	float diff1 = ddx(signal) * ((positionSS.x & 1) - 0.5);
	float diff2 = ddy(signal) * ((positionSS.y & 1) - 0.5);

	float finalVal = signal;
	finalVal -= diff1 * w1;
	finalVal -= diff2 * w2;
	return finalVal;
}
#endif //

const float4 makePlane( float3 planeNormal, float3 pointOnPlane )
{
	return float4( planeNormal, -dot( planeNormal, pointOnPlane ) );
}

void extractAABBCorners( out float3 p[8], float3 minc, float3 maxc )
{
	p[0] = float3( minc.x, minc.y, minc.z );
	p[1] = float3( maxc.x, minc.y, minc.z );
	p[2] = float3( maxc.x, maxc.y, minc.z );
	p[3] = float3( minc.x, maxc.y, minc.z );

	p[4] = float3( minc.x, minc.y, maxc.z );
	p[5] = float3( maxc.x, minc.y, maxc.z );
	p[6] = float3( maxc.x, maxc.y, maxc.z );
	p[7] = float3( minc.x, maxc.y, maxc.z );
}

float4 findMin( float4 p[8] )
{
	float4 min_0 = min3( p[0], p[1], p[2] );
	float4 min_1 = min3( p[3], p[4], p[5] );
	float4 min_2 = min( p[6], p[7] );
	float4 min_all = min3( min_0, min_1, min_2 );
	return min_all;
}

float4 findMax( float4 p[8] )
{
	float4 max_0 = max3( p[0], p[1], p[2] );
	float4 max_1 = max3( p[3], p[4], p[5] );
	float4 max_2 = max( p[6], p[7] );
	float4 max_all = max3( max_0, max_1, max_2 );
	return max_all;
}

float3 findMin( float3 p[8] )
{
	float3 min_0 = min3( p[0], p[1], p[2] );
	float3 min_1 = min3( p[3], p[4], p[5] );
	float3 min_2 = min( p[6], p[7] );
	float3 min_all = min3( min_0, min_1, min_2 );
	return min_all;
}

float3 findMax( float3 p[8] )
{
	float3 max_0 = max3( p[0], p[1], p[2] );
	float3 max_1 = max3( p[3], p[4], p[5] );
	float3 max_2 = max( p[6], p[7] );
	float3 max_all = max3( max_0, max_1, max_2 );
	return max_all;
}

float3 RGBToYCoCg( float3 RGB )
{
	float Y  = dot( RGB, float3(  1, 2,  1 ) ) * 0.25;
	float Co = dot( RGB, float3(  2, 0, -2 ) ) * 0.25 + ( 0.5 * 256.0 / 255.0 );
	float Cg = dot( RGB, float3( -1, 2, -1 ) ) * 0.25 + ( 0.5 * 256.0 / 255.0 );
	
	float3 YCoCg = float3( Y, Co, Cg );
	return YCoCg;
}

float3 YCoCgToRGB( float3 YCoCg )
{
	float Y  = YCoCg.x;
	float Co = YCoCg.y - ( 0.5 * 256.0 / 255.0 );
	float Cg = YCoCg.z - ( 0.5 * 256.0 / 255.0 );

	float R = Y + Co - Cg;
	float G = Y + Cg;
	float B = Y - Co - Cg;

	float3 RGB = float3( R, G, B );
	return RGB;
}
