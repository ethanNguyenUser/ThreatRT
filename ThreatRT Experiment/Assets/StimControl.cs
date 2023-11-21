using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System.IO;
using System;
using TMPro;

/* TODO
 * Fix phase 3-5
 */

/* NEEDS
 * - to be bound to one and only one object in the code
 * - all names in the first section of variables to be real objects
 *     - and match their case sensitive names
 * - a position to show participant text called textPos
 * - a position to show the cue called cuePos
 * - a object called "Cue"
 */

public class StimControl : MonoBehaviour
{
    // independent variable being tested
    // calculated for 10m distance from camera to deg0
    public string[] pos = { "deg30", "deg-30" }; // different random positions available (Unity object names)
    public string[] ecc = { "0", "+30", "-30" }; // names to write to csv file, corresponding respectively to pos
    public int[] delay = { -100, -50, -25, 0, 25, 50, 100 };
    public string[] stimuli = { "snake", "spider", "apple", "banana" }; // names of different stimuli

    // self explanatory
    public string[] instrTextValues = {
    // instruction 1
    @"You will be reacting to four different stimuli in this protocol:
        snake, spider, apple, and banana. 
        To react, you will be pressing the keys
        b, for the snake and spider 
        n, for the apple and banana
        Please try to react to the stimuli and don't try to anticipate them.
        Press Spacebar when ready.",
    // instruction 2
    @"This is a snake. Press b to continue.",
    // instruction 3
    @"This is a spider. Press b to continue.",
    // instruction 4
    @"This is an apple. Press n to continue.",
    // instruction 5
    @"This is a banana. Press n to continue.",
    // instruction 6
    @"Here are some practice rounds to familiarize you with the protocol.
        Press Spacebar to begin.",
    };

    // counter for finishing the program
    public int currentTrial = 1;
    public int trainingTrials = 3;
    public int trials = 5;

    // global variables for time
    public float preCue_time = (float)0.5; // wait time before cue is shown after trial ends
    public float cue_time = (float)0.2; // time that the cue is on screen
    public float time_min = (float)0.5; // minimum time between cue disappears and stimulus    
    public float time_max = (float)1.5; // maximum time between cue disappears and stimulus
    public float cueToStim_time = (float)0; // randomly set later in code

    public int countdownTime = 5; // time between training and experiment phase

    // phase of experiment
    public int phase = 0;
    private bool in_use = false;    // avoid user clicking multiple buttons at same time
    private bool start = false;     // it's the first trial
    /*
     * Phase -1,-2,-3... = in-between phase 1, 2, or 3, while co-routines are in the middle of running
     * Phase 0 = name input
     * Phase 1 = start / instructions
     * Phase 2 = training phase
     * Phase 3 = break 
     * Phase 4 = data taking phase
     * Phase 5 = thank you screen / demographics survey reminder\
     * in_use = currently going through the change coroutine, has not shown next stimulus yet
     */

    //misc variables
    static string dataPath = Directory.GetCurrentDirectory() + "/Assets/Data/";
    string logFile; // fileName, set in phase 0 after getting participant name
    Random rnd = new Random();
    private string responseKey = "";
    private string log; // new line of data
    private int instrNum = 0; // index used to increment instructions
    private bool keyReleased = false;
    private int stimIndex; // indices for pos and stimuli respectively randomized later in code (need global scope since they're used in multiple functions)
    public GameObject instrText; // text object for instructions
    public GameObject trainingText; // text object for training
    public TMP_InputField nameInputField; // UI object for name Input

    // New variables for experiment
    private int totalTrials = 140;
    private float trialDuration = 4f; // Duration of each trial in seconds
    private long logStartTime = 0; // Timestamp when a stimulus is displayed


