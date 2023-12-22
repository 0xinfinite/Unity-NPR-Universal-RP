#ifndef DOTS_DEFORMATIONS_INCLUDED
#define DOTS_DEFORMATIONS_INCLUDED

#if defined(_DOTS_DEFORMATION_ON)

struct DeformedVertexData
{
	float3 position;
	float3 normal;
	float3 tangent;
};
uniform StructuredBuffer<DeformedVertexData> _DeformedMeshData: register(t1);

void ComputeDeformedVertex(uint vertexId, out float3 p, out float3 n, float3 t)
{
	const DeformedVertexData vertexData = _DeformedMeshData[asuint(_ComputeMeshIndex) + vertexId];
	p = vertexData.position;
	n = vertexData.normal;
	t = vertexData.tangent;
}
//struct DeformedVertexData
//{
//    float3 Position;
//    float3 Normal;
//    float3 Tangent;
//};
//uniform StructuredBuffer<DeformedVertexData> _DeformedMeshData : register(t1);
//uniform StructuredBuffer<DeformedVertexData> _PreviousFrameDeformedMeshData;


//void ApplyDeformedVertexData(uint vertexID, out float3 positionOut, out float3 normalOut, out float3 tangentOut)
//{
//    //const uint4 materialProperty = asuint(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DotsDeformationParams, float4));
//    //const uint currentFrameIndex = materialProperty[2];
//    //const uint meshStartIndex = materialProperty[currentFrameIndex];

//    const DeformedVertexData vertexData = _DeformedMeshData[//meshStartIndex +
//vertexID];

//    positionOut = vertexData.Position;
//    normalOut = vertexData.Normal;
//    tangentOut = vertexData.Tangent;
//}

//void ApplyPreviousFrameDeformedVertexPosition(in uint vertexID, out float3 positionOS)
//{
//    //const uint4 materialProperty = asuint(UNITY_ACCESS_HYBRID_INSTANCED_PROP(_DotsDeformationParams, float4));
//    //const uint prevFrameIndex = materialProperty[2] ^ 1;
//    //const uint meshStartIndex = materialProperty[prevFrameIndex];

//    // If we have a valid index, fetch the previous frame position
//    // Index zero is reserved as 'uninitialized'.
//    if (meshStartIndex > 0)
//    {
//        positionOS = _PreviousFrameDeformedMeshData[meshStartIndex + vertexID].Position;
//    }
//    // Else grab the current frame position
//    else
//    {
//        const uint currentFrameIndex = materialProperty[2];
//        const uint currentFrameMeshStartIndex = materialProperty[currentFrameIndex];

//        positionOS = _DeformedMeshData[currentFrameMeshStartIndex + vertexID].Position;
//    }
//}
#endif //UNITY_DOTS_INSTANCING_ENABLED

#endif //DOTS_DEFORMATIONS_INCLUDED
