##################################################################################
# Experiment: Big Foot (Vive Rebuild)
# Remake Code by: Butler, Michael
# Original Code by: Eunice, Kaushik (, Mustafa, Garrett)
# Email: msbcoding@gmail.com
# TO DO: 
#       HIGH PRIORITY: 
#
#		LOW PRIORITY: 
#		Clean up orginization
#		Redo Documentation
##################################################################################

import steamvr
import datetime
import viztask
import viz
import sys
import os
import Shaders
import random
import time
import math
viz.phys.enable()


##Testing
text2D = viz.addText("AFFORDANCE",viz.SCREEN)


##
paused = False
Swapped = False
SubjEyePercent = None
SubjHeightJudgement = None
ToRecord_YN = None
HaltFlag = False
DebugFlag = False
CalcErrorFlag = True
EH_Flag = False
Estimate = None
FootH_Offset = 0
##Reorganize LAter
viz.setLogFile('Diagnostic.log')
print("New Log Begin")
if viz.choose("Debug Mode", ["False","True"]) == 1:
	DebugFlag = True
	print("In debug mode.")
#~~~~~~~~~~Environment Setup~~~~~~~~#
# Add the Piazza, set scale
Piazza = viz.add('Piazza.osgb')

#Disables rendering of Piazza built in ground plane.
Piazza.getChild('pz_ground').disable(viz.RENDERING)

# Create Grass ground plane
# Sets plane size to the size of Piazza and attaches it as a child (so it SHOULD rotate the grass plane with the Piazza)
GrassGround = viz.addTexQuad(parent = Piazza ,size = (Piazza.getBoundingBox().width, Piazza.getBoundingBox().depth))
# Rotates quad to be parallel with ground
GrassGround.setEuler([0,90,0])

# Creates nex texture for ground plane, sets wrap modes to repeat
GrassTexture = viz.addTexture('images/grass2.jpg')
GrassTexture.wrap(viz.WRAP_S, viz.REPEAT)
GrassTexture.wrap(viz.WRAP_T, viz.REPEAT)

# Creates a texture matrix for ground plane
grass_matrix = vizmat.Transform()
grass_matrix.setScale([Piazza.getBoundingBox().width, Piazza.getBoundingBox().depth,1])
GrassGround.texmat(grass_matrix)

# Apply the texture to the ground plane
GrassGround.texture(GrassTexture)

#~~~~~~~~~~Environment Setup end~~~~~~~~#

#~~~~~~~~~~Vive Setup~~~~~~~~#

# Link the virtual eye position to the Vive position
hmd = steamvr.HMD()
headTracker = hmd.getSensor()
viewLink = viz.link(headTracker,viz.MainView)
Tracking = []
Controllers = []
#Checks for HMD
if not hmd.getSensor():
	sys.exit('SteamVR HMD not detected')
	
#~~~~~~~~~~Vive Setup end~~~~~~~~#

def CheckForMarkers():
	global Tracking, Controllers
	Trackers = steamvr.getTrackerList()
	ControllerLocal = steamvr.getControllerList()
	Tracking = []
	Controllers = []
	print("Trackers: ")
	print(Tracking)
	print("Controllers: ")
	print(Controllers)
	i = 0
	for x in Trackers:
		Tracking.append(x)
		if DebugFlag == True:
			viz.link(Tracking[i], Tracking[i].addModel())
		i += 1
	i = 0
	for x in ControllerLocal:
		Controllers.append(x)
		if DebugFlag == True:
			viz.link(Controllers[i], Controllers[i].addModel())
		i += 1
	print("Trackers: ")
	print(Tracking)
	print("Controllers: ")
	print(Controllers)
	
	
	
	
baseDepth = 0.2595 # 25.95 cm, average shoe length for women and men combined
baseWidth = .09525 # Based on subjective measurements, adult version should be * 2
baseHeight = baseDepth * 0.60 # Shoe height for this particular kind of shoe estimated by roughly measuring Nike high tops



