#if !defined(TESSELLATION_INCLUDED)
#define TESSELLATION_INCLUDED

#endif

//Hull program, tell tessellation the surface it should work with & provide data
[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]//clockwise
[UNITY_partitioning("integer")]//how it should cut up the patch
[UNITY_patchconstantfunc("MyPatchConstantFunction")]//invoked once per patch
void MyHullProgram(
    InputPatch<appdata, 3> patch,
    uint id : SV_OutputControlPointID) {

    return patch[id];
}

struct TessellationFactors {
    float edge[3] : SV_TessFactor;// 3 edge
    float inside : SV_InsideTessFactor;// inside
};

//patch "constant" func, 决定本patch如何细分
//invoked once per patch
TessellationFactors MyPatchConstantFunction(InputPatch<VertexData, 3> patch) {
    TessellationFactors f;
    f.edge[0] = 1;
    f.edge[1] = 1;
    f.edge[2] = 1;
    f.inside = 1;
    return f;
}

//tessellation
//决定如何细分，但不产生新顶点，但为它们提供重心坐标系下的坐标


#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) data.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

//domain
//根据tessellation提供的坐标得出新vertices
//invoked once per vertex
[UNITY_domain("tri")]
void MyDomainProgram(
    TessellationFactors factors,
    OutputPatch<appdata, 3> patch,
    float3 barycentricCoordinates : SV_DomainLocation
) {
    appdata data;//new vertex
    MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
    MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
    MY_DOMAIN_PROGRAM_INTERPOLATE(tanget)
    MY_DOMAIN_PROGRAM_INTERPOLATE(uv)
    //data.unity_InstanceID=patch[0].unity_InstanceID; 细分和GPU实例化不能同时使用！
}