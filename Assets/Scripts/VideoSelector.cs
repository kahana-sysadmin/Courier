﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoSelector : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public UnityEngine.Video.VideoClip englishIntro;
    public UnityEngine.Video.VideoClip germanIntro;
    public UnityEngine.Video.VideoClip englishEfrIntro;
    public UnityEngine.Video.VideoClip germanEfrIntro;
    public UnityEngine.Video.VideoClip englishNewEfrIntro;
    public UnityEngine.Video.VideoClip germanNewEfrIntro;
    public UnityEngine.Video.VideoClip niclsEnglishIntro;
    public UnityEngine.Video.VideoClip[] niclsMovie;

    void OnEnable()
    {
        if (videoPlayer.clip == null)
            Debug.Log("VideoSelector::OnEnable - SetIntroductionVideo was " +
                      "not called before OnEnable");

        videoPlayer.Play();
    }

    public enum VideoType
    {
        MainIntro,
        EfrIntro,
        NewEfrIntro,
        NiclsMainIntro,
        NiclsMovie
    }

    public void SetIntroductionVideo(VideoType videoType, int videoIndex = 0)
    {
        switch (videoType)
        {
            // JPB: TODO: Refactor this to make movies an array of language options
            case VideoType.MainIntro:
                if (LanguageSource.current_language == LanguageSource.LANGUAGE.GERMAN)
                    videoPlayer.clip = germanIntro;
                else
                    videoPlayer.clip = englishIntro;
                break;
            case VideoType.EfrIntro:
                if (LanguageSource.current_language == LanguageSource.LANGUAGE.GERMAN)
                    videoPlayer.clip = germanEfrIntro;
                else
                    videoPlayer.clip = englishEfrIntro;
                break;
            case VideoType.NewEfrIntro:
                if (LanguageSource.current_language == LanguageSource.LANGUAGE.GERMAN)
                    videoPlayer.clip = germanNewEfrIntro;
                else
                    videoPlayer.clip = englishNewEfrIntro;
                break;
            case VideoType.NiclsMainIntro:
                videoPlayer.clip = niclsEnglishIntro;
                break;
            case VideoType.NiclsMovie:
                videoPlayer.clip = niclsMovie[videoIndex];
                break;
            default: break;
        }
    }
}
