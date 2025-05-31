using System;
using System.Collections.Generic;
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

    private LineRenderer currentLine;
    private int currentLineIndex = 0;
    [SerializeField] private Transform fingerTip;

    public void OnCanvasSelected()
    {
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
    }

    private void Update()
    {
        if (!currentLine) return;
        Vector3 pos = fingerTip.position;
        pos.z = canvas.position.z;
        var currentPosition = currentLine.GetPosition(currentLineIndex);
        if (Vector3.Distance(currentPosition, pos) > 0.01f)
        {
            currentLineIndex++;
            currentLine.positionCount = currentLineIndex + 1;
            currentLine.SetPosition(currentLineIndex, pos);
        }
    }

    public void OnCanvasUnselected()
    {
        currentLine = null;
        currentLineIndex = 0;
        SendToServer();
    }

    private void SendToServer()
    {
        // convert render texture to texture2D
        Texture2D texture = new Texture2D(canvasRenderTexture.width, canvasRenderTexture.height, TextureFormat.RGBA32, false);
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
    
}