using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace XAMLControls
{
    public partial class UCWaitSpinner : UserControl
    {
        public UCWaitSpinner()
        {
            InitializeComponent();
            MoveHands.RepeatBehavior = RepeatBehavior.Forever;
            LayoutRoot.Opacity = 0d;
            DisappearClock.Completed += (object sender, EventArgs e) => { MoveHands.Stop(); };
        }

        public void Start()
        {
            MoveHands.Stop();
            AppearClock.Begin();
            MoveHands.Begin();
        }
        public void Stop()
        {
            DisappearClock.Begin();
        }

    }
}
