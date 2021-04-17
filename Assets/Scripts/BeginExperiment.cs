﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginExperiment : MonoBehaviour
{
    public UnityEngine.GameObject greyedOutButton;
    public UnityEngine.GameObject beginExperimentButton;
    public UnityEngine.GameObject loadingButton;
    public UnityEngine.GameObject finishedButton;
    public UnityEngine.GameObject languageMismatchButton;
    public UnityEngine.UI.InputField participantCodeInput;
    public UnityEngine.UI.Toggle useRamulatorToggle;
    public UnityEngine.UI.Text beginButtonText;
    public UnityEngine.UI.InputField sessionInput;

    private const string scene_name = "MainGame";

    private void OnEnable() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void Update()
    {
        
        if (DeliveryItems.ItemsExhausted())
        {
            beginExperimentButton.SetActive(false);
            finishedButton.SetActive(true);
        }
        else
        {
            finishedButton.SetActive(false);
        }
        if (LanguageMismatch())
        {
            beginExperimentButton.SetActive(false);
            languageMismatchButton.SetActive(true);
        }
        else
        {
            languageMismatchButton.SetActive(false);
        }
    }

    public void UpdateParticipant() {
        if (IsValidParticipantName(participantCodeInput.text))
        {
            UnityEPL.ClearParticipants();
            beginExperimentButton.SetActive(true);
            greyedOutButton.SetActive(false);
            int nextSessionNumber = NextSessionNumber();
            sessionInput.text = nextSessionNumber.ToString();
            beginButtonText.text = LanguageSource.GetLanguageString("begin session") + " " + nextSessionNumber.ToString();
        }
        else
        {
            greyedOutButton.SetActive(true);
            beginExperimentButton.SetActive(false);
        }
    }

    public void UpdateSession() {
        int session;
         
        if(System.Int32.TryParse(sessionInput.text, out session)) {
            beginButtonText.text = LanguageSource.GetLanguageString("begin session") + " " + session.ToString();
            UnityEPL.SetSessionNumber(session);
            beginExperimentButton.SetActive(true);
            greyedOutButton.SetActive(false);
        }
        else {
            greyedOutButton.SetActive(true);
            beginExperimentButton.SetActive(false);
        }
    }

    private string GetLanguageFilePath()
    {
        string dataPath = UnityEPL.GetParticipantFolder();
        System.IO.Directory.CreateDirectory(dataPath);
        string languageFilePath = System.IO.Path.Combine(dataPath, "language");
        if (!System.IO.File.Exists(languageFilePath))
            System.IO.File.Create(languageFilePath).Close();
        return languageFilePath;
    }

    private bool LanguageMismatch()
    {
        if (UnityEPL.GetParticipants()[0].Equals("unspecified_participant"))
            return false;
        if (System.IO.File.ReadAllText(GetLanguageFilePath()).Equals(""))
            return false;
        return !LanguageSource.current_language.ToString().Equals(System.IO.File.ReadAllText(GetLanguageFilePath()));
    }

    private void LockLanguage()
    {
        System.IO.File.WriteAllText(GetLanguageFilePath(), LanguageSource.current_language.ToString());
    }

    public void DoBeginExperiment()
    {
        if (!IsValidParticipantName(participantCodeInput.text)) {
            loadingButton.SetActive(false);
            greyedOutButton.SetActive(true);
            beginExperimentButton.SetActive(false);

            throw new UnityException("You are trying to start the experiment with an invalid participant name!");
        }

        UnityEPL.SetSessionNumber(NextSessionNumber());
        UnityEPL.AddParticipant(participantCodeInput.text);
        UnityEPL.SetExperimentName("DBOY1");

        LockLanguage();
        // JPB: TODO: Change true to useNcislToggle
        DeliveryExperiment.ConfigureExperiment(useRamulatorToggle.isOn, true, NextSessionNumber(), participantCodeInput.text);
        Debug.Log(useRamulatorToggle.isOn);
        SceneManager.LoadScene(scene_name);
    }

    private int NextSessionNumber()
    {
        string dataPath = UnityEPL.GetParticipantFolder();
		System.IO.Directory.CreateDirectory (dataPath);
        string[] sessionFolders = System.IO.Directory.GetDirectories(dataPath);
        int mostRecentSessionNumber = -1;
        foreach (string folder in sessionFolders)
        {
            int thisSessionNumber = -1;
            if (int.TryParse(folder.Substring(folder.LastIndexOf('_')+1), out thisSessionNumber) && thisSessionNumber > mostRecentSessionNumber)
                mostRecentSessionNumber = thisSessionNumber;
        }
        return mostRecentSessionNumber + 1;
    }

    private bool IsValidParticipantName(string name)
    {

        if (name.Length < 1) {
            return false;
        }
        return true;

        // bool isTest = name.Equals("TEST");
        // if (isTest)
        //     return true;

        // if (name.Length != 6)
        //     return false;

        // bool isValidRAMName = name[0].Equals('R') && name[1].Equals('1') && char.IsDigit(name[2]) && char.IsDigit(name[3]) && char.IsDigit(name[4]) && char.IsUpper(name[5]);
        // bool isValidSCALPName = char.IsUpper(name[0]) && char.IsUpper(name[1]) && char.IsUpper(name[2]) && char.IsDigit(name[3]) && char.IsDigit(name[4]) && char.IsDigit(name[5]);
        // Debug.Log(isValidSCALPName);
        // return isValidRAMName || isValidSCALPName;
    }
}