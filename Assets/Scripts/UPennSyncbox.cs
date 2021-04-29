using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class UPennSyncbox : EventLoop {
//Function from Corey's Syncbox plugin (called "ASimplePlugin")
	[DllImport ("ASimplePlugin")]
	private static extern IntPtr OpenUSB();

	[DllImport ("ASimplePlugin")]
	private static extern IntPtr CloseUSB();


	[DllImport ("ASimplePlugin")]
	private static extern float SyncPulse();

    private const int PULSE_START_DELAY = 1000; // ms
    private const int TIME_BETWEEN_PULSES_MIN = 800;
    private const int TIME_BETWEEN_PULSES_MAX = 1200;

    private volatile bool stopped = true;

    private System.Random rnd;
    
    // from editor
    public ScriptedEventReporter scriptedInput = null;

    public UPennSyncbox(ScriptedEventReporter reporter = null) {
        scriptedInput = reporter;
    }

    public bool Init() {
        IntPtr ptr = OpenUSB();

        // TODO: update plugin to improve this check
        if(Marshal.PtrToStringAuto(ptr) != "didn't open USB...") {
            rnd = new System.Random();
            StopPulse();
            StartLoop();

            Debug.Log("Successful UpennSyncbox Init");

            return true;
        }
        Debug.Log("Failed UPennSyncbox Init");
        return false;
    }

    public bool IsRunning() {
        return !stopped;
    }

    public void TestPulse() {
        if(!IsRunning()) {
            Do(new EventBase(StartPulse));
            DoIn(new EventBase(StopPulse), 8000);
        }
    }

    public void StartPulse() {
        StopPulse();
        stopped = false;
        DoIn(new EventBase(Pulse), PULSE_START_DELAY);
    }

	private void Pulse ()
    {
		if(!stopped)
        {
            Debug.Log("Pew!");
            // Send a pulse
            if(scriptedInput != null)
                scriptedInput.ReportScriptedEvent("syncPulse", new System.Collections.Generic.Dictionary<string, object>());

            SyncPulse();

            // Wait a random interval between min and max
            int timeBetweenPulses = (int)(TIME_BETWEEN_PULSES_MIN + (int)(rnd.NextDouble() * (TIME_BETWEEN_PULSES_MAX - TIME_BETWEEN_PULSES_MIN)));
            DoIn(new EventBase(Pulse), timeBetweenPulses);
		}
	}

    public void StopPulse() {
        StopTimers();
        stopped = true;
    }

    public void OnDisable() {
        StopPulse();
        CloseUSB();
        StopLoop();
    }
}