# Gets all active vive Trackers/markers: Stored in Tracking[]
while True:
	CheckForMarkers()
	if len(Tracking) >= 2:
		break
	else:
		print("too few trackers")
		Response = viz.ask("too few trackers\nChoose Yes to exit\nOr No to check again")
		if Response:
			sys.exit()
	
#~~~~~~~~~~Crosshair Setup~~~~~~~~#
# Crosshair Code from Stepping Over 
jumpPos = [0.3 , 0 , -.1]
crosshair = viz.add('crosshair.fbx')
crosshair.disable(viz.LIGHTING)

crosshairDistance = 5

crosshair.setPosition([jumpPos[0],1,crosshairDistance])
crosshair.setScale(0.2 , 0.2 , 0.2 )

crosshairTexture = viz.add('redTexture.jpg')
crosshairTexture.wrap(viz.WRAP_T, viz.MIRROR)
crosshairTexture.wrap(viz.WRAP_S, viz.MIRROR)
crosshair.texture(crosshairTexture)
crosshair.visible(viz.OFF)


#~~~~~~~~~~Crosshair Setup end~~~~~~~~#

# Data is recorded/printed into the output file after every number of FRAME_DELAY frames
# trialstartframe indicates the first PPT vizard frame number after a trial has started
trialstartframe = 0
FRAME_DELAY = 9

#~~~~~~~~~~Tape/Line Setup~~~~~~~~#
# Constants that determine stripe size
STRIPE_LENGTH_RELATIVE = 2.0
STRIPE_WIDTH_RELATIVE = 0.035 

# Load the stripes

# stripe1 is the stripe at which the subjects start
stripe1 = viz.addTexQuad()
stripe1.setEuler([0.,90.,0.]) #sets quads rotation as to be flat on ground
stripe1.apply(Shaders.redShader) #sets to red color
stripe1.setScale([STRIPE_LENGTH_RELATIVE,.1,STRIPE_WIDTH_RELATIVE],viz.RELATIVE)
stripe1.setPosition([0.,0.001,-stripe1.getBoundingBox()[2]/2.]) # Move in negative z direction in order to have edge of stripe, where subjects' toes are aligned at the beg of each trial, at origin
stripe1Pos = stripe1.getPosition() # Store stripe1's starting pos in order to use as a reference when moving stripe2
stripe1.visible(viz.ON) # Sets to Visible at the start of the experiment

# stripe2 is the stripe that moves away/closer from the subject for AFFORDANCE judgments, also used but not moved in DISTANCE judgments
stripe2 = viz.addTexQuad()
stripe2.collideSphere() # In order to use viz.phys to accelerate as subjects move stripe2 away/closer
viz.phys.setGravity(0,0,0)
stripe2.setEuler([0.,90.,0.])
stripe2.apply(Shaders.blueShader)
stripe2.setScale([STRIPE_LENGTH_RELATIVE,.1,STRIPE_WIDTH_RELATIVE],viz.RELATIVE)
stripe2.setPosition([0.,0.001,stripe2.getBoundingBox()[2]/2.]) # Move in positive z direction in order to have distance between stripe1 and stripe2 be the distance between their inner edges
stripe2.visible(viz.OFF) # Will become visible at the beginnning of each trial
#~~~~~~~~~~Set up end~~~~~~~~#

#~~~~~~~~~~Functions~~~~~~~~#
def CrosshairDisplay(s):
	Piazza.visible(viz.OFF)
	GrassGround.visible(viz.OFF)
	crosshair.visible(viz.ON)
	viztask.schedule(ReturnToExperiment(s))
	stripe1.visible(viz.OFF)
	stripe2.visible(viz.OFF)
def ReturnToExperiment(s):
	yield viztask.waitTime(s)
	Piazza.visible(viz.ON)
	GrassGround.visible(viz.ON)
	crosshair.visible(viz.OFF)
	stripe1.visible(viz.ON)
	stripe2.visible(viz.ON)
