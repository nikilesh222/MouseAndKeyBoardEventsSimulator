using MouseSimulator.Interfaces;
using MouseSimulator.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using ConsoleService;

namespace MouseSimulator
{
    public class Simulator : ISimulator
    {
        private readonly POINT _mousePos;
        private readonly IInputMessageDispatcher _messageDispatcher;
        public enum InputType : uint // UInt32
        {
            Mouse = 0,

            Keyboard = 1,

            Hardware = 2,
        }

        public enum MouseFlag : uint // UInt32
        {
            Move = 0x0001,

            LeftDown = 0x0002,

            LeftUp = 0x0004,

            RightDown = 0x0008,

            RightUp = 0x0010,

            MiddleDown = 0x0020,

            MiddleUp = 0x0040,

            XDown = 0x0080,

            XUp = 0x0100,

            VerticalWheel = 0x0800,

            HorizontalWheel = 0x1000,

            VirtualDesk = 0x4000,

            Absolute = 0x8000,
        }

        public struct INPUT
        {
            public UInt32 Type;

            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public struct MOUSEINPUT
        {
            public Int32 X;

            public Int32 Y;

            public UInt32 MouseData;

            public UInt32 Flags;

            public UInt32 Time;

            public IntPtr ExtraInfo;
        }

        public struct KEYBDINPUT
        {
            public UInt16 KeyCode;

            public UInt16 Scan;

            public UInt32 Flags;

            public UInt32 Time;

            public IntPtr ExtraInfo;
        }

        public struct HARDWAREINPUT
        {
            public UInt32 Msg;

            public UInt16 ParamL;

            public UInt16 ParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;

            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;

            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
        }

        public void MoveMouse(int pixelDeltaX, int pixelDeltaY)
        {
            //var consoleAnimationService = new ConsoleAnimationService();
            //consoleAnimationService.TypeWriteLine("Simulating X direction");

            while (pixelDeltaX != 0 && pixelDeltaY != 0)
            {
                var point = GetCursorPosXYPoint();
                var resultantPoint = point;

                var x = pixelDeltaX > 0 ? -1 : 1;

                while (resultantPoint.X != point.X - x)
                {
                    MoveMouseBy(-x, 0);
                    resultantPoint = GetCursorPosXYPoint();
                }

                //Console.WriteLine($"{GetCursorPos1()} - {pixelDeltaX},{pixelDeltaY}");
                pixelDeltaX += x;
            }

            //consoleAnimationService.TypeWriteLine("Simulating Y direction");

            while (pixelDeltaY != 0)
            {
                var point = GetCursorPosXYPoint();
                var resultantPoint = point;

                var y = pixelDeltaY > 0 ? -1 : 1;

                while (resultantPoint.Y != point.Y - y)
                {
                    MoveMouseBy(0, -y);
                    resultantPoint = GetCursorPosXYPoint();
                }
                //Console.WriteLine($"{GetCursorPos1()} - {pixelDeltaX},{pixelDeltaY}");
                pixelDeltaY += y;
            }
        }

        public void MoveMouseBy(int pixelDeltaX, int pixelDeltaY)
        {
            var movement = new INPUT { Type = (UInt32)InputType.Mouse };
            movement.Data.Mouse.Flags = (UInt32)MouseFlag.Move;
            movement.Data.Mouse.X = pixelDeltaX;
            movement.Data.Mouse.Y = pixelDeltaY;
            var inputList = new List<INPUT>();
            inputList.Add(movement);
            DispatchInput(inputList.ToArray());
        }

        public void MouseLeftDown()
        {
            var movement = new INPUT { Type = (UInt32)InputType.Mouse };
            movement.Data.Mouse.Flags = (UInt32)MouseFlag.LeftDown;
            //movement.Data.Mouse.X = pixelDeltaX;
            //movement.Data.Mouse.Y = pixelDeltaY;
            var inputList = new List<INPUT>();
            inputList.Add(movement);
            DispatchInput(inputList.ToArray());
        }

        public void MouseLeftUp()
        {
            var movement = new INPUT { Type = (UInt32)InputType.Mouse };
            movement.Data.Mouse.Flags = (UInt32)MouseFlag.LeftUp;
            //movement.Data.Mouse.X = pixelDeltaX;
            //movement.Data.Mouse.Y = pixelDeltaY;
            var inputList = new List<INPUT>();
            inputList.Add(movement);
            DispatchInput(inputList.ToArray());
        }

        public void KeyPress(params VirtualKeyCode[] keyCodes)
        {
            var builder = new InputBuilder();
            KeysPress(builder, keyCodes);
            SendSimulatedInput(builder.ToArray());
            //return this;
        }

        private void KeysPress(InputBuilder builder, IEnumerable<VirtualKeyCode> keyCodes)
        {
            if (keyCodes == null) return;
            foreach (var key in keyCodes) builder.AddKeyPress(key);
        }

        private void SendSimulatedInput(INPUT[] inputList)
        {
            var messageDispatcher = new WindowsInputMessageDispatcher();
            messageDispatcher.DispatchInput(inputList);
        }

        public void DispatchInput(INPUT[] inputs)
        {
            if (inputs == null) throw new ArgumentNullException("inputs");
            if (inputs.Length == 0) throw new ArgumentException("The input array was empty", "inputs");
            var successful = SendInput((UInt32)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            //Console.WriteLine(successful.ToString());
            if (successful != inputs.Length)
                throw new Exception("Some simulated input commands were not sent successfully. The most common reason for this happening are the security features of Windows including User Interface Privacy Isolation (UIPI). Your application can only send commands to applications of the same or lower elevation. Similarly certain commands are restricted to Accessibility/UIAutomation applications. Refer to the project home page and the code samples for more information.");
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 SendInput(UInt32 numberOfInputs, INPUT[] inputs, Int32 sizeOfInputStructure);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT _mousePos);

        public string GetCursorPos1()
        {
            POINT point;
            if (GetCursorPos(out point))
            {
                return $"{point.X.ToString()},{point.Y.ToString()}";
            }
            return string.Empty;
        }

        public POINT GetCursorPosXYPoint()
        {
            if (GetCursorPos(out POINT point))
            {
                return point;
            }
            return new POINT();
        }

        public int GetCursorPosX()
        {
            if (GetCursorPos(out POINT point))
            {
                return point.X;
            }
            return 0;
        }

        public int GetCursorPosY()
        {
            if (GetCursorPos(out POINT point))
            {
                return point.Y;
            }
            return 0;
        }
    }
}
