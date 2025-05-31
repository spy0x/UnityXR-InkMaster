using System;
using System.Collections.Generic;
using UnityEngine;

public class CanvasPainting : MonoBehaviour
{
    [SerializeField] private Transform canvas;
    [SerializeField] Material drawingMaterial;
    [Range(0.01f, 0.1f)] [SerializeField] float penWidth = 0.01f;
    [SerializeField] Color penColor = Color.black;

    private LineRenderer currentLine;
    private int currentLineIndex = 0;
    private Transform fingerTip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IndexTip"))
        {
            fingerTip = other.transform;
            Vector3 pos = other.transform.position;
            pos.z = canvas.position.z;
            currentLine = new GameObject("Line").AddComponent<LineRenderer>();
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IndexTip"))
        {
            currentLine = null;
            currentLineIndex = 0;
        }
    }
    
    public void ClearCanvas()
    {
        foreach (Transform child in canvas)
        {
            Destroy(child.gameObject);
        }
    }
}