def BlockDisplay(tString):
	global text2D
	Piazza.visible(viz.OFF)
	GrassGround.visible(viz.OFF)
	text2D.message(tString)
	stripe1.visible(viz.OFF)
	stripe2.visible(viz.OFF)
def BlockDisplayOFF():
	Piazza.visible(viz.ON)
	GrassGround.visible(viz.ON)
	stripe1.visible(viz.ON)
	stripe2.visible(viz.ON)


def RotatePiazza(RotationV3):
	Piazza.setEuler(RotationV3[0],RotationV3[1],RotationV3[2])
	pass
	
#~~~~~~~~~~Functions end~~~~~~~~#


#~~~~~~~~~~Experiment Logic~~~~~~~~#

#Settings



MSAAVal = 0
MSAA = viz.choose("Multisample Anti-Aliasing", ['16','8','4','2','None'])
if MSAA == 0:
	MSAAVal = 16
elif MSAA == 1:
	MSAAVal = 8
elif MSAA == 2:
	MSAAVal = 4
elif MSAA == 3:
	MSAAVal = 2
elif MSAA == 4:
	MSAAVal = 0
viz.setMultiSample(MSAAVal)
print ("MSAA: " + str(MSAAVal))
del MSAAVal

VSync = viz.choose("Vertical Sync", ['False', 'True'])
if VSync == 0:
	viz.vsync(0)
else:
	viz.vsync(1)
print ("VSync: " + str(VSync))

# Input questions/variables: Asked at program launch
subjectno = viz.input('subject number?' , value='99')
age = viz.input('Age number?' , value='-1')
gender = viz.choose("Gender?", ["Male","Female","Not Given"])
footSize = 0
while int(footSize) <= 0:
	footSize = viz.input('Participant foot size (in CM)' , value='0')
baseDepth = footSize *.01 #sets participants foot size to be the basis of calculation for shoe model
print baseDepth
baseWidth = baseDepth * .74 #based on the percentage basewidth is of basdepth for the adult trial (which is the original declarition of these variables)
baseHeight = baseDepth * 0.60 # Shoe height for this particular kind of shoe estimated by roughly measuring Nike high tops



while(CalcErrorFlag):
	Tracker_Offset = viz.input('Tracker offset from center to ground (in CM)' , value='0')
	Tracker_Offset = Tracker_Offset * .01
	Tracker2FootEdge = viz.input('distance from tracker center to the ground in front of the participants foot (in CM)' , value='0')
	Tracker2FootEdge = Tracker2FootEdge * .01
	if(Tracker_Offset/Tracker2FootEdge > 1 or Tracker_Offset/Tracker2FootEdge < 0):
		pass
	else:
		CalcErrorFlag = False



Multi = 0
while Multi <= 0:
	Multi = viz.input("Input a scale modifier for the shoes:\n(input as a number, not a fraction)", value = '0')


"""
if(os.path.exists("Data/data-"+navigationModeString+"-"+str(subjectNumber)+".txt") and subjectNumber > 0):
	print("File Alread Exists!!")
outputFile = open("Data/data-"+navigationModeString+"-"+str(subjectNumber)+".txt","w")
""" #file example from another experiment, maybe this would work?
# File Output: Records the initial/one time data
dateTime = datetime.datetime.now().strftime("%Y-%m-%d")
PathEXE = viz.res.getPublishedPath() 
print (PathEXE)
FNameStr = viz.res.getPublishedPath("DataSimple\SubjectNumber_" + str(subjectno) + '_BigFoot_' + dateTime + '.csv')
FNameStr2= viz.res.getPublishedPath("DataDetailed\SubjectNumber_" + str(subjectno) + '_BigFoot_' + dateTime + '_TrackingData' + '.csv')
print FNameStr
out = file( FNameStr , 'a')
out2 = file ( FNameStr2 , 'a')

