# RobotMaze: Discrete Event Simulation Model for Robot Navigation

## Project Overview

**RobotMaze** is a project focused on developing a discrete event simulation model for automated design systems. The goal is to investigate the effectiveness of various pathfinding algorithms (A*, Fuzzy Logic, Ant Colony Optimization) in a maze environment with obstacles and stochastic factors. The project includes:
- Maze generation and robot initialization.
- Rendering of the maze and robots.
- Event handling and user interaction.
- Implementation of pathfinding algorithms and robot movement.
- Time and event management.

The project is developed in C# using WPF for the graphical user interface.

---

## Installation

### Requirements
- .NET 6.0 or higher.
- Visual Studio 2022 or another compatible IDE.

### Setup Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/robotmaze.git
   ```
2. Open the project in Visual Studio:
   ```bash
   cd robotmaze
   code .
   ```
3. Restore dependencies:
   - Visual Studio will automatically restore all necessary packages when the project is opened.

---

## Usage

### Running the Program
1. Open the project in Visual Studio.
2. Press `F5` or select `Debug > Start Debugging` to run the application.
3. The main window will display the maze and control panel.

### Key Features
- **Maze Generation**:
  - Enter the maze dimensions in the provided fields.
  - Click the "Generate Maze" button to create a maze using the Prim's algorithm or Perlin noise.
- **Robot Control**:
  - Set the start and goal points by clicking the corresponding buttons and selecting cells on the grid.
  - Start the robots by clicking the "Start" button.
- **Zooming**:
  - Use the "Zoom In" and "Zoom Out" buttons or the mouse wheel to adjust the zoom level.

---

## Project Architecture

The project is structured into the following components:

### 1. **Models**
- `Maze.cs`: Represents the maze structure.
- `RobotMetrics.cs`: Metrics for evaluating robot performance.

### 2. **RobotLogic**
- `ACOLogic.cs`: Ant Colony Optimization algorithm.
- `AStarLogic.cs`: A* pathfinding algorithm.
- `FuzzyLogic.cs`: Fuzzy Logic for decision-making.

### 3. **Utils**
- `RobotDrawer.cs`: Utility for drawing robots.
- `SvgLoader.cs`: Utility for loading SVG files.

### 4. **ViewModels**
- `MazeViewModel.cs`: ViewModel for interacting with the maze model.

### 5. **Views**
- `MazeCanvas.xaml`: Maze rendering.
- `MainWindow.xaml`: Main application window.

### 6. **Commands**
- `ZoomInCommand.cs`: Command for zooming in.
- `ZoomOutCommand.cs`: Command for zooming out.

### 7. **MazeGeneration**
- `PerlinNoiseGenerator.cs`: Maze generator using Perlin noise.
- `PrimsAlgorithmGenerator.cs`: Maze generator using Prim's algorithm.

---

## Testing

The project includes unit tests written using the XUnit framework. The tests cover core components such as pathfinding algorithms, metrics, and robot logic.

### Running Tests
1. Open the project in Visual Studio.
2. Select `Test > Run All Tests` from the menu.

---

## Contributing

### How to Contribute
1. Create a new branch for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```
2. Make changes to the code.
3. Test your changes:
   ```bash
   dotnet test
   ```
4. Commit your changes:
   ```bash
   git add .
   git commit -m "Your commit message"
   ```
5. Push your changes to the main repository:
   ```bash
   git push origin feature/your-feature-name
   ```
6. Create a pull request on GitHub.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## Contact

If you have any questions or suggestions, please contact the project author:
- **Author**: deadFenette
- **Email**: ssdf01788@gmail.com

---

Thank you for using **RobotMaze**! 🚀
