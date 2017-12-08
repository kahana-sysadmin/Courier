﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Environment
{
    public GameObject parent;
    public Store[] stores;
}

public class DeliveryExperiment : MonoBehaviour
{
    public delegate void StateChange(string stateName, bool on);
    public static StateChange OnStateChange;

    private static int sessionNumber = -1;
    private static bool useRamulator;

    private const int MICROPHONE_TEST_LENGTH = 5;
    private const string dboy_version = "v4.0";

    public MessageImageDisplayer messageImageDisplayer;
    public RamulatorInterface ramulatorInterface;
    public TextDisplayer textDisplayer;
    public SoundRecorder soundRecorder;
    public VideoControl videoPlayer;
    public ScriptedEventReporter scriptedEventReporter;
    public GameObject memoryWordCanvas;

    public AudioSource highBeep;
    public AudioSource lowBeep;
    public AudioSource lowerBeep;
    public GameObject microphoneTestMessage;
    public AudioSource microphoneTestPlayback;

    public Environment[] environments;

    public static void ConfigureExperiment(bool newUseRamulator, int newSessionNumber, string participantCode)
    {
        UnityEPL.AddParticipant(participantCode);
        UnityEPL.SetExperimentName("Delivery Boy");
        useRamulator = newUseRamulator;
        sessionNumber = newSessionNumber;
    }

	void Start ()
    {
        StartCoroutine(ExperimentCoroutine());
	}
	
	private IEnumerator ExperimentCoroutine()
    {
        if (sessionNumber == -1)
        {
            throw new UnityException("Please call ConfigureExperiment before beginning the experiment.");
        }

        //write versions to logfile
        Dictionary<string, object> versionsData = new Dictionary<string, object>();
        versionsData.Add("UnityEPL version", Application.version);
        versionsData.Add("Experiment version", dboy_version);
        versionsData.Add("Logfile version", "1");
        scriptedEventReporter.ReportScriptedEvent("versions", versionsData, 0);

        if (useRamulator)
            yield return ramulatorInterface.BeginNewSession(sessionNumber);

        yield return DoIntroductionVideo();
        yield return DoSubjectSessionQuitPrompt();
        yield return DoMicrophoneTest();

        memoryWordCanvas.SetActive(false);

        Environment environment = EnableEnvironment();

        yield return DoDeliveries(environment);
    }

    private IEnumerator DoDeliveries(Environment environment)
    {
        messageImageDisplayer.DisplayFindTheBlahMessage("library");

        for (int i = 0; i < environment.stores.Length; i++)
        {
            yield return null;
        }
    }

    private Environment EnableEnvironment()
    {
        System.Random reliable_random = new System.Random(UnityEPL.GetParticipants()[0].GetHashCode());
        Environment environment = environments[reliable_random.Next(environments.Length)];
        environment.parent.SetActive(true);
        return environment;
    }

    private IEnumerator DoSubjectSessionQuitPrompt()
    {
        yield return null;
        SetRamulatorState("WAITING", true, new Dictionary<string, object>());
        textDisplayer.DisplayText("subject/session confirmation", "Running " + UnityEPL.GetParticipants()[0] + " in session " + sessionNumber.ToString() + " of " + UnityEPL.GetExperimentName() + ".\n Press Y to continue, N to quit.");
        while (!Input.GetKeyDown(KeyCode.Y) && !Input.GetKeyDown(KeyCode.N))
        {
            yield return null;
        }
        textDisplayer.ClearText();
        SetRamulatorState("WAITING", false, new Dictionary<string, object>());
        if (Input.GetKey(KeyCode.N))
            Quit();
    }

    private IEnumerator DoMicrophoneTest()
    {
        microphoneTestMessage.SetActive(true);
        bool repeat = false;
        string wavFilePath;

        do
        {
            yield return PressAnyKey("Press any key to record a sound after the beep.");
            lowBeep.Play();
            textDisplayer.DisplayText("microphone test recording", "Recording...");
            textDisplayer.ChangeColor(Color.red);
            yield return new WaitForSeconds(lowBeep.clip.length);
            soundRecorder.StartRecording(MICROPHONE_TEST_LENGTH);
            yield return new WaitForSeconds(MICROPHONE_TEST_LENGTH);
            wavFilePath = System.IO.Path.Combine(UnityEPL.GetDataPath(), "microphone_test_" + DataReporter.RealWorldTime().ToString("yyyy-MM-dd_HH_mm_ss"));
            soundRecorder.StopRecording(wavFilePath);

            textDisplayer.DisplayText("microphone test playing", "Playing...");
            textDisplayer.ChangeColor(Color.green);

            microphoneTestPlayback.clip = soundRecorder.GetLastClip();
            microphoneTestPlayback.Play();
            yield return new WaitForSeconds(MICROPHONE_TEST_LENGTH);
            textDisplayer.ClearText();
            textDisplayer.OriginalColor();

            SetRamulatorState("WAITING", true, new Dictionary<string, object>());
            textDisplayer.DisplayText("microphone test confirmation", "Did you hear the recording? \n(Y=Continue / N=Try Again / C=Cancel).");
            while (!Input.GetKeyDown(KeyCode.Y) && !Input.GetKeyDown(KeyCode.N) && !Input.GetKeyDown(KeyCode.C))
            {
                yield return null;
            }
            textDisplayer.ClearText();
            SetRamulatorState("WAITING", false, new Dictionary<string, object>());
            if (Input.GetKey(KeyCode.C))
                Quit();
            repeat = Input.GetKey(KeyCode.N);
        }
        while (repeat);

        if (!System.IO.File.Exists(wavFilePath + ".wav"))
            yield return PressAnyKey("WARNING: Wav output file not detected.  Sounds may not be successfully recorded to disk.");

        microphoneTestMessage.SetActive(false);
    }

    private IEnumerator DoIntroductionVideo()
    {
        yield return PressAnyKey("Press any key to play movie.");

        bool replay = false;
        do
        {
            //start video player and wait for it to stop playing
            SetRamulatorState("INSTRUCT", true, new Dictionary<string, object>());
            videoPlayer.StartVideo();
            while (videoPlayer.IsPlaying())
                yield return null;
            SetRamulatorState("INSTRUCT", false, new Dictionary<string, object>());

            SetRamulatorState("WAITING", true, new Dictionary<string, object>());
            textDisplayer.DisplayText("repeat video prompt", "Press Y to continue to practice list, \n Press N to replay instructional video.");
            while (!Input.GetKeyDown(KeyCode.Y) && !Input.GetKeyDown(KeyCode.N))
            {
                yield return null;
            }
            textDisplayer.ClearText();
            SetRamulatorState("WAITING", false, new Dictionary<string, object>());
            replay = Input.GetKey(KeyCode.N);

        }
        while (replay);
    }

    private IEnumerator PressAnyKey(string displayText)
    {
        SetRamulatorState("WAITING", true, new Dictionary<string, object>());
        yield return null;
        textDisplayer.DisplayText("press any key prompt", displayText);
        while (!Input.anyKeyDown)
            yield return null;
        textDisplayer.ClearText();
        SetRamulatorState("WAITING", false, new Dictionary<string, object>());
    }

    //WAITING, INSTRUCT, COUNTDOWN, ENCODING, WORD, DISTRACT, RETRIEVAL
    private void SetRamulatorState(string stateName, bool state, Dictionary<string, object> extraData)
    {
        if (OnStateChange != null)
            OnStateChange(stateName, state);
        if (useRamulator)
            ramulatorInterface.SetState(stateName, state, extraData);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}