using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ImageSender : MonoBehaviour
{
    [SerializeField] string serverUrl = "http://127.0.0.1:8000/ocr/"; // Your Django OCR API endpoint
    [SerializeField] Texture2D testImage; // The image to send

    private IEnumerator Start()
    {
        // Send the image when the script starts
        yield return new WaitForSeconds(1);
        SendImage(testImage);
    }

    // Call this function to send an image file
    public void SendImage(Texture2D image)
    {
        StartCoroutine(UploadImage(image));
    }

    private IEnumerator UploadImage(Texture2D image)
    {
        // Convert Texture2D to a PNG byte array
        byte[] imageBytes = image.EncodeToPNG();
        WWWForm form = new WWWForm();
        // Add the image as a form file
        form.AddField("name", "image");
        form.AddBinaryData("image", imageBytes, "image.png", "image/png");

        // Send the request
        using (var request = UnityWebRequest.Post(serverUrl, form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Image uploaded successfully!");
                Debug.Log($"Server Response: {request.downloadHandler.text}");
                // Get the JSON response
                string jsonResponse = request.downloadHandler.text;

                // Wrap the JSON array in a temporary object for JsonUtility
                string wrappedJson = $"{{\"responses\":{jsonResponse}}}";
                OCRResponseArray responseArray = JsonUtility.FromJson<OCRResponseArray>(wrappedJson);

                // Process each response
                foreach (var response in responseArray.responses)
                {
                    Debug.Log($"Detected Text: {response.detectedText}, Confidence: {response.confidence}");
                }
            }
            else
            {
                Debug.LogError($"Error uploading image: {request.error}");
            }
        }
    }
}