out.write('BigFoot - Vive Remake\n')
out2.write('BigFoot - Vive Remake\n')
out.write('Subject number: ' + str(subjectno) + '\n') 
out2.write('Subject number: ' + str(subjectno) + '\n')  
out.write('Shoe Multiplier: ' + str(Multi) + '\n')
out2.write('Shoe Multiplier: ' + str(Multi) + '\n')

if age == -1:
	out.write('Age: Not specified\n')
	out2.write('Age: Not specified\n')
else:
	out.write('Age: ' + str(age) + '\n')
	out2.write('Age: ' + str(age) + '\n')
	
if gender == 0:
	out.write('Gender: Male\n')
	out2.write('Gender: Male\n')
elif gender == 1:
	out.write('Gender: Female\n')
	out2.write('Gender: Female\n')
else:
	out.write('Gender: Not Given\n')
	out2.write('Gender: Not Given\n')
	
#Change experiment columns with relevant information later`
out.write('\nSubject Number,Trial Index,Gap Width,Trial Type,Rotation,Can Step,Distance,Height Perception,Height Change \n')
out2.write('\nSubject Number,Trial Index,Gap Width,Trial Type,Rotation,Can Step,Distance,Height Perception,Height Change, X Position, Y Position, Z Position, Euler alpha (Yaw), Euler gamma (Pitch), Euler beta (Roll), LeftHeel X,LeftHeel Y,LeftHeel Z, RightHeel X, RightHeel Y,RightHeel Z\n')

#Flush the buffer, Ensure initial data is saved.
out.flush()
out2.flush()
os.fsync(out)
os.fsync(out2)

rFoot = viz.addGroup()
rFootModel = viz.addChild('models/shoesrsF.osgb', parent = rFoot)
rFoot.setScale([baseDepth,baseDepth,baseDepth])
rFoot.visible(viz.OFF)
rFootTracker = None
RFootLink = None


lFoot = viz.addGroup()
lFootModel = viz.addChild('models/shoeslsF.osgb', parent = lFoot)
lFoot.setScale([baseDepth,baseDepth,baseDepth])
lFoot.visible(viz.OFF)
lFootTracker = None
LFootLink = None




def SetShoes():
	global Tracking, RFootLink, LFootLink, rFoot, lFoot
	#global tFootExtra, TLink
		
	rFootTracker = Tracking[0]
	RFootLink = viz.link(Tracking[0], rFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))

	lFootTracker = Tracking[1]
	LFootLink = viz.link(Tracking[1], lFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))
	

	
	

#Launch
viz.go()

InputString = ""
InputText2D = viz.addText(InputString,viz.SCREEN, pos = [0,0.8,0]) 
##Note: because this uses the pos keyword, it must be after viz.go or it breaks when attempting to publish 
# (not 100% sure the above is accurate, but position of this variable is all that was changed in order to fix an infinite hang)

def printTarget(z=0): 
	
	while True:
		LeftArray = lFoot.getPosition(viz.ABS_GLOBAL)
		RightArray = rFoot.getPosition(viz.ABS_GLOBAL)
		if trialType == 0:
			TT_var = 'AFFORDANCE'
		elif trialType == 1:
			TT_var = 'DISTANCE'
		else:
			TT_var = 'HEIGHT ESTIM'
		out2.write(
		#Subject Number
		str(subjectno) + ',' + 
		#trial index
		str(t) + ',' + 
		#Gap Width
		str(trials[t][1]) + ',' +
		#Trial Type
		TT_var + ',' +
		#Rotation
		str(trials[t][0]) + ',' +
		#Can Step
		str(ToRecord_YN) + ',' +
		#Distance
		str(Estimate) + ',' +
		#Height Perception
		str(SubjHeightJudgement) + ',' + 
		#Height Change
		str(SubjEyePercent) + ',' +
		#Write the current position of the HMD
		str( viz.MainView.getPosition()[0] - jumpPos[0])+ ',' +
		str( viz.MainView.getPosition()[1] - jumpPos[1])+ ',' +
		str( viz.MainView.getPosition()[2] - jumpPos[2])+ ',' +
		#Write the current rotation of the HMD
		str(viz.MainView.getEuler()[0] ) + ',' +
		str(viz.MainView.getEuler()[1] ) + ',' +
		str(viz.MainView.getEuler()[2] ) + ',' + 
		#Left Heel Position
		str(LeftArray[0]) + ',' + str(LeftArray[1]) + ',' + str(LeftArray[2]) + ',' +
		#Right Heel Position
		str(RightArray[0]) + ',' + str(RightArray[1]) + ',' + str(RightArray[2]) + '\n'
		)
		yield viztask.waitFrame(FRAME_DELAY)
		

