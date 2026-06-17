# Izzys Generic Context Window UI

The purpose of this library is to provide a simple, but flexible, UI window for development and debugging

## Usage
- To setup, just add a node inheriting from IzzysUIController to the scene. Nothing else required
- To open a window, call IzzysUIController.OpenContextWindow() on a new ContextWindowInfo() object
  - The Header parameter sets the text at the top of the window
  - The Text parameter sets the text inside the window