using System.Drawing;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    static class GameConst
    {
        public const int DeltaTime = 40;
        public const int MinPercent = 0;
        public const int MaxPercent = 100;

        public static readonly Size ScreenSize = Screen.PrimaryScreen.Bounds.Size;
    }
}