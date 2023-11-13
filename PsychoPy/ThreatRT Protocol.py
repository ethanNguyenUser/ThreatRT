from psychopy import visual, core, event, data, gui
import random
import csv
import os
from datetime import datetime

NUM_TRIALS = 1  # Number of trials per stimulus
MIN_WAIT = 1.5  # Minimum wait time
MAX_WAIT = 3.5  # Maximum wait time

# Prompt for participant name
info = {"Participant Name": ""}
dlg = gui.DlgFromDict(dictionary=info, title="Experiment")
if dlg.OK == False:
    core.quit()

participant_name = info["Participant Name"]

# Setup window
win = visual.Window([800, 600], fullscr=True)

# Instructions
instructions = visual.TextStim(win, text="Focus on the central crosshair and press the spacebar as soon as you see an image. Press spacebar when ready to start.")
instructions.draw()
win.flip()
event.waitKeys(keyList=["space"])

# Stimuli list
stimuli = ["snake", "spider", "square", "circle", "star", "triangle", "clock", "chair", "apple", "banana"]
trials = data.TrialHandler(stimuli * NUM_TRIALS, 1, method="random")  # NUM_TRIALS for each stimulus

# Crosshair
crosshair = visual.TextStim(win, text="+", height=0.1)

# Check and create Data folder
data_folder = "Data"
if not os.path.exists(data_folder):
    os.makedirs(data_folder)

# Prepare CSV file
timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
csv_filename = os.path.join(data_folder, f"{participant_name}_reaction_times_{timestamp}.csv")
with open(csv_filename, "w", newline="") as file:
    writer = csv.writer(file)
    writer.writerow(["Stimulus", "Reaction Time (s)"])

    # Main experiment loop
    for trial in trials:
        crosshair.draw()
        win.flip()
        wait_time = random.uniform(MIN_WAIT, MAX_WAIT)  # Random wait between MIN_WAIT and MAX_WAIT
        core.wait(wait_time)

        # Display image
        imagePath = os.path.join("Images", trial + ".png")
        image = visual.ImageStim(win, image=imagePath, size=(0.5, 0.5))  # Adjust size as needed
        image.draw()
        win.flip()
        timer = core.Clock()
        keys = event.waitKeys(keyList=["space"], timeStamped=timer)
        
        # Check for premature press
        if keys[0][1] < wait_time:
            warning = visual.TextStim(win, text="Please wait for the image to appear before pressing space. This trial will be discounted.", height=0.05)
            warning.draw()
            win.flip()
            core.wait(2)
            continue  # Skip this trial
        
        # Record reaction time
        reaction_time = keys[0][1] - wait_time
        writer.writerow([trial, reaction_time])

        # Inter-trial interval
        core.wait(1)

# Close the window
win.close()
core.quit()
