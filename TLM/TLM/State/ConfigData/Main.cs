namespace TrafficManager.State.ConfigData {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TrafficManager.UI.MainMenu;
    using TrafficManager.UI.SubTools.SpeedLimits;

    public class Main {
        /// <summary>Whether floating keybinds panel is visible.</summary>
        public bool KeybindsPanelVisible = true;

        /// <summary>Main menu button position X.</summary>
        public int MainMenuButtonX = 464;

        /// <summary>Main menu button position Y.</summary>
        public int MainMenuButtonY = 10;

        /// <summary>Main menu button is not movable.</summary>
        public bool MainMenuButtonPosLocked = false;

        /// <summary>Main menu position X.</summary>
        public int MainMenuX = MainMenuWindow.DEFAULT_MENU_X;

        /// <summary>Main menu position Y.</summary>
        public int MainMenuY = MainMenuWindow.DEFAULT_MENU_Y;

        /// <summary>Main menu is not movable.</summary>
        public bool MainMenuPosLocked = false;

        /// <summary>Speed Limits tool window position X.</summary>
        public int SpeedLimitsWindowX = 0;

        /// <summary>Speed Limits tool window position Y.</summary>
        public int SpeedLimitsWindowY = 0;

        /// <summary>Put button inisde UUI.</summary>
        public bool UseUUI = false;

        /// <summary>Already displayed tutorial messages.</summary>
        public string[] DisplayedTutorialMessages = new string[0];

        /// <summary>Determines if tutorial messages shall show up.</summary>
        public bool EnableTutorial = true;

        /// <summary>Determines if the main menu shall be displayed in a tiny format.</summary>
        [Obsolete("Do not use. TM:PE now has UI scale slider")]
        public bool TinyMainMenu = true;

        /// <summary>User interface transparency, unit: percents, range: 0..100.</summary>
        [Obsolete("Value is not used anymore, use GuiOpacity instead")]
        public byte GuiTransparency = 75;

        /// <summary>User interface opacity, unit: percents, range: 0..100.</summary>
        public byte GuiOpacity = 75;

        /// <summary>User interface scale for TM:PE. Unit: percents, range: 30..200f.</summary>
        public float GuiScale = 100f;

        /// <summary>
        /// if checked, size remains constnat but pixel count changes when resolution changes. Quality drops with lower resolutions.
        /// if unchecked checked, size changes constnat but pixel count remains the same. Maintains same image quality for all resolution.
        /// </summary>
        public bool GuiScaleToResolution = true;

        /// <summary>
        /// Overlay transparency
        /// </summary>
        public byte OverlayTransparency = 40;

        /// <summary>
        /// Extended mod compatibility check
        /// </summary>
        public bool ShowCompatibilityCheckErrorMessage = false;

        /// <summary>
        /// Shows warning dialog if any incompatible mods detected
        /// </summary>
        public bool ScanForKnownIncompatibleModsAtStartup = true;

        /// <summary>
        /// Skip disabled mods while running incompatible mod detector
        /// </summary>
        public bool IgnoreDisabledMods = true;

        /// <summary>
        /// Prefer Miles per hour instead of Kmph (affects speed limits display
        /// but internally Kmph are still used).
        /// </summary>
        public bool DisplaySpeedLimitsMph = false;

        public SpeedUnit GetDisplaySpeedUnit() => DisplaySpeedLimitsMph
                                                      ? SpeedUnit.Mph
                                                      : SpeedUnit.Kmph;

        /// <summary>
        /// Selected theme for road signs when MPH is active.
        /// </summary>
        public MphSignStyle MphRoadSignStyle = MphSignStyle.SquareUS;

        public void AddDisplayedTutorialMessage(string messageKey) {
            HashSet<string> newMessages = DisplayedTutorialMessages != null
                                              ? new HashSet<string>(DisplayedTutorialMessages)
                                              : new HashSet<string>();
            newMessages.Add(messageKey);
            DisplayedTutorialMessages = newMessages.ToArray();
        }
    }
}