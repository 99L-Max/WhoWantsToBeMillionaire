﻿using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class LabelMenu : Label
    {
        public LabelMenu(float fontSize, ContentAlignment alignment = ContentAlignment.MiddleLeft)
        {
            ForeColor = Color.White;
            Font = FontManager.CreateFont(GameFont.Arial, fontSize, FontStyle.Bold);
            TextAlign = alignment;
        }
    }
}