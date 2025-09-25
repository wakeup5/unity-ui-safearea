# Unity UI SafeArea

This library assists in applying the concept of Safe Areas to Unity UI. Safe Areas are used to prevent content from being obscured by specific parts of the display, such as notches, home buttons, etc.

## Installation

To use this library in your Unity project, follow these steps:

1. Open your Unity project.
2. Go to `Window` -> `Package Manager` in the Unity Editor.
3. Click the `+` button in the top left corner of the Package Manager window.
4. Select `Add package from git URL...`.
5. Enter the following URL: ```https://github.com/wakeup5/unity-ui-safearea.git``` and click `Add`.

## Usage

### Step 1: Add SafeAreaCanvasScaler to Canvas

1. In the Unity Editor, select the `Canvas` object where you want to apply the Safe Area.
2. In the Inspector window, click on `Add Component`.
3. Search for `SafeAreaCanvasScaler` and add it to the Canvas.

### Step 2: Add SafeAreaRectTransform to Child Nodes

1. For each UI element that needs to respect the Safe Area, select the element in the Hierarchy.
2. In the Inspector window, click on `Add Component`.
3. Search for `SafeAreaRectTransform` and add it to the element.

## Options Explanation

The `SafeAreaRectTransform` component offers three modes to configure the safe area:

1. **Automation**: Automatically adjusts the `RectTransform` to fill the Safe Area. This mode ensures that the UI element fully respects the Safe Area boundaries.
2. **Simple**: Allows enabling or disabling each edge individually. This mode provides basic customization by letting you specify which edges (top, bottom, left, right) should respect the Safe Area.
3. **Advanced**: Provides more extensive options than Simple.
## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
