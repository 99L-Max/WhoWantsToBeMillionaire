using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    static class FontManager
    {
        private static readonly PrivateFontCollection s_fontCollection = new PrivateFontCollection();

        static FontManager()
        {
            LoadFont(Resources.Arial);
            LoadFont(Resources.COPRGTB);
        }

        private static void LoadFont(byte[] fontData)
        {
            using (var stream = new MemoryStream(fontData))
            {
                IntPtr fontPtr = IntPtr.Zero;

                try
                {
                    byte[] fontBytes = new byte[fontData.Length];
                    stream.Read(fontBytes, 0, fontBytes.Length);

                    fontPtr = Marshal.AllocCoTaskMem(fontBytes.Length);
                    Marshal.Copy(fontBytes, 0, fontPtr, fontBytes.Length);
                    s_fontCollection.AddMemoryFont(fontPtr, fontBytes.Length);
                }
                finally
                {
                    if (fontPtr != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(fontPtr);
                }
            }
        }

        public static Font CreateFont(GameFont font, float emSize, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Pixel)
            => new Font(s_fontCollection.Families[(int)font], emSize, style, unit);
    }
}
