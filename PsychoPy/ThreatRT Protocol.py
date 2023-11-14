from psychopy import visual, core, event, data, gui
import random
import csv
import os
from datetime import datetime

TRIALS_PER_STIMULUS = 15  # Number of trials per stimulus
MIN_WAIT = 1  # Minimum wait time
MAX_WAIT = 3  # Maximum wait time
DESIRED_AREA = 0.25  # Adjust this value as needed for image size
TIME_BETWEEN_TRIALS = 0.5 # Time between trials

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
trials = data.TrialHandler(stimuli * TRIALS_PER_STIMULUS, 1, method="random")  # NUM_TRIALS for each stimulus

# Crosshair
crosshair = visual.TextStim(win, text="+", height=0.1)

# Check and create Data folder
data_folder = "Data"
if not os.path.exists(data_folder):
    os.makedirs(data_folder)

# Prepare CSV file
timestamp = datetime.now().strftime("%m-%d-%Y_%H_%M_%S")
csv_filename = os.path.join(data_folder, f"{participant_name}_{timestamp}.csv")
with open(csv_filename, "w", newline="") as file:
    writer = csv.writer(file)
    writer.writerow(["Stimulus", "Reaction Time (ms)"])

    # Main experiment loop
    for trial in trials:
        # Draw crosshair
        crosshair.draw()
        win.flip()
        
        # Wait
        wait_time = random.uniform(MIN_WAIT, MAX_WAIT)  # Random wait between MIN_WAIT and MAX_WAIT
        core.wait(wait_time)

        # Display image to have DESIRED_AREA
        imagePath = os.path.join("Images", trial + ".png")
        image = visual.ImageStim(win, image=imagePath)
        original_size = image.size
        aspect_ratio = original_size[0] / original_size[1]
        new_height = (DESIRED_AREA / aspect_ratio) ** 0.5
        new_width = aspect_ratio * new_height
        image.size = (new_width, new_height)
        image.draw()
        win.flip()
        timer = core.Clock()
        keys = event.waitKeys(keyList=["space"], timeStamped=timer)
        
        # Record reaction time
        reaction_time = keys[0][1] * 1000
        writer.writerow([trial, reaction_time])
        
        core.wait(TIME_BETWEEN_TRIALS)

# Close the window
win.close()
core.quit()