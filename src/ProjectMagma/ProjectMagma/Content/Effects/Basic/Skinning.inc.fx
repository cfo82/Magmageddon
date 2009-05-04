//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
//PositionAndNormal Skin4(const SkinVsInput input)
//{
    //PositionAndNormal  output = (PositionAndNormal) 0;
    //
    //// Since the weights need to add up to one, store 1.0 - (sum of the weights)
    //float lastWeight = 1.0;
    //float weight = 0;
    //// Apply the transforms for the first 3 weights
    //for (int i = 0; i < 2; ++i)
    //{
        //weight = input.weights[i];
        //lastWeight -= weight;
        //output.position     += mul( input.position, MatrixPalette[input.indices[i]]) * weight;
        //output.normal       += mul( input.normal, MatrixPalette[input.indices[i]]) * weight;
    //}
    //// Apply the transform for the last weight
    //output.position     += mul( input.position, MatrixPalette[input.indices[3]])*lastWeight;
    //output.normal       += mul( input.normal, MatrixPalette[input.indices[3]])*lastWeight;
    //return output;
//};

PositionAndNormal Skin4(const SkinVsInput input)
{
    PositionAndNormal  output = (PositionAndNormal) 0;
    
    //float4 input_position = mul(input.position,Local);
    float4 input_position = input.position;
    
    // Since the weights need to add up to one, store 1.0 - (sum of the weights)
    float lastWeight = 1.0;
    float weight = 0;
    // Apply the transforms for the first 3 weights
    for (int i = 0; i < 2; ++i)
    {
        weight = input.weights[i];
        lastWeight -= weight;
        output.position     += mul( input_position, MatrixPalette[input.indices[i]]) * weight;
        output.normal       += mul( input.normal, MatrixPalette[input.indices[i]]) * weight;
    }
    // Apply the transform for the last weight
    output.position     += mul( input_position, MatrixPalette[input.indices[3]])*lastWeight;
    output.normal       += mul( input.normal, MatrixPalette[input.indices[3]])*lastWeight;
    return output;
};