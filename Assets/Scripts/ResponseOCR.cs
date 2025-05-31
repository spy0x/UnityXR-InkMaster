[System.Serializable]
public class OCRResponse
{
    public string detectedText;
    public float confidence;
}

[System.Serializable]
public class OCRResponseArray
{
    public OCRResponse[] responses;
}