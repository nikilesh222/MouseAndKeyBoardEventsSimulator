using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;

namespace MouseSimulator
{
    public partial class Main : Form
    {
        private IKeyboardMouseEvents m_Events;
        private DateTime _lastDt = DateTime.UtcNow;
        public Main()
        {
            InitializeComponent();
            SubscribeGlobal();

            var timer = new System.Timers.Timer();
            timer.Interval = 10000;
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        private void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }

        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;

            m_Events.MouseUp += OnMouseUp;
            m_Events.KeyUp += OnKeyUp;
            
        }

        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.MouseUp -= OnMouseUp;
            m_Events.KeyUp -= OnKeyUp;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _lastDt = DateTime.UtcNow;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            Console.WriteLine("keyboard pressed" + DateTime.Now);
            _lastDt = DateTime.UtcNow;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var simulate = new Simulator();
            if ((DateTime.UtcNow - _lastDt).TotalSeconds > 50)
            {
                _lastDt = DateTime.UtcNow;
                SimulateClick();
            }
        }

        private void SimulateClick()
        {
            Random r = new Random();
            int mouseRand = r.Next(1, 5);
            int keyRand = r.Next(10, 50);
            var simulate = new Simulator();
            for (int i = 0; i < mouseRand; i++)
            {
                simulate.MouseLeftDown();
                simulate.MouseLeftUp();
                Console.WriteLine("click");
            }
            for (int i = 0; i < keyRand; i++)
            {
                simulate.KeyPress(Models.VirtualKeyCode.ESCAPE);
                Console.WriteLine("escape");
            }
        }
    }
}