######## Set up FEET ########


## Changed to asking for a multiplier rather than two conditions
"""
# Read foot size
ShoeCondition = viz.choose("Shoe Condition", ["Little", "Big"])
LittleFootMultiplier = .5 #Little Foot Multiplier
BigFootMultiplier = 2 #Big Foot Multiplier
Multi = 0

if ShoeCondition == 0:
	Multi = LittleFootMultiplier
elif ShoeCondition == 1:
	Multi = BigFootMultiplier
else:
	print("Shoe Condition error!")
"""

######
# This flag indicates whether the shoes were scaled to the base size/dimensions and linked, see myKeyboard() function
feetScaledFlag = -1 # Updated only once throughout the experiment after pressing "u" on the keyboard
##############################

# @param: elts = the elements that are to be randomized, stored in an array
# @param: times = the number of times that elts is to be repeated/appended
# @return: arr = an array that is comprised of elts repeated times times, len(arr) == len(elts) * times
def buildRandomOrder(elts, times):
	arr = list(elts) # Counts as first time that elts is repeated/appended
	random.shuffle(arr) # Shuffle elts
	t = 1 # The number of times that elts has been appended to array arr
	
	temp = list(elts)
	while t < times:
		random.shuffle(temp)
		while temp[0] == arr[len(arr)-1]: # Ensures that subjects will not be in the same direction for two consecutive trials
			random.shuffle(temp)
		arr.extend(temp)
		t += 1
	
	return arr
	
trials = [] # Will include ALL trials, AFFORDANCE & DISTANCE
		
# AFFORDANCE trials set up
# Show random widths in random directions

# Trials for the AFFORDANCE judgments
affTrials = [] # All AFFORDANCE trials, both foot sizes
directions = [0,1,2,3]
widths = [  .45,  .60 ,  .75 ,  .90 , 1.05 , 1.20 , 1.35, 1.50 ]

# Trials in first foot size
directionsOrder = buildRandomOrder(directions, 6) # len(directionsOrder) = 24, 24 trials in each foot size
widthsOrder = buildRandomOrder(widths, 3) # Each width is presented 3 times each

assert len(directionsOrder) == len(widthsOrder)
for i in range(len(directionsOrder)): # len(directionsOrder) == len(widthsOrder)
	temp = []
	temp.append(directionsOrder[i])
	temp.append(widthsOrder[i])
	affTrials.append(temp)


# DISTANCE judgments set up
distTrials = [] # All DISTANCE trials, both foot sizes
possibleDistTrials = list() # All possible DISTANCE trials
distances = [ 0.5, 0.9, 1.3, 1.7]

# Choose only desired number of directions randomly
desiredNumDirections = 3 # How many directions we want to rotate through
desiredDirections = list() # The directions we will use; desiredNumDirections = len(desiredDirections)
tempDirections = list(directions) # Copy of directions, from above in AFFORDANCE trials set up

for i in range(0,desiredNumDirections):
	index = random.choice((0, len(tempDirections) - 1))
	desiredDirections.append(tempDirections[index])
	
	del tempDirections[index]
	
