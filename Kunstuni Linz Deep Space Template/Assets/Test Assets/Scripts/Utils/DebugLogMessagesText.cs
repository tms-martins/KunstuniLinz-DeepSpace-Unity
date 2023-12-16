/*
 * Tiago Martins 2023
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLogMessagesText : MonoBehaviour
{
    public Text uiText;
    public int maxMessages = 10;

    List<string> messages = new List<string>();

    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    public void ToggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        messages.Add($"[{type.ToString()}]: {message}");
        if (messages.Count > maxMessages)
        {
            messages.RemoveAt(0);
        }
        UpdateText();
    }

    public void LogSimpleMessage(string message)
    {
        LogMessage(message, "(simple message)", LogType.Log);
    }

    private void UpdateText()
    {
        string newText = "";
        int lineCount = 0;
        foreach(string message in messages)
        {
            if (lineCount++ > 0) newText += "\n";
            newText += message;
        }
        uiText.text = newText;
    }
}
