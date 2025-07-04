using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasPainting : MonoBehaviour
{
    [SerializeField] private Transform canvas;
    [SerializeField] Material drawingMaterial;
    [Range(0.01f, 0.1f)] [SerializeField] float penWidth = 0.01f;
    [SerializeField] Color penColor = Color.black;
    [SerializeField] private string canvasLayerMask;
    [SerializeField] private RenderTexture canvasRenderTexture;
    [SerializeField] private ImageSender imageSender;
    [SerializeField] private GameObject handObject;
    [SerializeField] private Transform fingerTip;
    [SerializeField] private GameObject brush;
    [SerializeField] private ParticleSystem brushHandParticleEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip brushHandAudioClip;
    [SerializeField] private float strokeTimeLimit = 0.2f;
    [SerializeField] private OVRPassthroughLayer passthroughLayer;
    [SerializeField] private float passthroughCanvasBrightness = -0.7f;
    [SerializeField] private float passthroughCanvasContrast = 0.5f;
    [SerializeField] private float passthroughCanvasSaturation = 1f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private ParticleSystem wrongParticleEffect;
    [SerializeField] private ParticleSystem correctParticleEffect;

    private LineRenderer currentLine;
    private int currentLineIndex = 0;
    private bool isHandOverCanvas = false;
    private bool hideHand = false;
    private float lastStrokeTime;

    public void OnCanvasSelected()
    {
        if (!hideHand) return;
        Vector3 pos = fingerTip.position;
        pos.z = canvas.position.z;
        currentLine = new GameObject("Line").AddComponent<LineRenderer>();
        currentLine.gameObject.layer = LayerMask.NameToLayer(canvasLayerMask);
        currentLine.alignment = LineAlignment.TransformZ;
        currentLine.numCornerVertices = 5;
        currentLine.numCapVertices = 5;
        currentLine.useWorldSpace = false;
        currentLine.transform.SetParent(canvas);
        currentLine.material = drawingMaterial;
        currentLine.startWidth = penWidth;
        currentLine.endWidth = penWidth;
        currentLine.startColor = penColor;
        currentLine.endColor = penColor;
        currentLine.positionCount = 1;
        currentLine.SetPosition(0, pos);
        lastStrokeTime = .0f;
    }

    private IEnumerator StartPassthroughEffects()
    {
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            passthroughLayer.colorMapEditorBrightness = Mathf.Lerp(0f, passthroughCanvasBrightness, alpha);
            passthroughLayer.colorMapEditorContrast = Mathf.Lerp(0f, passthroughCanvasContrast, alpha);
            passthroughLayer.colorMapEditorSaturation = Mathf.Lerp(0f, passthroughCanvasSaturation, alpha);
            yield return null;
        }
        passthroughLayer.colorMapEditorBrightness = passthroughCanvasBrightness;
        passthroughLayer.colorMapEditorContrast = passthroughCanvasContrast;
        passthroughLayer.colorMapEditorSaturation = passthroughCanvasSaturation;
    }

    private void Update()
    {
        if (!currentLine) return;
        lastStrokeTime += Time.deltaTime;
        Vector3 pos = fingerTip.position;
        pos.z = canvas.position.z;
        var currentPosition = currentLine.GetPosition(currentLineIndex);
        if (Vector3.Distance(currentPosition, pos) > 0.01f)
        {
            lastStrokeTime = 0.0f;
            if (!audioSource.isPlaying) audioSource.Play();
            currentLineIndex++;
            currentLine.positionCount = currentLineIndex + 1;
            currentLine.SetPosition(currentLineIndex, pos);
        } else if (lastStrokeTime > strokeTimeLimit)
        {
            // If the finger is not moving for a while, stop the audio
            audioSource.Stop();
        }
    }

    public void OnCanvasUnselected()
    {
        audioSource.Stop();
        currentLine = null;
        currentLineIndex = 0;
    }

    private IEnumerator StopPassthroughEffects()
    {
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            passthroughLayer.colorMapEditorBrightness = Mathf.Lerp(passthroughCanvasBrightness, 0f, alpha);
            passthroughLayer.colorMapEditorContrast = Mathf.Lerp(passthroughCanvasContrast, 0f, alpha);
            passthroughLayer.colorMapEditorSaturation = Mathf.Lerp(passthroughCanvasSaturation, 0f, alpha);
            yield return null;
        }
        passthroughLayer.colorMapEditorBrightness = 0f;
        passthroughLayer.colorMapEditorContrast = 0f;
        passthroughLayer.colorMapEditorSaturation = 0f;
    }

    // Called from Unity Wrapper Event ThumbsUp gameObject
    public void SendToServer()
    {
        // convert render texture to texture2D
        Texture2D texture = new Texture2D(canvasRenderTexture.width, canvasRenderTexture.height, TextureFormat.RGBA32,
            false);
        RenderTexture.active = canvasRenderTexture;
        texture.ReadPixels(new Rect(0, 0, canvasRenderTexture.width, canvasRenderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;
        // Save the texture2D to a file or send it to a server
        imageSender.SendImage(texture);
    }

    public void ClearCanvas()
    {
        foreach (Transform child in canvas)
        {
            Destroy(child.gameObject);
        }
    }

    public void UndoLastStroke()
    {
        if (canvas.childCount > 0)
        {
            Transform lastStroke = canvas.GetChild(canvas.childCount - 1);
            Destroy(lastStroke.gameObject);
        }
    }

    // Called from Unity Wrapper Event IndexFingerUp gameObject
    public void HideHand(bool state)
    {
        hideHand = state;
        SetHandDisplayState();
    }

    // Called from Unity Wrapper Event Hover CanvasPainting gameObject
    public void SetIsHandOverCanvas(bool state)
    {
        isHandOverCanvas = state;
        SetHandDisplayState();
    }

    private void SetHandDisplayState()
    {
        if (hideHand && isHandOverCanvas)
        {
            handObject.SetActive(false);
            brush.SetActive(true);
            PlayBrushHandsEffects();
            StartCoroutine(StartPassthroughEffects());
        }
        else
        {
            if (!handObject.activeSelf)
            {
                handObject.SetActive(true);
                PlayBrushHandsEffects();
                StartCoroutine(StopPassthroughEffects());
            }

            brush.SetActive(false);
        }
    }

    private void PlayBrushHandsEffects()
    {
        if (brushHandParticleEffect) brushHandParticleEffect.Play();
        if (audioSource && brushHandAudioClip) audioSource.PlayOneShot(brushHandAudioClip);
    }

    public void PlayParticleEffect(bool isCorrect)
    {
        if (isCorrect)
        {
            if (correctParticleEffect) correctParticleEffect.Play();
        }
        else
        {
            if (wrongParticleEffect) wrongParticleEffect.Play();
        }
    }
}