for r in range(len(desiredDirections)): # For each direction, add the 4 possible trials, 1 with each distance
	possibleDistTrials.append(list())
	for s in range(len(distances)):
		possibleDistTrials[r].append([desiredDirections[r], distances[s]]) # Add direction paired with each distance
		
# Randomize
for i in range(len(possibleDistTrials)):
	random.shuffle(possibleDistTrials[i])
# Order trials so that the trials rotate among the directions but the distances shown are random
for r in range(len(possibleDistTrials[0])):
	for c in range(len(possibleDistTrials)):
		distTrials.append(possibleDistTrials[c][r])


# Store ALL trials in one trial array
trials.extend(affTrials)
trials.extend(distTrials)


# This function takes input to rotate the world and show second stripe
# Rotating the world 90 degrees gives the subject the illusion 
# of being moved to look in a different cardinal direction
# For the AFFORDANCE & DISTANCE judgements, the trials are set up 
# the same way, so there is only one doTrial method
def doTrial( direction, width):
	global Piazza
	if direction == 0: # East
		Piazza.setEuler([0, 0, 0], viz.ABSOLUTE)
		
	elif direction == 1: # South
		Piazza.setEuler([-90, 0, 0], viz.ABSOLUTE)
		
	elif direction == 2: # West
		Piazza.setEuler([-180, 0, 0], viz.ABSOLUTE)
		
	else: # direction == 3, North
		Piazza.setEuler([-270, 0, 0], viz.ABSOLUTE) 
		
	# Place second stripe at its starting position
	stripe2.setPosition(0, 0.001, stripe1Pos[2] + stripe1.getBoundingBox()[2] + width)


#########################

######## CONTROL ########
 
# Make sure t = 0 to start with
t = 0 
# Load the first trial at the start (since, t=0 here)
stripe2.visible(viz.ON)
doTrial (affTrials[t][0] , affTrials[t][1])
print t
print trials[t]


numTrials = 24 # Number of trials for which foot size should remain constant
trialType = 0 # 0 = AFFORDANCE, 1 = DISTANCE, 2 = Eye Height Questions @ end of study
canStep = -1 # Used for AFFORDANCE only
DataEntry = False # Used to enter subject's judgments for DISTANCE
verbalEst = '' # Used for DISTANCE only (is an empty string for all AFFORDANCE judgments)

# Maps keys to different actions
def myKeyboard( key):
	global t , trialstartframe, trialType, feetScaledFlag, Piazza, stripe2, lFoot, rFoot
	global affTrials, distTrials, trials, footSize, numTrials, canStep, DataEntry, verbalEst
	global subjectno, ToRecord_YN, Estimate, FootH_Offset, Swapped, Tracker_Offset, paused
	global lFootTracker, rFootTracker, LFootLink, RFootLink, rFoot, lFoot, Tracker2FootEdge
	global lFootModel, rFootModel, InputString, EH_Flag
	
	if DataEntry == False:
		
		if key == '`':
			DataEntry = True
		if key == 'n':
			ToRecord_YN = "FALSE"
		if key == 'y':
			ToRecord_YN = "TRUE"
		# Starts the next trial
		if key == 'p':
			BlockDisplayOFF()
		if key ==  ' ':
			if trialType == 0: # AFFORDANCE
				out.write(str(subjectno) + ',' + str(t) + ',' +	str(trials[t][1]) + ',' + 'AFFORDANCE' + ',' + str(trials[t][0]) + ',' + str(ToRecord_YN) +'\n') 
				#('\nSubject Number,Trial Index,Gap Width,Trial Type,Rotation,Can Step,Distance,Height Perception,Height Change %')
				out.flush()
				os.fsync(out) 
				ToRecord_YN = None
				
				if t < len(affTrials) - 1:
					CrosshairDisplay(2)
					t += 1
					doTrial( trials[t][0] , trials[t][1] )
					print t
					print trials[t]
					trialstartframe =  viz.getFrameNumber()
				else: # Finished AFFORDANCE
					t += 1 # Increment so that the scene that is loaded after the message box is the first DISTANCE trial/next trial
					trialType = 1 # DISTANCE
	
					numTrials = 12 # Number of trials in each foot size for the DISTANCE judgments
					canStep = -1 # No longer used for remaining trials
					
					# Take break before beginning DISTANCE judgments
					BlockDisplay("DISTANCE")
					# Load the first DISTANCE trial
					doTrial (trials[t][0] , trials[t][1])
					print t
					print trials[t]
					
			else: # DISTANCE (trialType == 1) or EYE HEIGHT (trialType == 2) 
				if t < len(trials) - 1:
					CrosshairDisplay(3)
					out.write(str(subjectno) + ',' + str(t) + ',' +	str(trials[t][1]) + ',' + 'DISTANCE' + ',' + str(trials[t][0]) + ',' + "N/A" + ',' + InputString +'\n') 
					#('\nSubject Number,Trial Index,Trial Type,Rotation,Can Step,Distance,Height Perception,Height Change %')
					out.flush()
					os.fsync(out) 
					InputString = ""
					InputText2D.message(InputString)
					
					t += 1
