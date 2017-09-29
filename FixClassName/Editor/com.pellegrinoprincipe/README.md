## Contents
- **FixClassName**. The script **FixClassName.cs** generates a menu item *Assets > Fix Class Name...*. When used on a script file it automatically rename the relative class name. This is useful because if we rename a script file the relative class name is not automatically renamed. So, if we try to attach the script on a game object Unity will raise the following message *"Can't add script component ... because the script class cannot be found. Make sure that there are no compile errors and that **the file name and class name match**"*.
## Usage
- Put the script into **Editor** folder;
- Select the script file and hit **ALT + SHIFT + f** or open the menu **Asset > Fix Class Name...** (you can also use the contextual menu item into the Project window).