    IEnumerator change()
    {
        /*currentTrial++;
        yield return new WaitForSecondsRealtime(preCue_time); // wait before trial starts
        GameObject.Find("Cue").transform.position = GameObject.Find("cuePos").transform.position; // Cue appears at center
        log = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ","; // CueShowTime
        yield return new WaitForSecondsRealtime(cue_time); // Cue stays there for this long

        // randomizes stimulus every round
        posIndex = rnd.Next(0, pos.Length);
        stimIndex = rnd.Next(0, stimuli.Length);

        // wait time between cue and stimulus
        cueToStim_time = (float)((rnd.NextDouble() * (time_max - time_min)) + time_min);

        GameObject.Find("Cue").transform.position = GameObject.Find("Disappear").transform.position; // Cue disappears
        // waits before showing stimulus
        yield return new WaitForSecondsRealtime(cueToStim_time);

        // shows stimulus
        GameObject.Find(stimuli[stimIndex]).transform.position = GameObject.Find(pos[posIndex]).transform.position; // StimType appears
        log += DateTimeOffset.Now.ToUnixTimeMilliseconds() + ","; // ObjShowTime
        start = true;
        in_use = false;*/


        currentTrial++;
        yield return new WaitForSecondsRealtime(preCue_time); // wait before trial starts
        GameObject.Find("Cue").transform.position = GameObject.Find("cuePos").transform.position; // Cue appears at center
        log = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ","; // CueShowTime
        yield return new WaitForSecondsRealtime(cue_time); // Cue stays there for this long


        // wait time between cue and stimulus
        cueToStim_time = (float)((rnd.NextDouble() * (time_max - time_min)) + time_min);

        GameObject.Find("Cue").transform.position = GameObject.Find("Disappear").transform.position; // Cue disappears
        // waits before showing stimulus
        yield return new WaitForSecondsRealtime(cueToStim_time);

        // Randomize stimulus and position with delay
        int delayIndex = rnd.Next(delay.Length);
        int stimulusPair = rnd.Next(0, 4); // 0 for snake-apple, 1 for snake-banana, 2 for spider-apple, 3 for spider-banana
        ShowStimulusPair(stimulusPair, delay[delayIndex]);
        log += DateTimeOffset.Now.ToUnixTimeMilliseconds() + ","; // ObjShowTime
        start = true;
        in_use = false;


        /*if (currentTrial <= totalTrials)
        {
            yield return new WaitForSecondsRealtime(preCue_time);
            GameObject.Find("Cue").transform.position = GameObject.Find("cuePos").transform.position;
            log = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ","; // CueShowTime

            yield return new WaitForSecondsRealtime(cue_time);
            GameObject.Find("Cue").transform.position = GameObject.Find("Disappear").transform.position;

            // Randomize stimulus and position with delay
            int delayIndex = rnd.Next(delay.Length);
            int stimulusPair = rnd.Next(0, 4); // 0 for snake-apple, 1 for snake-banana, 2 for spider-apple, 3 for spider-banana
            ShowStimulusPair(stimulusPair, delay[delayIndex]);

            start = true;
            in_use = false;
            currentTrial++;
        }
        else
        {
            phase = 5; // Move to next phase after all trials
        }
        */
    }

    // Method for showing stimulus pair with delay
    private void ShowStimulusPair(int pairIndex, int delayTime)
    {
        string[] pair = GetStimulusPair(pairIndex);
        int firstPosIndex = rnd.Next(0, 1); // Randomly choose between -30 and +30 degrees
        int secondPosIndex = firstPosIndex == 0 ? 1 : 0; // Choose other value for second position

        // Show first stimulus
        GameObject.Find(pair[0]).transform.position = GameObject.Find(pos[firstPosIndex]).transform.position;
        StartCoroutine(ShowSecondStimulusWithDelay(pair[1], pos[secondPosIndex], delayTime));
    }

