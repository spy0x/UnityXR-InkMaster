using System;
using Meta.WitAi.TTS.Utilities;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum JapaneseCharacterType
{
    Hiragana,
    Katakana,
    Kanji
}

[System.Serializable]
public struct JapaneseCharacter
{
    public string romaji;
    public string japanese;
}

public class Trainer : MonoBehaviour
{
    [SerializeField] private JapaneseCharacterType characterType;
    [SerializeField] private JapaneseCharacter[] hiraganaCharacters;
    [SerializeField] private JapaneseCharacter[] katakanaCharacters;
    [SerializeField] private JapaneseCharacter[] kanjiCharacters;
    [SerializeField] private TextMeshProUGUI characterText;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float delayBeforeNextCharacter = 2f;
    [SerializeField] private CanvasPainting canvasPainting;
    [SerializeField] private TextMeshProUGUI japaneseCharacterText;
    [SerializeField] private TTSSpeaker ttsSpeaker;

    private string currentCharacter;
    private void Start()
    {
        characterText.text = "";
        japaneseCharacterText.text = "";
        SetCurrentCharacter();
    }

    private void SetCurrentCharacter()
    {
        canvasPainting.ClearCanvas();
        japaneseCharacterText.text = "";
        characterText.color = defaultColor; // Reset text color
        japaneseCharacterText.color = defaultColor; // Reset text color
        JapaneseCharacter[] characters =
            characterType switch
            {
                JapaneseCharacterType.Hiragana => hiraganaCharacters,
                JapaneseCharacterType.Katakana => katakanaCharacters,
                JapaneseCharacterType.Kanji => kanjiCharacters,
                _ => throw new ArgumentOutOfRangeException()
            };

        if (characters.Length == 0) return;

        JapaneseCharacter randomCharacter = characters[Random.Range(0, characters.Length)];
        characterText.text = randomCharacter.romaji;
        currentCharacter = randomCharacter.japanese;
        ttsSpeaker.Speak(randomCharacter.romaji);
    }
    public void CheckInput(OCRResponse input)
    {
        japaneseCharacterText.text = currentCharacter; // Show the correct character
        if (input.detectedText == currentCharacter)
        {
            characterText.color = correctColor;
            Color correctWithAlpha = correctColor;
            correctWithAlpha.a = 0.5f; // Make the text semi-transparent
            japaneseCharacterText.color = correctWithAlpha; // Set the color with transparency
            Invoke(nameof(SetCurrentCharacter), delayBeforeNextCharacter); // Set a new character after 1 second
        }
        else WrongInput();
    }
    public void WrongInput()
    {
        japaneseCharacterText.text = currentCharacter; // Show the correct character
        Color incorrectWithAlpha = incorrectColor;
        incorrectWithAlpha.a = 0.5f; // Make the text semi-transparent
        japaneseCharacterText.color = incorrectWithAlpha; // Set the color with transparency
        characterText.color = incorrectColor;
        Invoke(nameof(SetCurrentCharacter), delayBeforeNextCharacter);
    }
}