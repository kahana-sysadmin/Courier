﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Environment
{
    public GameObject parent;
    public StoreComponent[] stores;
}

public class DeliveryExperiment : CoroutineExperiment
{
    public delegate void StateChange(string stateName, bool on);
    public static StateChange OnStateChange;

    private static int sessionNumber = -1;
    private static bool useRamulator;
    private static bool useNicls;

    // JPB: TODO: Make this a configuration variable
    private static bool standaloneTesting = true; // JPB: TODO: Change to false

    private const string DBOY_VERSION = "v4.1.2";
    private const string RECALL_TEXT = "*******";
    private const int DELIVERIES_PER_TRIAL = 13;
    private const float MIN_FAMILIARIZATION_ISI = 0.4f;
    private const float MAX_FAMILIARIZATION_ISI = 0.6f;
    private const float FAMILIARIZATION_PRESENTATION_LENGTH = 1.5f;
    private const float RECALL_MESSAGE_DISPLAY_LENGTH = 6f;
    private const float RECALL_TEXT_DISPLAY_LENGTH = 1f;
    private const float FREE_RECALL_LENGTH = 30f;
    private const float STORE_FINAL_RECALL_LENGTH = 90f;
    private const float FINAL_RECALL_LENGTH = 240f;
    private const float TIME_BETWEEN_DIFFERENT_RECALL_PHASES = 2f;
    private const float CUED_RECALL_TIME_PER_STORE = 5f;
    private const float CUED_RECALL_ISI = 1f;
    private const float ARROW_CORRECTION_TIME = 3f;

    public Camera regularCamera;
    public Camera blackScreenCamera;
    public Familiarizer familiarizer;
    public MessageImageDisplayer messageImageDisplayer;
    public RamulatorInterface ramulatorInterface;
    public NiclsInterface niclsInterface;
    public PlayerMovement playerMovement;
    public GameObject pointer;
    public ParticleSystem pointerParticleSystem;
    public GameObject pointerMessage;
    public UnityEngine.UI.Text pointerText;
    public StarSystem starSystem;
    public DeliveryItems deliveryItems;
    public Pauser pauser;

    public float pointerRotationSpeed = 10f;

    public ScriptedEventReporter scriptedEventReporter;
    public GameObject memoryWordCanvas;

    public Environment[] environments;

    private List<StoreComponent> this_trial_presented_stores = new List<StoreComponent>();
    private List<string> all_presented_objects = new List<string>();

    private Syncbox syncs;

    public static void ConfigureExperiment(bool newUseRamulator, bool newUseNicls, int newSessionNumber, string participantCode)
    {
        useRamulator = newUseRamulator;
        useNicls = newUseNicls;
        sessionNumber = newSessionNumber;
    }

	void Update()
	{
		Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
		// need to do this because macos thinks it knows better than you do
	}

	void Start ()
    {
        if(UnityEPL.viewCheck) {
            return;
        }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = 300;
        Cursor.SetCursor(new Texture2D(0,0), new Vector2(0,0), CursorMode.ForceSoftware);
        QualitySettings.vSyncCount = 1;

        // Start syncpulses
        standaloneTesting = true;
        if (!standaloneTesting)
        {
            syncs = GameObject.Find("SyncBox").GetComponent<Syncbox>();
            syncs.StartPulse();
        }

        Dictionary<string, object> sceneData = new Dictionary<string, object>();
        sceneData.Add("sceneName", "MainGame");
        scriptedEventReporter.ReportScriptedEvent("loadScene", sceneData);

		StartCoroutine(ExperimentCoroutine());
	}

