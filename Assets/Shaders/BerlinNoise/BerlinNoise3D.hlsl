#ifndef BERLINNOISE3D_INCLUDED
#define BERLINNOISE3D_INCLUDED

float hash(float3 p)
{
    p = frac(p * 0.3183099 + float3(0.1, 0.1, 0.1));
    p *= 17.0;
    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float3 rotate(float3 p)
{
    // 3D rotation matrix or cheap axis shuffle
    return float3(p.x + p.z, p.y + p.x, p.z + p.y);
}


float noise3D(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    //float3 u = f * f * (3.0 - 2.0 * f);
    float3 u = f*f*f; // Linear interpolation instead of smoothstep

    
    return lerp(
        lerp(
            lerp(hash(i + float3(0, 0, 0)), hash(i + float3(1, 0, 0)), u.x),
            lerp(hash(i + float3(0, 1, 0)), hash(i + float3(1, 1, 0)), u.x),
            u.y
        ),
        lerp(
            lerp(hash(i + float3(0, 0, 1)), hash(i + float3(1, 0, 1)), u.x),
            lerp(hash(i + float3(0, 1, 1)), hash(i + float3(1, 1, 1)), u.x),
            u.y
        ),
        u.z
    );
}

void BerlinNoise3D_float(float3 Position, float Size, out float output)
{
    //output = 0.0;
    
    // Stretch coordinates to distort shape (more linear)
    //float3 distortedPos = float3(Position.x, Position.y * 0.5, Position.z * 0.2);
    float3 distortedPos = rotate(Position * Size);


    // Control frequency of spots
    float noiseVal = noise3D(distortedPos * Size);

    float wearBias = 1.0 - (Position.y * 0.08); // tweakable

    output = noiseVal * wearBias;
}

#endif
