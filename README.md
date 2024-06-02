# AHAP_Manager
## Description

AHAP_Manager is a application that help you import/export transitions from Another Hour Another Planet.<br>

## How that works

### Export (Blue)

- Export the .oxz file through 'EditOneLife.exe'.
- On AHAP_Manager, click on 'browser' to open your .oxz file situate in the 'export' folder (The name look like this : 'name_XX_######.oxz').
- After the load, you can see the objects contain in this game folder on the left panel.
- Select them will show you, on the right panel, the transitions associate to it.
- Transitions with a bad reference appear in red (Object not find in the list (ID Miss) or not selected).
- You can select/deselect any transition except the red one.
- Clicking 'Export' button will copy all transitions selected into a .trt file (with same name that .oxz file).

### Import (Green)

- Put .trt and .oxz file into "import_add" folder
- Run EditOneLife.exe and close after the load are done
- On AHAP_Manager, click on 'browser' to open your .trt file situate in the 'import_add' folder.
- After the load, you can see the transitions on the left panel.
- Transitions with missing reference appear in red.
- Transitions that already exist appear in yellow.
- You can select/deselect any transition except the red one.
- Clicking 'Import' button will create all new transitions selected and replace all yellow transition, in 'transitions' folder.
