using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipplePlane : MonoBehaviour
{
    private int waveNumber { get; set; }
    public float[] waveAmplitude = new float[8];
    public MeshRenderer Renderer;
    public Vector2[] impactPositions = new Vector2[8];
    public float[] distance = new float[8];

    public void CardPlayed(Coordinate atCoordinate)
    {
        Vector3 center = atCoordinate.GetWorldspacePosition();
        waveNumber = (waveNumber + 1) % 8;
        waveAmplitude[waveNumber] = 1f;
        impactPositions[waveNumber] = center;
        distance[waveNumber] = 0;

        Renderer.material.SetFloat($"_OffsetX{waveNumber + 1}", -atCoordinate.GetWorldspacePosition().x * .5f);
        Renderer.material.SetFloat($"_OffsetZ{waveNumber + 1}", -atCoordinate.GetWorldspacePosition().y * .5f);
        Renderer.material.SetFloat($"_WaveAmplitude{waveNumber + 1}", waveAmplitude[waveNumber]);
        Renderer.material.SetFloat($"_xImpact{waveNumber + 1}", center.x);
        Renderer.material.SetFloat($"_zImpact{waveNumber + 1}", center.y);
        Renderer.material.SetFloat($"_Distance{waveNumber + 1}", 10);
    }

    private void Update()
    {
        for (int ii = 0; ii < waveAmplitude.Length; ii++)
        {
            if (waveAmplitude[ii] > 0)
            {
                distance[ii] = distance[ii] + Time.deltaTime;
                waveAmplitude[ii] = Mathf.Max(0, waveAmplitude[ii] - (Time.deltaTime * .25f));
                Renderer.material.SetFloat($"_WaveAmplitude{ii + 1}", waveAmplitude[ii]);
                Renderer.material.SetFloat($"_Distance{ii + 1}", 10); // distance[ii]);
            }
        }
    }
}