    // Coroutine to show second stimulus after a delay
    IEnumerator ShowSecondStimulusWithDelay(string stimulus, string position, int delayTime)
    {
        yield return new WaitForSecondsRealtime(Math.Abs(delayTime) / 1000.0f);
        logStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); // Set log start time
        GameObject.Find(stimulus).transform.position = GameObject.Find(position).transform.position;
    }

    // Method to return a pair of stimuli based on index
    private string[] GetStimulusPair(int index)
    {
        switch (index)
        {
            case 0: return new string[] { "snake", "apple" };
            case 1: return new string[] { "snake", "banana" };
            case 2: return new string[] { "spider", "apple" };
            case 3: return new string[] { "spider", "banana" };
            default: return new string[] { "apple", "banana" }; // Default case
        }
    }

    void phase0()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            GameObject.Find("Canvas").transform.position = GameObject.Find("cuePos").transform.position; // canvas appears
            logFile = dataPath + nameInputField.text + "rtData-" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            File.WriteAllText(logFile, "CueShowTime,ObjShowTime,ReactionTime,Eccentricity,StimType,Guess,Correct\n");

            Debug.Log($"Data file started for {nameInputField.text}");
            GameObject.Find("Canvas").transform.position = GameObject.Find("Disappear").transform.position; // canvas disappears

            phase = 1;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            instrText.transform.position = GameObject.Find("textPos").transform.position;
            return;
        }
    }

    void phase1() // start and instruction phase
    {
        if (Input.GetKeyDown(KeyCode.Space) && instrNum == 0)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("snake").transform.position = GameObject.Find("deg0").transform.position;
        }
        else if (Input.GetKeyDown(KeyCode.B) && instrNum == 1)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("snake").transform.position = GameObject.Find("Disappear").transform.position;
            GameObject.Find("spider").transform.position = GameObject.Find("deg0").transform.position;
        }
        else if (instrNum == 1 && !Input.GetKeyDown(KeyCode.B)) //this is required or else we will skip instrNum == 2 case due to the user having already pressed B
        {
            keyReleased = true;
        }
        else if (Input.GetKeyDown(KeyCode.B) && instrNum == 2 && keyReleased)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("spider").transform.position = GameObject.Find("Disappear").transform.position;
            GameObject.Find("apple").transform.position = GameObject.Find("deg0").transform.position;
            keyReleased = false;
        }
        else if (Input.GetKeyDown(KeyCode.N) && instrNum == 3)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("apple").transform.position = GameObject.Find("Disappear").transform.position;
            GameObject.Find("banana").transform.position = GameObject.Find("deg0").transform.position;
        }
        else if (instrNum == 3 && !Input.GetKeyDown(KeyCode.N))
        {
            keyReleased = true;
        }
        else if (Input.GetKeyDown(KeyCode.N) && instrNum == 4)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("banana").transform.position = GameObject.Find("Disappear").transform.position;
            keyReleased = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && instrNum == 5)
        {
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            instrText.transform.position = GameObject.Find("Disappear").transform.position;
            phase = 2;
            StartCoroutine(change());
            start = false;
        }
    }


    //IEnumerator phase2() // training phase
    //{
    //    phase *= -1;
    //    if (!in_use)
    //    {
    //        if (Input.GetKeyDown(KeyCode.V)) { responseKey = "Face1"; }
    //        else if (Input.GetKeyDown(KeyCode.B)) { responseKey = "Face2"; }
    //        else if (Input.GetKeyDown(KeyCode.N)) { responseKey = "Face3"; }
    //        if (responseKey != "")
    //        {
    //            in_use = true;
    //            for (int k = 0; k < stimuli.Length; k++)
    //            {
    //                GameObject.Find(stimuli[k]).transform.position = GameObject.Find("Disappear").transform.position;
    //            }
    //            if (start)
    //            {
    //                if (stimuli[stimIndex] == responseKey)
    //                {
    //                    trainingText.GetComponent<TextMeshPro>().text = "Correct!";
    //                    trainingText.transform.position = GameObject.Find("textPos").transform.position;
    //                    yield return new WaitForSecondsRealtime((float)1.5);
    //                    trainingText.transform.position = GameObject.Find("Disappear").transform.position;
    //                }
    //                else
    //                {
    //                    trainingText.GetComponent<TextMeshPro>().text = "Incorrect.";
    //                    trainingText.transform.position = GameObject.Find("textPos").transform.position;
    //                    yield return new WaitForSecondsRealtime((float)1.5);
    //                    trainingText.transform.position = GameObject.Find("Disappear").transform.position;
    //                }
    //            }
    //            for (int k = 0; k < stimuli.Length; k++)
    //            {
    //                GameObject.Find(stimuli[k]).transform.position = GameObject.Find("Disappear").transform.position;
    //            }
    //            responseKey = "";
    //            if (currentTrial > trainingTrials)
    //            {
    //                trainingText.GetComponent<TextMeshPro>().text = "";
    //                trainingText.transform.position = GameObject.Find("textPos").transform.position;
    //                currentTrial = 1;
    //                phase = 3;
    //                start = false;
    //                yield break;
    //            }
    //            StartCoroutine(change());
    //        }
    //    }
    //    phase *= -1;
    //}

    IEnumerator phase2() // training phase
    {
        phase *= -1;
        while (currentTrial <= trainingTrials)
        {
            if (!in_use)
            {
                in_use = true;
                stimIndex = rnd.Next(stimuli.Length); // Randomly pick a stimulus
                GameObject.Find(stimuli[stimIndex]).transform.position = GameObject.Find("deg0").transform.position; // Show it at the center

                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.N));
                HandleTrainingResponse();

                // Clear stimuli from screen and prepare for next trial
                ClearStimuli();
                currentTrial++;
                start = false;
                yield return new WaitForSecondsRealtime(trialDuration); // Wait for trial duration
            }
        }

        // Training completed, move to next phase
        trainingText.GetComponent<TextMeshPro>().text = "";
        trainingText.transform.position = GameObject.Find("Disappear").transform.position;
        currentTrial = 1;
        phase = 3; // Move to break phase
    }

    // Method to show a single stimulus for training
    private void ShowSingleStimulusForTraining()
    {
        stimIndex = rnd.Next(stimuli.Length); // Randomly pick a stimulus
        GameObject.Find(stimuli[stimIndex]).transform.position = GameObject.Find("deg0").transform.position; // Show it at the center
    }

    // Method to handle training response and provide feedback
    private void HandleTrainingResponse()
    {
        bool correct = CheckResponseCorrectness();
        string feedbackText = correct ? "Correct!" : "Incorrect.";
        trainingText.GetComponent<TextMeshPro>().text = feedbackText;
        trainingText.transform.position = GameObject.Find("textPos").transform.position;
        StartCoroutine(ClearFeedbackAfterDelay(1.5f)); // Show feedback for 1.5 seconds
    }

    // Method to check if the response is correct
    private bool CheckResponseCorrectness()
    {
        string pressedKey = Input.GetKeyDown(KeyCode.B) ? "B" : "N";
        bool isThreateningStimulus = (stimuli[stimIndex] == "snake" || stimuli[stimIndex] == "spider");
        return (isThreateningStimulus && pressedKey == "B") || (!isThreateningStimulus && pressedKey == "N");
    }

    // Coroutine to clear feedback after a delay
    IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        trainingText.transform.position = GameObject.Find("Disappear").transform.position;
    }

    // Method to clear stimuli from screen
    private void ClearStimuli()
    {
        foreach (string stimulus in stimuli)
        {
            GameObject.Find(stimulus).transform.position = GameObject.Find("Disappear").transform.position;
        }
    }


    //IEnumerator phase3()
    //{
    //    phase *= -1;
    //    trainingText.GetComponent<TextMeshPro>().text = $"Training has finished. The experiment will begin in {countdownTime} seconds";
    //    yield return new WaitForSecondsRealtime((float)1);
    //    countdownTime -= 1;
    //    phase *= -1;
    //    if (countdownTime == 0)
    //    {
    //        trainingText.GetComponent<TextMeshPro>().text = "";
    //        trainingText.transform.position = GameObject.Find("Disappear").transform.position;
    //        StartCoroutine(change());
    //        phase = 4;
    //        yield break;
    //    }
    //}
    IEnumerator phase3()
    {
        phase *= -1;
        trainingText.GetComponent<TextMeshPro>().text = $"Training complete. The experiment will begin in {countdownTime} seconds.";
        while (countdownTime > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            countdownTime--;
            trainingText.GetComponent<TextMeshPro>().text = $"Training complete. The experiment will begin in {countdownTime} seconds.";
        }

        trainingText.GetComponent<TextMeshPro>().text = "";
        trainingText.transform.position = GameObject.Find("Disappear").transform.position;
        phase = 4; // Move to data collection phase
    }

    //void phase4()
    //{
    //    if (!in_use)
    //    {
    //        if (Input.GetKeyDown(KeyCode.V)) { responseKey = "Face1"; }
    //        else if (Input.GetKeyDown(KeyCode.B)) { responseKey = "Face2"; }
    //        else if (Input.GetKeyDown(KeyCode.N)) { responseKey = "Face3"; }
    //        if (responseKey != "")
    //        {
    //            in_use = true;
    //            if (start)
    //            {
    //                log += DateTimeOffset.Now.ToUnixTimeMilliseconds() + ","; // ReactionTime
    //                log += ecc[posIndex] + "," + stimuli[stimIndex] + "," + responseKey + ","; // independentVar, StimType, Guess
    //                if (stimuli[stimIndex] == responseKey)
    //                {
    //                    log += "True\n";
    //                }
    //                else
    //                {
    //                    log += "False\n";
    //                }
    //                File.AppendAllText(logFile, log);
    //                log = "";
    //            }
    //            for (int k = 0; k < stimuli.Length; k++)
    //            {
    //                GameObject.Find(stimuli[k]).transform.position = GameObject.Find("Disappear").transform.position;
    //            }
    //            responseKey = "";
    //            if (currentTrial > trials)
    //            {
    //                phase = 5;
    //                return;
    //            }
    //            StartCoroutine(change());
    //        }
    //    }
    //}
    IEnumerator phase4()
    {
        while (currentTrial <= totalTrials)
        {
            if (!in_use)
            {
                StartCoroutine(change());
                yield return new WaitUntil(() => start); // Wait until stimulus is shown

                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.N));
                LogResponse();

                // Clear stimuli from screen and prepare for next trial
                ClearStimuli();
                currentTrial++;
                start = false;
                yield return new WaitForSecondsRealtime(trialDuration); // Wait for trial duration
            }
            else
            {
                yield return null; // Wait for the next frame before rechecking the condition
            }
        }

        phase = 5; // Move to the conclusion phase after all trials are completed
    }

    // Ensure LogResponse and ClearStimuli methods are defined as before


    // Method to log participant response
    private void LogResponse()
    {
        string pressedKey = Input.GetKeyDown(KeyCode.B) ? "B" : "N";
        long reactionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - logStartTime; // Calculate reaction time

        bool correctResponse = CheckResponseCorrectness();
        string trialResult = correctResponse ? "Correct" : "Incorrect";
        string logEntry = $"{logStartTime},{reactionTime},{ecc[posIndex]},{stimuli[stimIndex]},{pressedKey},{trialResult}\n";
        File.AppendAllText(logFile, logEntry);
    }

    // Make sure CheckResponseCorrectness and ClearStimuli methods are as previously defined


    //IEnumerator phase5()
    //{
    //    phase *= -1;
    //    instrText.GetComponent<TextMeshPro>().text = "Thank you for taking data for us! Please take your demographics survey now";
    //    instrText.transform.position = GameObject.Find("textPos").transform.position;
    //    yield return new WaitForSecondsRealtime((float)2);
    //    UnityEditor.EditorApplication.isPlaying = false;
    //    phase *= -1;
    //}
    IEnumerator phase5()
    {
        phase *= -1;
        instrText.GetComponent<TextMeshPro>().text = "Thank you for participating! Please complete the demographics survey.";
        instrText.transform.position = GameObject.Find("textPos").transform.position;
        yield return new WaitForSecondsRealtime(5);
        UnityEditor.EditorApplication.isPlaying = false; // or Application.Quit() for built applications
    }


    //void Start()
    //{
    //    instrText = GameObject.Find("instrText");
    //    trainingText = GameObject.Find("trainingText");
    //    nameInputField = GameObject.Find("nameInput").GetComponent<TMP_InputField>(); ; // UI object for name Input
    //}
    void Start()
    {
        phase = 2;
        GameObject.Find("Canvas").transform.position = GameObject.Find("Disappear").transform.position; // canvas appears
        instrText = GameObject.Find("instrText");
        trainingText = GameObject.Find("trainingText");
        nameInputField = GameObject.Find("nameInput").GetComponent<TMP_InputField>();
        // Additional initialization code here if necessary
    }


    //void Update()
    //{
    //    if (Input.GetKey(KeyCode.Escape))
    //    {
    //        // this only works in editor view
    //        UnityEditor.EditorApplication.isPlaying = false;
    //        // this only works for built programs
    //        // Application.Quit();
    //    }
    //    else if (phase < 0)
    //    {
    //        return;
    //    }
    //    else if (phase == 0) // name input
    //    {
    //        phase0();
    //    }
    //    else if (phase == 1) // in instructions / start phase
    //    {
    //        phase1();
    //    }
    //    else if (phase == 2) // in training phase
    //    {
    //        StartCoroutine(phase2());
    //    }
    //    else if (phase == 3) // break between training and data taking
    //    {
    //        StartCoroutine(phase3());
    //    }
    //    else if (phase == 4) // in data taking phase
    //    {
    //        phase4();
    //    }
    //    else if (phase == 5) // thank you / demographics survey reminder
    //    {
    //        StartCoroutine(phase5());
    //    }
    //}
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            UnityEditor.EditorApplication.isPlaying = false; // or Application.Quit() for built applications
        }
        else if (phase < 0)
        {
            return; // In-between phases, do nothing
        }
        else if (phase == 0)
        {
            phase0(); // Name input phase
        }
        else if (phase == 1)
        {
            phase1(); // Instruction phase
        }
        else if (phase == 2)
        {
            StartCoroutine(phase2()); // Training phase
        }
        else if (phase == 3)
        {
            StartCoroutine(phase3()); // Break phase
        }
        else if (phase == 4)
        {
            StartCoroutine(phase4()); // Data collection phase
        }
        else if (phase == 5)
        {
            StartCoroutine(phase5()); // Conclusion phase
        }
    }

}