	private IEnumerator ExperimentCoroutine()
    {
        if (sessionNumber == -1)
        {
            throw new UnityException("Please call ConfigureExperiment before beginning the experiment.");
        }

        // TODO: log scene changed

        //write versions to logfile
        LogVersions();

        if (useRamulator)
            yield return ramulatorInterface.BeginNewSession(sessionNumber);

        //yield return niclsInterface.Test();

        useNicls = true;
        if (useNicls)
            yield return niclsInterface.BeginNewSession(sessionNumber);

        BlackScreen();
        yield return DoIntroductionVideo(LanguageSource.GetLanguageString("play movie"), LanguageSource.GetLanguageString("first day"));
        yield return DoSubjectSessionQuitPrompt(sessionNumber,
                                                LanguageSource.GetLanguageString("running participant"));
        yield return DoMicrophoneTest(LanguageSource.GetLanguageString("microphone test"),
                                      LanguageSource.GetLanguageString("after the beep"),
                                      LanguageSource.GetLanguageString("recording"),
                                      LanguageSource.GetLanguageString("playing"),
                                      LanguageSource.GetLanguageString("recording confirmation"));

        yield return DoFamiliarization();

        yield return messageImageDisplayer.DisplayLanguageMessage(messageImageDisplayer.delivery_restart_messages);



        Environment environment = EnableEnvironment();

        Dictionary<string, object> storeMappings = new Dictionary<string, object>();
        foreach (StoreComponent store in environment.stores)
        {
            // old name : new name
            storeMappings.Add(store.gameObject.name, store.GetStoreName());
            //storeMappings.Add(store.GetStoreName(), store.gameObject.name);
            storeMappings.Add(store.GetStoreName() + " position X", store.transform.position.x);
            storeMappings.Add(store.GetStoreName() + " position Y", store.transform.position.y);
            storeMappings.Add(store.GetStoreName() + " position Z", store.transform.position.z);
        }
        scriptedEventReporter.ReportScriptedEvent("store mappings", storeMappings);

        int trial_number = 0;
        for (trial_number = 0; trial_number < 12; trial_number++)
        {
            Dictionary<string, object> trialData = new Dictionary<string, object>();
            trialData.Add("trial number", trial_number);
            scriptedEventReporter.ReportScriptedEvent("begin new trial", trialData);
            WorldScreen();
            if (useRamulator)
                ramulatorInterface.BeginNewTrial(trial_number);
            yield return null;
            yield return DoDelivery(environment, trial_number);

            BlackScreen();
            yield return DoRecall(trial_number);

            SetRamulatorState("WAITING", true, new Dictionary<string, object>());
            yield return null;
            if (!DeliveryItems.ItemsExhausted())
            {
                textDisplayer.DisplayText("proceed to next day prompt", LanguageSource.GetLanguageString("next day"));
                while (!Input.GetButton("q (secret)") && !Input.GetButton("x (continue)"))
                    yield return null;
                
                textDisplayer.ClearText();
                if (Input.GetButton("q (secret)"))
                    break;
            }
            else
            {
                yield return PressAnyKey(LanguageSource.GetLanguageString("final recall"));
                break;
            }
            SetRamulatorState("WAITING", false, new Dictionary<string, object>());
        }

        yield return messageImageDisplayer.DisplayLanguageMessage(messageImageDisplayer.final_recall_messages);
        yield return DoFinalRecall(environment);

        //int delivered_objects = trial_number == 12 ? (trial_number) * 12 : (trial_number + 1) * 12;
        textDisplayer.DisplayText("end text", LanguageSource.GetLanguageString("end message") + starSystem.CumulativeRating().ToString("+#.##;-#.##") );
    }

    private void LogVersions()
    {
        Dictionary<string, object> versionsData = new Dictionary<string, object>();
        versionsData.Add("UnityEPL version", Application.version);
        versionsData.Add("Experiment version", DBOY_VERSION);
        versionsData.Add("Logfile version", "1");
        scriptedEventReporter.ReportScriptedEvent("versions", versionsData);
    }

    private void BlackScreen()
    {
        pauser.ForbidPausing();
        memoryWordCanvas.SetActive(true);
        regularCamera.enabled = false;
        blackScreenCamera.enabled = true;
        starSystem.gameObject.SetActive(false);
        playerMovement.Freeze();
    }

