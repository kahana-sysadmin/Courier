﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public abstract class CoroutineExperiment : MonoBehaviour
{
    private const int MICROPHONE_TEST_LENGTH = 5;

    public SoundRecorder soundRecorder;
    public TextDisplayer textDisplayer;
    public VideoControl videoPlayer;
    public VideoSelector videoSelector;

    public GameObject titleMessage;
    public UnityEngine.UI.Text titleText;

    public AudioSource audioPlayback;
    public AudioSource highBeep;
    public AudioSource lowBeep;
    public AudioSource lowerBeep;

    protected abstract void SetRamulatorState(string stateName, bool state, Dictionary<string, object> extraData);

    protected IEnumerator DoSubjectSessionQuitPrompt(int sessionNumber, string message)
    {
        yield return null;
        SetRamulatorState("WAITING", true, new Dictionary<string, object>());
        textDisplayer.DisplayText("subject/session confirmation", message);
        while (!InputManager.GetKeyDown(KeyCode.Y) && !InputManager.GetKeyDown(KeyCode.N))
        {
            yield return null;
        }
        textDisplayer.ClearText();
        SetRamulatorState("WAITING", false, new Dictionary<string, object>());
        if (InputManager.GetKey(KeyCode.N))
            Quit();
    }

    protected IEnumerator DoMicrophoneTest(string title, string press_any_key, string recording, string playing, string confirmation)
    {
        DisplayTitle(title);
        bool repeat = false;
        string wavFilePath;

        do
        {
            yield return PressAnyKey(press_any_key);
            lowBeep.Play();
            textDisplayer.DisplayText("microphone test recording", recording);
            textDisplayer.ChangeColor(Color.red);
            yield return new WaitForSeconds(lowBeep.clip.length);
            wavFilePath = System.IO.Path.Combine(UnityEPL.GetDataPath(), "microphone_test_" + DataReporter.RealWorldTime().ToString("yyyy-MM-dd_HH_mm_ss") + ".wav");
            soundRecorder.StartRecording(wavFilePath);
            yield return new WaitForSeconds(MICROPHONE_TEST_LENGTH);

            audioPlayback.clip = soundRecorder.StopRecording();

            textDisplayer.DisplayText("microphone test playing", playing);
            textDisplayer.ChangeColor(Color.green);

            audioPlayback.Play();
            yield return new WaitForSeconds(MICROPHONE_TEST_LENGTH);
            textDisplayer.ClearText();
            textDisplayer.OriginalColor();

            SetRamulatorState("WAITING", true, new Dictionary<string, object>());
            textDisplayer.DisplayText("microphone test confirmation", confirmation);
            while (!InputManager.GetKeyDown(KeyCode.Y) && !InputManager.GetKeyDown(KeyCode.N) && !InputManager.GetKeyDown(KeyCode.C))
            {
                yield return null;
            }
            textDisplayer.ClearText();
            SetRamulatorState("WAITING", false, new Dictionary<string, object>());
            if (InputManager.GetKey(KeyCode.C))
                Quit();
            repeat = InputManager.GetKey(KeyCode.N);
        }
        while (repeat);

        if (!System.IO.File.Exists(wavFilePath))
            yield return PressAnyKey("WARNING: Wav output file not detected.  Sounds may not be successfully recorded to disk.");

        ClearTitle();
    }

    protected void DisplayTitle(string title)
    {
        titleMessage.SetActive(true);
        titleText.text = title;
    }

    protected void ClearTitle()
    {
        titleMessage.SetActive(false);
    }

    protected IEnumerator DoVideo(string playPrompt, string repeatPrompt, VideoSelector.VideoType videoType)
    {
        yield return PressAnyKey(playPrompt);

        bool replay = false;
        do
        {
            //start video player and wait for it to stop playing
            SetRamulatorState("INSTRUCT", true, new Dictionary<string, object>());
            videoSelector.SetIntroductionVideo(videoType);
            videoPlayer.StartVideo();
            while (videoPlayer.IsPlaying())
                yield return null;
            SetRamulatorState("INSTRUCT", false, new Dictionary<string, object>());

            SetRamulatorState("WAITING", true, new Dictionary<string, object>());
            textDisplayer.DisplayText("repeat video prompt", repeatPrompt);
            while (!InputManager.GetKeyDown(KeyCode.Y) && !InputManager.GetKeyDown(KeyCode.N))
            {
                yield return null;
            }
            textDisplayer.ClearText();
            SetRamulatorState("WAITING", false, new Dictionary<string, object>());
            replay = InputManager.GetKey(KeyCode.N);

        }
        while (replay);
    }

    protected IEnumerator PressAnyKey(string displayText)
    {
        SetRamulatorState("WAITING", true, new Dictionary<string, object>());
        yield return null;

        textDisplayer.DisplayText("press any key prompt", displayText);
        while (!InputManager.anyKeyDown)
            yield return null;

        textDisplayer.ClearText();
        SetRamulatorState("WAITING", false, new Dictionary<string, object>());
    }


    protected void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
