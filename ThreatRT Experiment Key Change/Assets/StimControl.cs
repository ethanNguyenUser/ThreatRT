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
    //------------------------------------------------Define variables here, initialize in Start()-------------------------------------------------

    // independent variable being tested
    // calculated for 10m distance from camera to deg0
    public string[] pos; // different random positions available (Unity object names)
    public int[] delay;
    public string[] stimuli; // names of different stimuli

    // self explanatory
    public string[] instrTextValues;

    // counter for finishing the program
    public int currentTrial;
    public int trainingTrials;
    private int totalTrials;

    // global variables for time
    public float preCue_time;
    // wait time before cue is shown after trial ends
    public float cue_time; // time that the cue is on screen
    public float time_min; // minimum time between cue disappears and stimulus    
    public float time_max; // maximum time between cue disappears and stimulus
    public float cueToStim_time; // randomly set later in code

    public int countdownTime; // time between training and experiment phase

    // phase of experiment
    public int phase;
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
    static string dataPath;
    string logFile; // fileName, set in phase 0 after getting participant name
    Random rnd;
    private string responseKey;
    private int instrNum; // index used to increment instructions
    private bool keyReleased;
    private int stimIndex; // indices for pos and stimuli respectively randomized later in code (need global scope since they're used in multiple functions)
    public GameObject instrText; // text object for instructions
    public GameObject trainingText; // text object for training
    public TMP_InputField nameInputField; // UI object for name Input

    // New variables for experiment
    private float trialDuration; // Duration of each trial in seconds
    private long logStartTime; // Timestamp when a stimulus is displayed

    private KeyCode threatKey;
    private KeyCode foodKey;

    // Initialize variables with Start() to avoid requirement to reset the script in Unity everytime a value is changed
    void Start()
    {
        pos = new string[] { "deg-15", "deg15" }; // different random positions available (Unity object names)
        //delay = new int[] {200 };
        delay = new int[] { 0, 25, 25, 50, 50, 100, 100 };
        stimuli = new string[] { "snake", "spider", "apple", "banana" }; // names of different stimuli

        // self explanatory
        instrTextValues = new string[] {
            // instruction 0
            @"You will be reacting to four different stimuli in this protocol:
            snake, spider, apple, and banana. 
            Please try to react to the stimuli and don't try to anticipate them.
            You will be shown what these stimuli look like now
            Press Spacebar when ready.",
        // instruction 1
        @"This is a snake. Press Spacebar to continue.",
        // instruction 2
        @"This is a spider. Press Spacebar to continue.",
        // instruction 3
        @"This is an apple. Press Spacebar to continue.",
        // instruction 4
        @"This is a banana. Press Spacebar to continue.",
        // instruction 5
        @"You will now be shown two stimuli
        on the left and right of the screen 
        at the same time. For whichever stimuli
        you think appears first, press
        the left arrow key for left stimuli and
        the right arrow key for right stimuli
        Prioritize accuracy over speed for these trials
        Press Spacebar when ready."
        };

        // counter for finishing the program
        currentTrial = 1;
        trainingTrials = 20;
        totalTrials = 196;

        // global variables for time
        preCue_time = (float)0.5; // wait time before cue is shown after trial ends
        cue_time = (float)0.2; // time that the cue is on screen
        time_min = (float)0.5; // minimum time between cue disappears and stimulus    
        time_max = (float)1.5; // maximum time between cue disappears and stimulus
        cueToStim_time = (float)0; // randomly set later in code

        countdownTime = 5; // time between training and experiment phase

        /*
         * Phase -1,-2,-3... = in-between phase 1, 2, or 3, while co-routines are in the middle of running
         * Phase 0 = name input
         * Phase 1 = start / instructions
         * Phase 2 = training phase
         * Phase 3 = break 
         * Phase 4 = data taking phase
         * Phase 5 = thank you screen / demographics survey reminder\
         */

        //misc variables
        dataPath = Directory.GetCurrentDirectory() + "/Assets/Data/";
        rnd = new Random();
        responseKey = "";
        instrNum = 0; // index used to increment instructions
        keyReleased = false;

        // New variables for experiment
        phase = 0; //set starting phase here
        threatKey = KeyCode.G;
        foodKey = KeyCode.H;

        // Initialize CSV file for data logging
        logFile = dataPath + "Testing.csv"; // Example file name
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        File.WriteAllText(logFile, "DecisionTimeMs,DelayMs,StimType1st,StimType2nd,StimPos1st,StimPos2nd,KeyPressed,Correct\n");

        //------------------------------------------End of variable initalization-----------------------------------------
        GameObject.Find("Canvas").transform.position = GameObject.Find("Disappear").transform.position; // hide canvas
        instrText = GameObject.Find("instrText");
        trainingText = GameObject.Find("trainingText");
        nameInputField = GameObject.Find("nameInput").GetComponent<TMP_InputField>();
    }

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
        //else if (phase == 2)
        //{
        //    StartCoroutine(phase2()); // Training phase
        //}
        //else if (phase == 3)
        //{
        //    StartCoroutine(phase3()); // Break phase
        //}
        else if (phase == 4)
        {
            StartCoroutine(phase4()); // Data collection phase
        }
        else if (phase == 5)
        {
            StartCoroutine(phase5()); // Conclusion phase
        }
    }

    void phase0()
    {
        GameObject.Find("Canvas").transform.position = GameObject.Find("deg0").transform.position; // hide canvas
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Get the participant's name and create the new file name
            string newLogFile = dataPath + nameInputField.text + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".csv";

            // Rename the log file to include name
            if (File.Exists(logFile))
            {
                File.Move(logFile, newLogFile);
                logFile = newLogFile; // Update the logFile variable to reflect the new file name
            }

            // Proceed to the next phase
            GameObject.Find("Canvas").transform.position = GameObject.Find("Disappear").transform.position; // hide canvas
            phase = 1;
        }
    }

    // Start and Instruction phase
    void phase1()
    {
        instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
        instrText.transform.position = GameObject.Find("textPos").transform.position;
        if (Input.GetKeyDown(KeyCode.Space) && instrNum == 0)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("snake").transform.position = GameObject.Find("deg0").transform.position;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && instrNum == 1)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("snake").transform.position = GameObject.Find("Disappear").transform.position;
            GameObject.Find("spider").transform.position = GameObject.Find("deg0").transform.position;
        }
        else if (instrNum == 1 && !Input.GetKeyDown(KeyCode.Space)) //this is required or else we will skip instrNum == 2 case due to the user having already pressed B
        {
            keyReleased = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && instrNum == 2 && keyReleased)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("spider").transform.position = GameObject.Find("Disappear").transform.position;
            GameObject.Find("apple").transform.position = GameObject.Find("deg0").transform.position;
            keyReleased = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && instrNum == 3)
        {
            instrNum++;
            instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
            GameObject.Find("apple").transform.position = GameObject.Find("Disappear").transform.position;
            GameObject.Find("banana").transform.position = GameObject.Find("deg0").transform.position;
        }
        else if (instrNum == 3 && !Input.GetKeyDown(KeyCode.Space))
        {
            keyReleased = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && instrNum == 4)
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
            phase = 4;
        }
    }
    //// Training Phase
    //IEnumerator phase2()
    //{
    //    phase *= -1; // Indicate that the coroutine is running

    //    while (currentTrial <= trainingTrials)
    //    {
    //        //----------------------------------------------------show stimulus----------------------------------------------------------
    //        yield return new WaitForSecondsRealtime(preCue_time);
    //        GameObject.Find("Cue").transform.position = GameObject.Find("cuePos").transform.position;
    //        yield return new WaitForSecondsRealtime(cue_time);
    //        GameObject.Find("Cue").transform.position = GameObject.Find("Disappear").transform.position;
    //        // randomizes stimulus every round
    //        stimIndex = rnd.Next(0, stimuli.Length);
    //        yield return new WaitForSecondsRealtime(cueToStim_time); // waits before showing stimulus

    //        // shows stimulus
    //        GameObject.Find(stimuli[stimIndex]).transform.position = GameObject.Find("deg0").transform.position; // StimType appears

    //        //--------------------------------------------wait for and deal with response----------------------------------------------
    //        yield return new WaitUntil(() => Input.GetKeyDown(threatKey) || Input.GetKeyDown(foodKey)); // Wait for key press

    //        responseKey = Input.GetKeyDown(threatKey) ? "Threat" : "Food";
    //        string feedbackText = CheckResponseCorrectness() ? "Correct!" : "Incorrect.";
    //        trainingText.GetComponent<TextMeshPro>().text = feedbackText;
    //        trainingText.transform.position = GameObject.Find("textPos").transform.position;
    //        yield return new WaitForSecondsRealtime(1.5f);
    //        trainingText.transform.position = GameObject.Find("Disappear").transform.position;

    //        ClearStimuli(); // Clear the screen for the next stimulus

    //        currentTrial++;
    //    }
    //    phase = 3; // Move to the next phase
    //}

    //// Break Phase
    //IEnumerator phase3()
    //{
    //    phase *= -1;
    //    trainingText.GetComponent<TextMeshPro>().text = $"Training complete. The experiment will begin in {countdownTime} seconds.";
    //    trainingText.transform.position = GameObject.Find("textPos").transform.position;
    //    while (countdownTime > 0)
    //    {
    //        yield return new WaitForSecondsRealtime(1);
    //        countdownTime--;
    //        trainingText.GetComponent<TextMeshPro>().text = $"Training complete. The experiment will begin in {countdownTime} seconds.";
    //    }

    //    trainingText.transform.position = GameObject.Find("Disappear").transform.position;
    //    phase = 4; // Move to data collection phase
    //}

    // Data Collection Phase
    IEnumerator phase4()
    {
        phase *= -1;

        instrNum = 5;
        instrText.GetComponent<TextMeshPro>().text = instrTextValues[instrNum];
        instrText.transform.position = GameObject.Find("deg0").transform.position;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        instrText.transform.position = GameObject.Find("Disappear").transform.position;

        currentTrial = 1;
        while (currentTrial <= totalTrials)
        {
            ClearStimuli();

            yield return new WaitForSecondsRealtime(preCue_time);
            GameObject.Find("Cue").transform.position = GameObject.Find("cuePos").transform.position;
            yield return new WaitForSecondsRealtime(cue_time);
            GameObject.Find("Cue").transform.position = GameObject.Find("Disappear").transform.position;

            // Show the stimulus pair with delay logic here
            int delayTime = delay[rnd.Next(delay.Length)];
            string[] pair;

            switch (rnd.Next(0, 4)) // Choose pair to show
            {
                case 0:
                    pair = new string[] { "snake", "apple" };
                    break;
                case 1:
                    pair = new string[] { "snake", "banana" };
                    break;
                case 2:
                    pair = new string[] { "spider", "apple" };
                    break;
                case 3:
                    pair = new string[] { "spider", "banana" };
                    break;
                default:
                    pair = new string[] { "FAIL", "FAIL" }; // shouldn't happen
                    break;
            }
            string firstPos = pos[rnd.Next(0, 2)]; // Randomly choose between -15 and +15 degrees
            string secondPos = firstPos == pos[0] ? pos[1] : pos[0]; // Choose other value for second position

            string firstStim = pair[rnd.Next(0, 2)]; // Randomly choose between 1st and 2nd stimuli
            string secondStim = firstStim == pair[0] ? pair[1] : pair[0]; // Choose other value for second stimuli

            // Show first stimulus
            logStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            GameObject.Find(firstStim).transform.position = GameObject.Find(firstPos).transform.position;

            // Show second stimulus
            yield return new WaitForSecondsRealtime(Math.Abs(delayTime) / 1000.0f);
            GameObject.Find(secondStim).transform.position = GameObject.Find(secondPos).transform.position;

            // Wait for participant's response
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow));
            long decisionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - logStartTime; // Adjust according to your timing logic

            // Log the response
            string pressedKey = Input.GetKeyDown(KeyCode.LeftArrow) ? "Left" : "Right";
            string correctKey = firstPos == pos[0] ? "Left" : "Right";
            string trialResult = (pressedKey == correctKey || delayTime == 0).ToString();
            // Columns: DecisionTimeMs,DelayMs,StimType1st,StimType2nd,StimPos1st,StimPos2nd,KeyPressed,Correct
            string logEntry = $"{decisionTime},{delayTime},{firstStim},{secondStim},{firstPos},{secondPos},{pressedKey},{trialResult}\n"; // Adjust fields as necessary
            File.AppendAllText(logFile, logEntry);
            currentTrial++;
        }
        ClearStimuli();
        phase = 5;
    }

    IEnumerator phase5()
    {
        phase *= -1;
        instrText.GetComponent<TextMeshPro>().text = "Experiment over\nThank you for participating!";
        instrText.transform.position = GameObject.Find("textPos").transform.position;
        yield return new WaitForSecondsRealtime(5);
        UnityEditor.EditorApplication.isPlaying = false; // or Application.Quit() for built applications
    }

    // Method to check if the response is correct
    private bool CheckResponseCorrectness()
    {
        string pressedKey = Input.GetKeyDown(threatKey) ? threatKey.ToString() : foodKey.ToString();
        bool isThreateningStimulus = (stimuli[stimIndex] == "snake" || stimuli[stimIndex] == "spider");
        return (isThreateningStimulus && pressedKey == threatKey.ToString()) || (!isThreateningStimulus && pressedKey == foodKey.ToString());
    }

    // Method to clear stimuli from screen
    private void ClearStimuli()
    {
        foreach (string stimulus in stimuli)
        {
            GameObject.Find(stimulus).transform.position = GameObject.Find("Disappear").transform.position;
        }
    }
}