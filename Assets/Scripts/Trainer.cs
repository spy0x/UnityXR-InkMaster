using System;
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

    private string currentCharacter;
    private Color defaultTextColor;
    private void Start()
    {
        characterText.text = "";
        SetCurrentCharacter();
    }

    private void SetCurrentCharacter()
    {
        canvasPainting.ClearCanvas();
        characterText.color = defaultTextColor; // Reset text color
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
    }
    public void CheckInput(OCRResponse input)
    {
        if (input.detectedText == currentCharacter)
        {
            characterText.color = correctColor;
            Invoke(nameof(SetCurrentCharacter), delayBeforeNextCharacter); // Set a new character after 1 second
        }
        else WrongInput();
    }
    public void WrongInput()
    {
        characterText.color = incorrectColor;
        Invoke(nameof(SetCurrentCharacter), delayBeforeNextCharacter);
    }
}