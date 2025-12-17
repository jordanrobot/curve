# Phase 3.0 Functional Requirements: Generic Panel Expand/Collapse

## CurveEditor Panel Expand/Collapse Mechanism

- [ ] The CurveEditor application should have a generic expand/collapse mechanism for window panels. This mechanism should allow users to expand or collapse individual panels within the application. This mechanism should be implemented similar to the way Visual Studio Code handles its side panels. Please refer to the Visual Studio Code codebase for an example of the rough functionality I'm looking for: https://github.com/microsoft/vscode/tree/main

- [ ] Panels that should use this mechanism include:
  - [ ] Directory Browser panel
  - [ ] Curve Data panel
  - [ ] Any future panels that may be added to the application.
  - [ ] Motor Properties panel
  - [ ] Curve Graph panel (should this have a no-collapse option that prevents it from being collapsed?

- [ ] All window panels within the CurveEditor application should have a header at the top of each panel. This panel header should contain the Name of the panel.

- [ ] The CurveEditor application should have a vertical bar that shows an icon for each collapsible panel.
- [ ] This vertical bar with icons should be docked to one side of the main window.
- [ ] The vertical bar should not overlap with the main content area of the application.
- [ ] This vertical bar should be on the left side of the application window by default, but users should be able to change its position to the right side via user settings. 
- [ ] This vertical bar should always be visible.
- [ ] Clicking on a panel icon in the vertical bar should expand that panel, and collapse any other expanded panels.
- [ ] If the clicked panel is already expanded, clicking its icon should collapse it.
- [ ] The size of the vertical bar should be fixed, and should not change when panels are expanded or collapsed.
- [ ] Any collapsed panel should be hidden from view, except for its icon in the vertical bar.
- [ ] The expand/collapse state of each panel should persist across application restarts. If a panel is expanded when the application is closed, it should be expanded when the application is reopened.
- [ ] Each expand/collapse panel should have a unique width that persists across application restarts. Users should be able to resize the width of each expanded panel by dragging its right edge.
- [ ] The expand/collapse mechanism should be implemented in a way that allows for easy addition of new panels in the future.
- [ ] The expand/collapse mechanism should be responsive and should not cause any noticeable lag or delay when expanding or collapsing panels.
- [ ] The expand/collapse mechanism should be visually appealing and should use smooth animations when expanding or collapsing panels.



# Phase 3.1 Functional Requirements: Directory Browser

## Directory Browser: General

## Directory Browser: file browser behavior

- [ ] Only show folders, and valid curve definition files in the directory listing.
  - [ ] To make this efficient, make an initial file list containing only directories and json files, then validate each file in a background task to filter out invalid files.

- [ ] Browser directory listing and naviation should work like VS Code's file browser.
  - [ ] Show directories in a tree view on the left side, allowing navigation into subdirectories via expansion/collapse.
  - [ ] Directories should be expandable/collapsible via carat icons to the left of the directory name.
  - [ ] Clicking on a directory expansion icon will expand or collapse the directory.
  - [ ] Directories should be sorted alphabetically, with folders listed before files.
  - [ ] Show valid curve definition files in the selected directory on the right side.
  - [ ] Single clicking on a file will open it in the curve editor.

  - [ ] Clicking on a directory name will expand/collapse it, rather than selecting it.

  - [ ] Directories and files should be displayed in the same tree; remove the two-pane view. It should look roughly like VS Code's file explorer pane:

  - [ ] files and directories within a parent directory should be shown as children of that directory in the tree view. They should be indented to indicate they are children.

``` directory browser example tree
top directory
 > directory 1
 > directory 2
 > directory 3
 V motor profiles
     motor profile 1.json
     motor profile 2.json
 > directory 4
   motor profile 3.json
   motor profile 4.json

```
Note: In this example, you'll notice that `motor profile 1.json` and `motor profile 2.json` are inside the `motor profiles` directory, while `motor profile 3.json` and `motor profile 4.json` are inside `top directory`. Note that `directory 4` is not expanded, so it's clear that those last two files are not in that directory. The tree structure allows for easy navigation through directories and files.

- [ ] The top level directory in the directory browser should not participate in the expand/collapse mechanism. It should always be expanded, and does not have a carat icon.

## Directory Browser: Behavior

- [ ] When the program starts, automatically open the last opened file in the curve editor.
- [ ] By default, the directory browser should be collapsed when the program starts.
- [ ] When the user opens a directory, automatically expand the directory tree to show the opened directory.
- [ ] Implement a "Close Directory" button to close the currently opened directory in the directory browser.
- [ ] When the command "Close Directory" is executed, collapse the directory tree to hide the closed directory.
- [ ] When the program starts, automatically open the last opened directory in the directory browser, unless the user had explicitly closed it before exiting the program.
- [ ] When the program starts, remember the expanded/collapsed state of directories in the directory browser from the last session.
- [ ] When the program starts, if the last opened directory no longer exists, collapse the directory browser.
- [ ] Directory browser width should persist in user settings.


## Directory Browser: UI
- [ ] move the Open Folder button to the File Menu
- [ ] Add a "Refresh Explorer" icon button at the top of the file browser to re-scan the directory tree for files.
- [ ] Implement a keyboard shortcut (F5) to implement "Refresh Explorer".


## Directory Browser: Text display
- [ ] The text within the directory browser should use a monospace font for better alignment.
- [ ] The text size within the directory browser should persist in user settings.
- [ ] The text size within the directory browser should be adjustable via keyboard shortcuts (e.g., Ctrl + Plus to increase, Ctrl + Minus to decrease).
- [ ] the text size within the directory browser should be adjustable via mouse wheel while holding Ctrl key.
  - [ ] Ctrl+Mouse Wheel Up to increase text size.
  - [ ] Ctrl+Mouse Wheel Down to decrease text size.
- [ ] The text within the directory browser should not text wrap; long file and directory names should be truncated with ellipses if they exceed the available width.


## Phase 3.2 Functional Requirements: Curve Data Panel

These requirements will be added in a future update.