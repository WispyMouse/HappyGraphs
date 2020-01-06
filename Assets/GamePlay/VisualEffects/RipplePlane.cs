using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipplePlane : MonoBehaviour
{
    private int waveNumber { get; set; }
    public float[] waveAmplitude { get; set; } = new float[8];
    public MeshRenderer Renderer;

    public void CardPlayed(Coordinate atCoordinate)
    {
        Vector3 center = atCoordinate.GetWorldspacePosition();
        waveNumber = (waveNumber + 1) % 8;
        waveAmplitude[waveNumber] = 1f;

        Renderer.material.SetFloat($"_OffsetX{waveNumber + 1}", -atCoordinate.GetWorldspacePosition().x);
        Renderer.material.SetFloat($"_OffsetZ{waveNumber + 1}", -atCoordinate.GetWorldspacePosition().y);
        Renderer.material.SetFloat($"_WaveAmplitude{waveNumber + 1}", waveAmplitude[waveNumber]);
    }

    private void Update()
    {
        for (int ii = 0; ii < waveAmplitude.Length; ii++)
        {
            if (waveAmplitude[ii] > 0)
            {
                waveAmplitude[ii] = Mathf.Max(0, waveAmplitude[ii] - (Time.deltaTime * .25f));
                Renderer.material.SetFloat($"_WaveAmplitude{ii + 1}", waveAmplitude[ii]);
            }
        }
    }
}