    private void WorldScreen()
    {
        pauser.AllowPausing();
        regularCamera.enabled = true;
        blackScreenCamera.enabled = false;
        starSystem.gameObject.SetActive(true);
        memoryWordCanvas.SetActive(false);
        playerMovement.Zero();
    }


    private IEnumerator DoRecall(int trial_number)
    {
        SetRamulatorState("RETRIEVAL", true, new Dictionary<string, object>());

        yield return DoFreeRecal(trial_number);

        yield return DoCuedRecall(trial_number);

        SetRamulatorState("RETRIEVAL", false, new Dictionary<string, object>());
    }


    private IEnumerator DoFreeRecal(int trial_number)
    {
        textDisplayer.DisplayText("display day objects recall prompt", LanguageSource.GetLanguageString("day objects recall"));
        yield return SkippableWait(RECALL_MESSAGE_DISPLAY_LENGTH);
        textDisplayer.ClearText();
        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });
        textDisplayer.DisplayText("display recall text", RECALL_TEXT);
        yield return SkippableWait(RECALL_TEXT_DISPLAY_LENGTH);
        textDisplayer.ClearText();

        string output_directory = UnityEPL.GetDataPath();
        string wavFilePath = System.IO.Path.Combine(output_directory, trial_number.ToString()) + ".wav";
        Dictionary<string, object> recordingData = new Dictionary<string, object>();
        recordingData.Add("trial number", trial_number);
        scriptedEventReporter.ReportScriptedEvent("object recall recording start", recordingData);
        soundRecorder.StartRecording(wavFilePath);
        yield return SkippableWait(FREE_RECALL_LENGTH);

        scriptedEventReporter.ReportScriptedEvent("object recall recording stop", recordingData);
        soundRecorder.StopRecording();
        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });
    }

    private IEnumerator DoCuedRecall(int trial_number)
    {
        this_trial_presented_stores.Shuffle(new System.Random());

        textDisplayer.DisplayText("display day cued recall prompt", LanguageSource.GetLanguageString("store cue recall"));
        yield return SkippableWait(RECALL_MESSAGE_DISPLAY_LENGTH);
        textDisplayer.ClearText();
        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });
        textDisplayer.DisplayText("display recall text", RECALL_TEXT);
        yield return SkippableWait(RECALL_TEXT_DISPLAY_LENGTH);
        textDisplayer.ClearText();
        foreach (StoreComponent cueStore in this_trial_presented_stores)
        {
            WaitUntilWithTimeout waitForClassifier = new WaitUntilWithTimeout(niclsInterface.classifierReady, 5);
            yield return waitForClassifier;
            if (waitForClassifier.timedOut())
            {
                Debug.Log("Classifier wait timed out");
                // JPB: TODO: Send message back to NICLServer
            }

            cueStore.familiarization_object.SetActive(true);
            string output_file_name = trial_number.ToString() + "-" + cueStore.GetStoreName();
            string output_directory = UnityEPL.GetDataPath();
            string wavFilePath = System.IO.Path.Combine(output_directory, output_file_name) + ".wav";
            string lstFilepath = System.IO.Path.Combine(output_directory, output_file_name) + ".lst";
            AppendWordToLst(lstFilepath, cueStore.GetLastPoppedItemName());
            Dictionary<string, object> cuedRecordingData = new Dictionary<string, object>();
            cuedRecordingData.Add("trial number", trial_number);
            cuedRecordingData.Add("store", cueStore.GetStoreName());
            cuedRecordingData.Add("item", cueStore.GetLastPoppedItemName());
            cuedRecordingData.Add("store position", cueStore.transform.position.ToString());
            scriptedEventReporter.ReportScriptedEvent("cued recall recording start", cuedRecordingData);
            soundRecorder.StartRecording(wavFilePath);
            yield return SkippableWait(CUED_RECALL_TIME_PER_STORE);
            cueStore.familiarization_object.SetActive(false);
            scriptedEventReporter.ReportScriptedEvent("cued recall recording stop", cuedRecordingData);
            soundRecorder.StopRecording();


            lowBeep.Play();
            scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", highBeep.clip.length.ToString() } });
            textDisplayer.DisplayText("display recall text", RECALL_TEXT);
            yield return SkippableWait(RECALL_TEXT_DISPLAY_LENGTH);
            textDisplayer.ClearText();
        }
    }

    private IEnumerator DoFinalRecall(Environment environment)
    {
        SetRamulatorState("RETRIEVAL", true, new Dictionary<string, object>());

        DisplayTitle(LanguageSource.GetLanguageString("all stores recall"));

        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });
        textDisplayer.DisplayText("display recall text", RECALL_TEXT);
        yield return SkippableWait(RECALL_TEXT_DISPLAY_LENGTH);
        textDisplayer.ClearText();

        string output_directory = UnityEPL.GetDataPath();
        string output_file_name = "store recall";
        string wavFilePath = System.IO.Path.Combine(output_directory, output_file_name) + ".wav";
        string lstFilepath = System.IO.Path.Combine(output_directory, output_file_name) + ".lst";
        foreach (StoreComponent store in environment.stores)
            AppendWordToLst(lstFilepath, store.GetStoreName());

        scriptedEventReporter.ReportScriptedEvent("final store recall recording start", new Dictionary<string, object>());
        soundRecorder.StartRecording(wavFilePath);
        yield return SkippableWait(STORE_FINAL_RECALL_LENGTH);

        scriptedEventReporter.ReportScriptedEvent("final store recall recording stop", new Dictionary<string, object>());
        soundRecorder.StopRecording();
        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });

        ClearTitle();

        yield return SkippableWait(TIME_BETWEEN_DIFFERENT_RECALL_PHASES);

        DisplayTitle(LanguageSource.GetLanguageString("all objects recall"));

        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });
        textDisplayer.DisplayText("display recall text", RECALL_TEXT);
        yield return SkippableWait(RECALL_TEXT_DISPLAY_LENGTH);
        textDisplayer.ClearText();

        output_file_name = "final recall";
        wavFilePath = System.IO.Path.Combine(output_directory, output_file_name) + ".wav";
        lstFilepath = System.IO.Path.Combine(output_directory, output_file_name) + ".lst";
        foreach (string deliveredObject in all_presented_objects)
            AppendWordToLst(lstFilepath, deliveredObject);

        scriptedEventReporter.ReportScriptedEvent("final object recall recording start", new Dictionary<string, object>());
        soundRecorder.StartRecording(wavFilePath);
        yield return SkippableWait(FINAL_RECALL_LENGTH);
        scriptedEventReporter.ReportScriptedEvent("final object recall recording stop", new Dictionary<string, object>());
        soundRecorder.StopRecording();

        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });

        ClearTitle();
        SetRamulatorState("RETRIEVAL", false, new Dictionary<string, object>());
    }

    private IEnumerator DoFamiliarization()
    {
        yield return messageImageDisplayer.DisplayLanguageMessage(messageImageDisplayer.store_images_presentation_messages);
        yield return familiarizer.DoFamiliarization(MIN_FAMILIARIZATION_ISI, MAX_FAMILIARIZATION_ISI, FAMILIARIZATION_PRESENTATION_LENGTH);
    }

    private IEnumerator DoDelivery(Environment environment, int trialNumber)
    {
        SetRamulatorState("ENCODING", true, new Dictionary<string, object>());
        WaitUntilWithTimeout waitForClassifier = new WaitUntilWithTimeout(niclsInterface.classifierReady, 5);
        yield return waitForClassifier;
        if (waitForClassifier.timedOut())
        {
            Debug.Log("Classifier wait timed out");
            // JPB: TODO: Send message back to NICLServer
        }
        messageImageDisplayer.please_find_the_blah_reminder.SetActive(true);

        this_trial_presented_stores = new List<StoreComponent>();
        List<StoreComponent> unvisitedStores = new List<StoreComponent>(environment.stores);
        for (int i = 0; i < DELIVERIES_PER_TRIAL; i++)
        {
            StoreComponent nextStore = null;
            int random_store_index = -1;
            int tries = 0;

            do
            {
                tries++;
                random_store_index = Random.Range(0, unvisitedStores.Count);
                nextStore = unvisitedStores[random_store_index];
            }
            while (nextStore.IsVisible() && tries < 17);

            unvisitedStores.RemoveAt(random_store_index);


            playerMovement.Freeze();
            messageImageDisplayer.please_find_the_blah_reminder.SetActive(false);
            messageImageDisplayer.SetReminderText(nextStore.GetStoreName());
            yield return DoPointingTask(nextStore);
            messageImageDisplayer.please_find_the_blah_reminder.SetActive(true);
            playerMovement.Unfreeze();

            while (!nextStore.PlayerInDeliveryPosition())
            {
                yield return null;
            }

            ///AUDIO PRESENTATION OF OBJECT///
            if (i != DELIVERIES_PER_TRIAL - 1)
            {
                playerMovement.Freeze();
                AudioClip deliveredItem = nextStore.PopItem();
                string deliveredItemName = deliveredItem.name;
                audioPlayback.clip = deliveredItem;
                audioPlayback.Play();
                scriptedEventReporter.ReportScriptedEvent("object presentation begins",
                                                          new Dictionary<string, object>() { {"trial number", trialNumber},
                                                                                             {"item name", deliveredItemName},
                                                                                             {"store name", nextStore.GetStoreName()},
                                                                                             {"serial position", i+1},
                                                                                             {"player position", playerMovement.transform.position.ToString()},
                                                                                             {"store position", nextStore.transform.position.ToString()}});
                AppendWordToLst(System.IO.Path.Combine(UnityEPL.GetDataPath(), trialNumber.ToString() + ".lst"), deliveredItemName);
                this_trial_presented_stores.Add(nextStore);
                all_presented_objects.Add(deliveredItemName);
                SetRamulatorState("WORD", true, new Dictionary<string, object>() { { "word", deliveredItemName} });
                yield return SkippableWait(deliveredItem.length);
                SetRamulatorState("WORD", false, new Dictionary<string, object>() { { "word", deliveredItemName } });
                scriptedEventReporter.ReportScriptedEvent("audio presentation finished",
                                                          new Dictionary<string, object>());
                playerMovement.Unfreeze();
            }
        }

        messageImageDisplayer.please_find_the_blah_reminder.SetActive(false);
        SetRamulatorState("ENCODING", false, new Dictionary<string, object>());
    }

    private void ColorPointer(Color color)
    {
        foreach (Renderer eachRenderer in pointer.GetComponentsInChildren<Renderer>())
            eachRenderer.material.SetColor("_Color", color);
    }

    private IEnumerator DoPointingTask(StoreComponent nextStore)
    {
        pointer.SetActive(true);
        ColorPointer(new Color(0.5f, 0.5f, 1f));
        pointer.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        scriptedEventReporter.ReportScriptedEvent("pointing begins", new Dictionary<string, object>() { {"start direction", pointer.transform.eulerAngles.y}, {"store", nextStore.GetStoreName() } });
        pointerMessage.SetActive(true);
        pointerText.text = LanguageSource.GetLanguageString("next package prompt") +
                           LanguageSource.GetLanguageString(nextStore.GetStoreName()) + ". " +
                           LanguageSource.GetLanguageString("please point") +
                           LanguageSource.GetLanguageString(nextStore.GetStoreName()) + "." + "\n\n" + 
                           LanguageSource.GetLanguageString("joystick");
        yield return null;
        while (!Input.GetButtonDown("x (continue)"))
        {
            if (!playerMovement.IsDoubleFrozen())
                pointer.transform.eulerAngles = pointer.transform.eulerAngles + new Vector3(0, Input.GetAxis("Horizontal") * Time.deltaTime * pointerRotationSpeed, 0);
            yield return null;
        }

        float pointerError = PointerError(nextStore.gameObject);
        if (pointerError < Mathf.PI / 12)
        {
            pointerParticleSystem.Play();
            pointerText.text = LanguageSource.GetLanguageString("correct to within") + Mathf.RoundToInt(pointerError * Mathf.Rad2Deg).ToString() + ". ";
        }
        else
        {
            pointerText.text = LanguageSource.GetLanguageString("wrong by") + Mathf.RoundToInt(pointerError * Mathf.Rad2Deg).ToString() + ". ";
        }

        float wrongness = pointerError / Mathf.PI;
        ColorPointer(new Color(wrongness, 1 - wrongness, .2f));
        bool improvement = starSystem.ReportScore(1 - wrongness);

        if (improvement)
        {
            pointerText.text = pointerText.text + LanguageSource.GetLanguageString("rating improved");
        }


        yield return null;
        yield return PointArrowToStore(nextStore.gameObject);
        while (!Input.GetButtonDown("x (continue)"))
        {
            yield return null;
        }
        scriptedEventReporter.ReportScriptedEvent("pointer message cleared", new Dictionary<string, object>());
        pointerParticleSystem.Stop();
        pointer.SetActive(false);
        pointerMessage.SetActive(false);
    }

    private float PointerError(GameObject toStore)
    {
        Vector3 lookDirection = toStore.transform.position - pointer.transform.position;
        float correctYRotation = Quaternion.LookRotation(lookDirection).eulerAngles.y;
        float actualYRotation = pointer.transform.eulerAngles.y;
        float offByRads = Mathf.Abs(correctYRotation - actualYRotation) * Mathf.Deg2Rad;
        if (offByRads > Mathf.PI)
            offByRads = Mathf.PI * 2 - offByRads;

        scriptedEventReporter.ReportScriptedEvent("pointing finished", new Dictionary<string, object>() { {"correct direction (degrees)", correctYRotation},
                                                                                                          {"pointed direction (degrees)", actualYRotation} });

        return offByRads;
    }

    private IEnumerator PointArrowToStore(GameObject pointToStore)
    {
        float rotationSpeed = 1f;
        float startTime = Time.time;
        Vector3 lookDirection = pointToStore.transform.position - pointer.transform.position;
        while (Time.time < startTime + ARROW_CORRECTION_TIME)
        {
            pointer.transform.rotation = Quaternion.Slerp(pointer.transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * rotationSpeed);
            yield return null;
        }
    }

    private void AppendWordToLst(string lstFilePath, string word)
    {
        System.IO.FileInfo lstFile = new System.IO.FileInfo(lstFilePath);
        bool firstLine = !lstFile.Exists;
        if (firstLine)
            lstFile.Directory.Create();
        lstFile.Directory.Create();
        using (System.IO.StreamWriter w = System.IO.File.AppendText(lstFilePath))
        {
            if (!firstLine)
                w.Write(System.Environment.NewLine);
            w.Write(word);
        }
    }

    private Environment EnableEnvironment()
    {
        System.Random reliable_random = new System.Random(UnityEPL.GetParticipants()[0].GetHashCode());
        Environment environment = environments[reliable_random.Next(environments.Length)];
        environment.parent.SetActive(true);
        return environment;
    }

    //WAITING, INSTRUCT, COUNTDOWN, ENCODING, WORD, DISTRACT, RETRIEVAL
    protected override void SetRamulatorState(string stateName, bool state, Dictionary<string, object> extraData)
    {
        if (OnStateChange != null)
            OnStateChange(stateName, state);
        if (useRamulator)
            ramulatorInterface.SetState(stateName, state, extraData);
    }

    private IEnumerator SkippableWait(float waitTime)
    {
        float startTime = Time.time;
        while (Time.time < startTime + waitTime)
        {
            if (Input.GetButtonDown("q (secret)"))
                break;
            yield return null;
        }
    }

    public string GetStoreNameFromGameObjectName(string gameObjectName)
    {
        foreach (StoreComponent store in environments[0].stores)
            if (store.gameObject.name.Equals(gameObjectName))
                return store.GetStoreName();
        throw new UnityException("That store game object doesn't exist in the stores list.");
    }
}

public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts, System.Random random)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = random.Next(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}
