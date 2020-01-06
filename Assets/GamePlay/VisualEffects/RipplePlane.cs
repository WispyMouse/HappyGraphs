using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipplePlane : MonoBehaviour
{
    private int waveNumber { get; set; }
    public float[] waveAmplitude = new float[8];
    public MeshRenderer Renderer;
    public float[] impactPositionX = new float[8];
    public float[] impactPositionZ = new float[8];
    public float[] offsetPositionX = new float[8];
    public float[] offsetPositionZ = new float[8];
    public float[] distance = new float[8];

    public void CardPlayed(Coordinate atCoordinate)
    {
        Vector3 center = atCoordinate.GetWorldspacePosition();
        waveNumber = (waveNumber + 1) % 8;
        waveAmplitude[waveNumber] = 1f;
        offsetPositionX[waveNumber] = -center.x * .5f;
        offsetPositionZ[waveNumber] = -center.y * .5f;
        impactPositionX[waveNumber] = center.x;
        impactPositionZ[waveNumber] = center.y;
        distance[waveNumber] = 0;

        Renderer.material.SetFloatArray("_xOffset", offsetPositionX);
        Renderer.material.SetFloatArray("_zOffset", offsetPositionZ);
        Renderer.material.SetFloatArray("_xImpact", impactPositionX);
        Renderer.material.SetFloatArray("_zImpact", impactPositionZ);
        Renderer.material.SetFloatArray("_WaveAmplitude", waveAmplitude);
        Renderer.material.SetFloatArray("_Distance", distance);
    }

    private void Update()
    {
        bool anySet = false;

        for (int ii = 0; ii < waveAmplitude.Length; ii++)
        {
            if (waveAmplitude[ii] > 0)
            {
                anySet = true;
                distance[ii] = distance[ii] + Time.deltaTime;
                waveAmplitude[ii] = Mathf.Max(0, waveAmplitude[ii] - (Time.deltaTime * .25f));
            }
        }

        if (anySet)
        {
            Renderer.material.SetFloatArray("_xOffset", offsetPositionX);
            Renderer.material.SetFloatArray("_zOffset", offsetPositionZ);
            Renderer.material.SetFloatArray("_xImpact", impactPositionX);
            Renderer.material.SetFloatArray("_zImpact", impactPositionZ);
            Renderer.material.SetFloatArray("_WaveAmplitude", waveAmplitude);
            Renderer.material.SetFloatArray("_Distance", distance);
        }

    }
}
