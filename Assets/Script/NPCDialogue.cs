using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [Header("Dialogue Lines — add as many as you want")]
    [TextArea(2, 5)]
    public string[] sentences;
    public float typeSpeed = 0.04f;

    [Header("Bubble — drag the child Canvas here")]
    public GameObject dialogueBubble;
    public TextMeshProUGUI dialogueText;

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (dialogueBubble != null) dialogueBubble.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractManager.Instance.RegisterInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        InteractManager.Instance.UnregisterInteractable(this);
        CloseDialogue();
    }

    // Called by InteractManager when shoot button is pressed near NPC
    public void OnInteract()
    {
        // If typing skip to end
        if (isTyping)
        {
            SkipTyping();
            return;
        }

        // Open bubble on first press
        if (!dialogueBubble.activeSelf)
        {
            currentIndex = 0;
            dialogueBubble.SetActive(true);
            ShowSentence(currentIndex);
            return;
        }

        // Next sentence
        currentIndex++;
        if (currentIndex < sentences.Length)
            ShowSentence(currentIndex);
        else
            CloseDialogue();
    }

    void ShowSentence(int index)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(sentences[index]));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = sentences[currentIndex];
        isTyping = false;
    }

    void CloseDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (dialogueBubble != null) dialogueBubble.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
        currentIndex = 0;
        isTyping = false;
    }
}