using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ELSuitcases.SystemResourceMonitorWpf
{
    internal class WindowsTaskbarHelper
    {
        public enum TaskbarPosition
        {
            Left,
            Top,
            Right,
            Bottom,
            Unknown
        }

        private const int ABM_GETTASKBARPOS = 0x00000005;

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);



        public static RECT GetTaskbarAreaRectangle()
        {
            APPBARDATA data = new APPBARDATA()
            {
                cbSize = Marshal.SizeOf(typeof(APPBARDATA))
            };

            SHAppBarMessage(ABM_GETTASKBARPOS, ref data);

            return data.rc;
        }

        public static TaskbarPosition GetTaskbarPosition()
        {
            APPBARDATA data = new APPBARDATA()
            {
                cbSize = Marshal.SizeOf(typeof(APPBARDATA))
            };

            SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
            
            TaskbarPosition position = TaskbarPosition.Unknown;

            switch (data.uEdge)
            {
                case 0:
                    position = TaskbarPosition.Left;
                    break;

                case 1:
                    position = TaskbarPosition.Top;
                    break;

                case 2:
                    position = TaskbarPosition.Right;
                    break;

                case 3:
                    position = TaskbarPosition.Bottom;
                    break;

                default:
                    position = TaskbarPosition.Unknown;
                    break;
            }

            return position;
        }

        public static System.Drawing.Size GetTaskbarSize()
        {
            int[] size = new int[2];
            
            APPBARDATA data = new APPBARDATA()
            {
                cbSize = Marshal.SizeOf(typeof(APPBARDATA))
            };

            SHAppBarMessage(ABM_GETTASKBARPOS, ref data);

            size[0] = data.rc.right - data.rc.left;
            size[1] = data.rc.bottom - data.rc.top;

            return new System.Drawing.Size(size[0], size[1]);
        }

        public static System.Drawing.Rectangle GetWorkingAreaExceptTaskbar(IntPtr hWindow)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle();

            RECT rcTaskbar = GetTaskbarAreaRectangle();
            TaskbarPosition position = GetTaskbarPosition();
            var screen = System.Windows.Forms.Screen.FromHandle(hWindow);

            switch (position)
            {
                case TaskbarPosition.Left:
                    rect.Size = new System.Drawing.Size(screen.WorkingArea.Width - rcTaskbar.right, screen.WorkingArea.Height);
                    rect.Location = new System.Drawing.Point(rcTaskbar.right, screen.WorkingArea.Top);
                    break;

                case TaskbarPosition.Right:
                    rect.Size = new System.Drawing.Size(screen.WorkingArea.Width - (rcTaskbar.right - rcTaskbar.left), screen.WorkingArea.Height);
                    rect.Location = new System.Drawing.Point(screen.WorkingArea.Left, screen.WorkingArea.Top);
                    break;

                case TaskbarPosition.Top:
                    rect.Size = new System.Drawing.Size(screen.WorkingArea.Width, screen.WorkingArea.Height - (rcTaskbar.bottom - rcTaskbar.top));
                    rect.Location = new System.Drawing.Point(screen.WorkingArea.Left, rcTaskbar.bottom);
                    break;

                case TaskbarPosition.Bottom:
                    rect.Size = new System.Drawing.Size(screen.WorkingArea.Width, screen.WorkingArea.Height - (rcTaskbar.bottom - rcTaskbar.top));
                    rect.Location = new System.Drawing.Point(screen.WorkingArea.Left, screen.WorkingArea.Top);
                    break;
            }

            return rect;
        }
    }
}