#					if (t != len(affTrials)) and (t-len(affTrials))%numTrials == 0:
#						viz.message("")
#						print 'hit third break'
#						currSizeIndex = 1 # Set foot size index to second foot size
					doTrial( trials[t][0] , trials[t][1] )
					print t
					print trials[t]
					trialstartframe =  viz.getFrameNumber()
				else: # Finished DISTANCE
					if(EH_Flag == False):
						out.write(str(subjectno) + ',' + str(t) + ',' +	str(trials[t][1]) + ',' + 'DISTANCE' + ',' + str(trials[t][0]) + ',' + "N/A" + ',' + InputString +'\n')
						EH_Flag = True
					else:
						out.write(str(subjectno) + ',' + str(t+1) + ',' +	"N/A" + ',' + 'HEIGHT ESTIM' + ',' + str(trials[t][0]) + ',' + "N/A" + ',' + "N/A" + ',' + InputString +'\n')
					trialType = 2 # EYE HEIGHT
					paused = True
					InputString = ""
					InputText2D.message(InputString)
					BlockDisplay("EYE HEIGHT")
					
		# Precautionary inclusion, these keys should be VERY circumstantially used
		# Go back one trial
		if key == viz.KEY_LEFT:
			t -= 2
			myKeyboard(' ')
		# Going forward one trial would be equivalent to pressing the spacebar so that key == ' '
			
		
		# Use this key for updating scale of the shoes to reflect the base shoe size/dimensions
		if key == 'u' and feetScaledFlag < 0:
			print ("Scaling Feet")
			# The shoes will now be scaled properly
			feetScaledFlag = 1
			
			TempList = rFoot.getScale()
			TempList = [TempList[0]*Multi,TempList[1]*Multi,TempList[2]*Multi]
			rFoot.setScale(TempList)
			TempList = lFoot.getScale()
			TempList = [TempList[0]*Multi,TempList[1]*Multi,TempList[2]*Multi]
			lFoot.setScale(TempList)
			SetShoes()
			del TempList
			
			# Make the shoes visible to the user
			rFoot.visible(viz.ON)
			lFoot.visible(viz.ON)
			
			RotAngle = math.degrees(math.asin(Tracker_Offset/Tracker2FootEdge))
			print("RotAngle: " + str(RotAngle))
			rFootModel.setAxisAngle([1,0,0,RotAngle])
			lFootModel.setAxisAngle([1,0,0,RotAngle])
			
			# Schedule a task so that the data is recorded every FRAME_DELAY seconds
			viztask.schedule(printTarget)
			
			print ("Scaling Feet Done")
		
		# Show noisy blank screen
		if key == 'b':
			CrosshairDisplay(2)
		
		if key == '1':
			Tracker_Offset = viz.input('Tracker offset from center to ground (in CM)' , value='0')
			Tracker_Offset = Tracker_Offset * .01
			LFootLink.remove()
			RFootLink.remove()
			RFootLink = viz.link(Tracking[0], rFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))
			LFootLink = viz.link(Tracking[1], lFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))
			
			if(Tracker_Offset/Tracker2FootEdge > 1 or Tracker_Offset/Tracker2FootEdge < 0):
				CalcErrorFlag = True
			
			while(CalcErrorFlag):
				Tracker_Offset = viz.input('Tracker offset from center to ground (in CM)' , value='0')
				Tracker_Offset = Tracker_Offset * .01
				Tracker2FootEdge = viz.input('distance from tracker center to the ground in front of the participants foot (in CM)' , value='0')
				Tracker2FootEdge = Tracker2FootEdge * .01
				if(Tracker_Offset/Tracker2FootEdge > 1 or Tracker_Offset/Tracker2FootEdge < 0):
					pass
				else:
					CalcErrorFlag = False
					
			RotAngle = math.degrees(math.asin(Tracker_Offset/Tracker2FootEdge))
			print("RotAngle: " + str(RotAngle))
			rFootModel.setAxisAngle([1,0,0,RotAngle])
			lFootModel.setAxisAngle([1,0,0,RotAngle])
	
		if key == '2':
			Tracker2FootEdge = viz.input('distance from tracker center to the ground in front of the participants foot (in CM)' , value='0')
			Tracker2FootEdge = Tracker2FootEdge * .01

			if(Tracker_Offset/Tracker2FootEdge > 1 or Tracker_Offset/Tracker2FootEdge < 0):
				CalcErrorFlag = True
			
			while(CalcErrorFlag):
				Tracker_Offset = viz.input('Tracker offset from center to ground (in CM)' , value='0')
				Tracker_Offset = Tracker_Offset * .01
				Tracker2FootEdge = viz.input('distance from tracker center to the ground in front of the participants foot (in CM)' , value='0')
				Tracker2FootEdge = Tracker2FootEdge * .01
				if(Tracker_Offset/Tracker2FootEdge > 1 or Tracker_Offset/Tracker2FootEdge < 0):
					pass
				else:
					CalcErrorFlag = False
	
			RotAngle = math.degrees(math.asin(Tracker_Offset/Tracker2FootEdge))
			print("RotAngle: " + str(RotAngle))
			rFootModel.setAxisAngle([1,0,0,RotAngle])
			lFootModel.setAxisAngle([1,0,0,RotAngle])

	elif DataEntry == True:
		
		if key == '`' or key == viz.KEY_RETURN:
			DataEntry = False
		elif key == viz.KEY_BACKSPACE:
			InputString = InputString[:-1]
		else:
			InputString = InputString + key
		InputText2D.message(InputString)
		
		
	if key == viz.KEY_F6:
		LFootLink.remove()
		RFootLink.remove()
		if Swapped == False:
			Swapped = True
			RFootLink = viz.link(Tracking[1], rFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))###FIX
			LFootLink = viz.link(Tracking[0], lFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))
			
		elif Swapped == True:
			Swapped = False
			RFootLink = viz.link(Tracking[0], rFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))###FIX
			LFootLink = viz.link(Tracking[1], lFoot, offset=(0,-Tracker_Offset,0, viz.REL_PARENT))
			

	if key == viz.KEY_ESCAPE:
		out.flush()
		out2.flush()
		os.fsync(out)
		os.fsync(out2) 
		out.close()
		out2.close()
		viz.quit()
	
#########################

# Register myKeyboard function with any keyboard event
# Ideally this should be a keyboard up event, but this will work too!
# TO DO: Look into the keyboard up events later
viz.callback(viz.KEYBOARD_EVENT,myKeyboard)



#~~~~~~~~~~Experiment Logic end~~~~~~~~#

