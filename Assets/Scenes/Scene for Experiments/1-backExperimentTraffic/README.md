# MapScale_UrbanCityTraffic
This project is built on MapScale project by adding pedestrians (rocketbox avatar) and vehicles (based on UTS_PRO)

## Scene Overview
- **Location**: `Assets/Scenes/Scene for Experiments/1-backExperimentTraffic.unity`
- **Purpose**: To study the effects of environmental density (**crowds and traffic**) in urban navigation on **navigation behavior** and **cue usage**. This scene implements a cognitive workload experiment combining an auditory **N-Back task** with a dynamic **Crowd and Traffic Simulation** environment. 


## Core Components

### 1. N-Back Task (Auditory)
The N-Back task is a continuous performance task used to measure functional working memory.
- **Stimulus**: Auditory clips (e.g., letters or alphabets) played at regular intervals.
- **Logic (`NBackTask.cs`)**:
  - Randomly selects and plays auditory stimuli from a predefined list.
  - Maintains a sequence of presented stimuli.
  - **Level**: Configurable (default is 1-back for this scene).
- **Response (`NBackResponseRecord.cs`)**:
  - Participants respond using the **Left-hand XR Controller Trigger**.
  - Logs "Hits", "Misses", "False Alarms", and "Correct Rejections".
  - Calculates Real-time Accuracy and Average Reaction Time (ms).

### 2. Traffic Simulation
The urban traffic environment is a dual-system simulation comprising both vehicle and pedestrian traffic, integrated using the **Gley Traffic System** and **UTS_PRO**.

#### A. Pedestrian Simulation
- **Logic**: A waypoint-based navigation system where pedestrians randomly move between nodes.
- **Key Structures**:
  - **`waypoints_main`**: GameObjects in the scene hierarchy serving as navigation points for pedestrians.
  - **`PedestrianSpawner.cs`**: Manages the instantiation and cleanup of pedestrian prefabs at these waypoint locations.
- **Key Scripts**:
  - `Waypoint.cs`: Defines individual nodes and their connections.
  - `WaypointNavigator.cs`: Handles the movement logic for pedestrians as they traverse the waypoint network.

#### B. Vehicle Simulation
- **Logic**: An AI-driven path-following system with advanced obstacle avoidance and traffic rule compliance.
- **Key Structures**:
  - **`car_path`**: GameObjects representing the physical paths and waypoints for vehicle traffic.
  - **`UTS_PRO`**: The underlying traffic system managing vehicle behaviors.
- **Key Scripts**:
  - `CarWalkPath.cs`: Defines the road paths and handles vehicle spawning.
  - `CarAIController.cs`: The core AI component for each vehicle, managing speed, steering, and obstacle detection.
  - `ReasonsStoppingCars.cs`: Centralized logic for vehicle interactions. It triggers braking or stopping when the following are detected:
    - **Other Vehicles**: Prevents rear-end collisions.
    - **Pedestrians**: Detected via raycasting (tagged as "Traffic" or "People"); cars will slow down (>2m) or full stop (<2m) to avoid collisions.
    - **Player/Camera**: Ensures vehicles stop for the participant.
    - **Semaphores**: Handles traffic light logic for intersections.


### 3. Experiment Management
- **`MapExperimentManager.cs`**: Controls the visibility of environmental cues.
  - **World Cue**: Landmarks or visual guides in the virtual world.
  - **MR Map**: A Mixed Reality map that can be toggled or activated based on experiment start.
  - **Modes**: World-only, Map-only, or World + Map.
- **`DataManager.cs`**: Manages the output directory for experiment logs based on Participant ID and Trial Number.

## How to Run
1. **Setup**: Ensure the VR headset and controllers are connected.
2. **Participant Info**: Edit the `DataManager` component in the scene to set the `Participant ID` and `Task Num`.
3. **Start**: Press the **'S'** key on the keyboard to initialize the N-Back task and begin data recording.
4. **Interaction**: Use the **Left-hand Trigger** to respond to N-back matches (when the current sound is the same as the one heard 1 step ago).

## Data Logging
Logs are saved in `.csv` format under `Assets/Logs/[ParticipantID]/`.
- **Detailed Log**: `[SceneName]_Trial[TaskNum]NBackResponsesDetailed_[Timestamp].csv`
  - Records every stimulus, participant response, classification (Hit/Miss), and reaction time.
- **Summary Log**: `[SceneName]_Trial[TaskNum]NBackSummaryRealtime_[Timestamp].csv`
  - Records aggregate statistics like Task Accuracy (%), Avg Reaction Time, and Error Rates.

