﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LanguageSource
{
    public enum LANGUAGE { ENGLISH, GERMAN };
    public static LANGUAGE current_language;

    private const string GERMAN_TRANSLATION_NEEDED = "GERMAN TRANSLATION NEEDED";

    private static Dictionary<string, string[]> language_string_dict = new Dictionary<string, string[]>()
    {
        {"", new string[] {"", ""}},

        { "final recall", new string[] {"All items exhausted. Press any key to proceed to final recall.", "Alle Gegenstände ausgeliefert. Weiter mit beliebiger Taste."} },
        { "store cue recall", new string[] {"Please recall which object you delivered to the store shown on the screen.", "Bitte nennen Sie den Gegenstand, den Sie zu dem dargestellten Geschäft geliefert haben."} },
        { "day objects recall", new string[] {"After the beep, please recall all objects from this delivery day.", "Nach dem Piepton, erinnern Sie bitte alle Gegenstände, die Sie in dieser Runde zugestellt haben."} },
        { "microphone test", new string[] {"Microphone Test", "Mikrofontest"} },
        { "next package prompt", new string[] {"The next package has to be delivered to the ", "Als nächstes beliefern Sie "} },
        { "rating improved", new string[] {"Your rating improved!", "Ihre Wertung hat sich verbessert!"} },
        //{ "you now have", new string[] {"You now have points: ", "Aktuelle Punktzahl: "} },
        //{ "you earn points", new string[] {"You earned points: ", "Verdiente Punkte: "} },
        { "continue", new string[] {"Press (X) to continue.", "Drücken Sie (X) um fortzufahren."} },
        { "please point", new string[] {"Please point to the ", "Bitte richten Sie den Pfeil aus auf "} },
        { "joystick", new string[] {"Use the joystick to adjust the arrow, then press (X) to continue.", "Nutzen Sie den Joystick um den Pfeil zu rotieren und (X) um zu bestätigen."} },
        { "wrong by", new string[] {"Not quite. The arrow will now show the exact direction. That was off by degrees: ", "Nicht ganz! Der Pfeil zeigt Ihnen nur die richtige Richtung. Abweichung in Grad zur korrekten Antwort: "} },
        { "correct to within", new string[] {"Good! That was correct to within degrees: ", "Fast perfekt! Abweichung in Grad zur korrekten Antwort: "} },
        { "all objects recall", new string[] {"Please recall all the objects that you delivered.", "Bitte erinnern Sie alle Gegenstände, die Sie zugestellt haben."} },
        { "all stores recall", new string[] {"Please recall all the stores that you delivered objects to.", "Bitte erinnern Sie alle Geschäfte, zu denen Sie Pakete geliefert haben."} },
        { "end message", new string[] {"Thank you for being a great delivery person! Your cumulative score is: ", "Vielen Dank für Ihre Teilnahme! Ihre abschließende Wertung ist: "} },

        { "first video", new string[] {"Press (Y) to continue, \n Press (N) to replay instructional video.",
                                       "Drücken Sie (Y) um fortzufahren, \n Drücken Sie (N) um das Video noch einmal zu sehen."} },
        //{ "first practice day", new string[] {"Press (X) to proceed to the first practice delivery day.", 
        //                                      "Drücken Sie (X) um die erste trainier Auslieferungsrunde zu starten."} },
        { "next practice day video", new string[] {"Press (Y) to continue to the next practice delivery day, \n Press (N) to replay instructional video.", 
                                                   "Drücken Sie (Y) um die nächste trainier Auslieferungsrunde zu starten, \n Drücken Sie (N) um das Video noch einmal zu sehen."} },
        { "next practice day", new string[] {"Press (X) to proceed to the next practice delivery day.",
                                             "Drücken Sie (X) um die nächste trainier Auslieferungsrunde zu starten."} },
        
        //{ "first day", new string[] {"Press (X) to proceed to the first delivery day.", "Drücken Sie (X) um die erste Auslieferungsrunde zu starten."} },
        { "next day", new string[] {"Press (X) to proceed to the next delivery day.", "Drücken Sie (X) um die nächste Auslieferungsrunde zu starten."} },

        { "first day main", new string [] {"Let’s start the first delivery day!", GERMAN_TRANSLATION_NEEDED}},
        { "first day description", new string [] {"Don’t forget to continue pressing the left/right buttons when recalling items at the end of each delivery day.",
                                                  GERMAN_TRANSLATION_NEEDED}},

        // JPB: TODO: NEEDS GERMAN
        { "efr left button correct message", new string [] {" Press the <i>left button</i> \nfor correct recall",
                                                            GERMAN_TRANSLATION_NEEDED}},
        { "efr left button incorrect message", new string [] {" Press the <i>left button</i> \nfor incorrect recall",
                                                              GERMAN_TRANSLATION_NEEDED}},
        { "efr right button correct message", new string [] {"Press the <i>right button</i>\nfor correct recall",
                                                             GERMAN_TRANSLATION_NEEDED}},
        { "efr right button incorrect message", new string [] {"Press the <i>right button</i>\nfor incorrect recall",
                                                               GERMAN_TRANSLATION_NEEDED}},

        { "efr keypress practice left button correct message", new string [] {" Press the <b><i>left button</i></b> \nfor correct recall",
                                                                              GERMAN_TRANSLATION_NEEDED}},
        { "efr keypress practice left button incorrect message", new string [] {" Press the <b><i>left button</i></b> \nfor incorrect recall",
                                                                                GERMAN_TRANSLATION_NEEDED}},
        { "efr keypress practice right button correct message", new string [] {"Press the <b><i>right button</i></b>\nfor correct recall",
                                                                               GERMAN_TRANSLATION_NEEDED}},
        { "efr keypress practice right button incorrect message", new string [] {"Press the <b><i>right button</i></b>\nfor incorrect recall",
                                                                                 GERMAN_TRANSLATION_NEEDED}},

        { "practice invitation", new string [] {"Let's practice!", GERMAN_TRANSLATION_NEEDED}},
        { "efr check main", new string [] {"Let's make sure your keys are working.", GERMAN_TRANSLATION_NEEDED}},
        { "efr check description left button", new string [] {"Please press the <i><b>left button</b></i>, and make sure the text on the left is bolded:",
                                                              GERMAN_TRANSLATION_NEEDED}},
        { "efr check description right button", new string [] {"Please press the <i><b>right button</b></i>, and make sure the text on the right is bolded:",
                                                              GERMAN_TRANSLATION_NEEDED}},
        { "efr check try again main", new string [] {"Try again!", GERMAN_TRANSLATION_NEEDED}},
        { "efr check try again description", new string [] {"Make sure you press the designated buttons after saying each word.", GERMAN_TRANSLATION_NEEDED}},

        { "efr keypress practice main", new string [] {"Let's practice pressing the keys.", GERMAN_TRANSLATION_NEEDED}},
        { "efr keypress practice description", new string [] {"When the <b>right button</b> text becomes bolded - press the\nright button\n\n" +
                                                              "When the <b>left button</b> text becomes bolded - press the\nleft button", 
                                                              GERMAN_TRANSLATION_NEEDED}},

        { "fixation item", new string [] {"+", "+"}},
        { "fixation practice message", new string [] {"Please look at the plus sign", GERMAN_TRANSLATION_NEEDED}},

        { "cued recall message", new string [] {"Press the (X) key after recalling the item to move to the next store", GERMAN_TRANSLATION_NEEDED}},

        { "play movie", new string[] {"Press any key to play movie.", "Starten Sie das Video mit beliebiger Taste."} },
        { "recording confirmation", new string[] {"Did you hear the recording? \n(Y = Continue / N = Try Again / C = Cancel).",
                                                  "War die Aufnahme verständlich? \n(Y = Ja, weiter / N = Neuer Versuch / C = Abbrechen)."} },
        { "playing", new string[] {"Playing...", "Spiele ab…"} },
        { "recording", new string[] {"Recording...", "Nehme auf…"} },
        { "after the beep", new string[] {"Press any key to record a sound after the beep.", "Drücken Sie eine beliebige Taste, um eine Testaufnahme zu starten."} },
        { "running participant", new string[] {"Running a new session of Delivery Person. \n Press (Y) to continue, (N) to quit.",
                                               "Wir starten jetzt eine neue Session Fahrradkurier.\n Drücken Sie (Y) um fortzufahren, (N) um abzubrechen.",} },
        { "begin session", new string[] {"Begin session", "Beginne Session"} },
        { "break", new string[] {"It's time for a short break.\nPlease wait for the researcher to come check on you before continuing the experiment.\n\nResearcher: Press space to resume the experiment.",
                                 ""} },

        { "please find prompt", new string[] {"please find the ", "Bitte finden Sie "} },
        { "bakery", new string[] {"bakery", "die Bäckerei"} },
        { "barber shop", new string[] {"barber shop", "den Friseur"} },
        { "bike shop", new string[] {"bike shop", "den Fahrradladen"} },
        { "cafe", new string[] {"cafe", "das Cafe"} },
        { "clothing store", new string[] {"clothing store", "das Kleidungsgeschäft"} },
        { "dentist", new string[] {"dentist", "den Zahnarzt"} },
        { "craft shop", new string[] {"craft shop", "den Bastelladen"} },
        { "grocery store", new string[] {"grocery store", "den Supermarkt"} },
        { "jewelry store", new string[] {"jewelry store", "den Juwelier"} },
        { "florist", new string[] {"florist", "den Blumenladen"} },
        { "hardware store", new string[] {"hardware store", "den Baumarkt"} },
        { "gym", new string[] {"gym", "das Fitnessstudio"} },
        { "pizzeria", new string[] {"pizzeria", "die Pizzeria"} },
        { "pet store", new string[] {"pet store", "die Tierhandlung"} },
        { "music store", new string[] {"music store", "das Musikgeschäft"} },
        { "pharmacy", new string[] {"pharmacy", "die Apotheke"} },
        { "toy store", new string[] {"toy store", "den Spielwarenladen"} }, 

        { "confetti", new string[] {"confetti", "Konfetti"} },
    };

    public static string GetLanguageString(string string_name)
    {
        if (!language_string_dict.ContainsKey(string_name))
            throw new UnityException("I don't have a language string called: " + string_name);
        return language_string_dict[string_name][(int)current_language];
    }
}
