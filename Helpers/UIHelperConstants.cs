using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontDropTheSoap.Helpers
{
    public static class UIHelperConstants
    {
        // Player movement
        public const string PlayerForward = "move_forward";
        public const string PlayerBack = "move_back";
        public const string PlayerLeft = "move_left";
        public const string PlayerRight = "move_right";
        public const string PlayerJump = "move_jump";

        // UI menu keys
        public const string EscapeMenuOpened = "ui_cancel";
        public const string ToggleControls = "menu_toggle_controls";
        public const string ToggleQuests = "menu_toggle_quests";
        public const string MenuTabLeft= "menu_tab_left";
        public const string MenuTabRight = "menu_tab_right";
        public const string QuestNext = "quest_next";
        public const string QuestPrevious = "quest_previous";
    }
}
