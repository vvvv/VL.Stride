using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;

namespace UI
{
    public class UISelectionChanger : StartupScript
    {
        public static int UISelection;
        public static string ParticleCountText;

        public override void Start()
        {
            var uiRoot = Entity.Get<UIComponent>().Page.RootElement;
            AssignUISelectionNumber(uiRoot, "Button_Particles", 0);
            AssignUISelectionNumber(uiRoot, "Button_Full", 1);
            AssignUISelectionNumber(uiRoot, "Button_Cull", 2);
            AssignUISelectionNumber(uiRoot, "Button_Tess", 3);
            AssignUISelectionNumber(uiRoot, "Button_SO", 4);

            var et = uiRoot.FindVisualChildOfType<EditText>();
            ParticleCountText = et.Text;
            et.TextChanged += (s, a) =>
            {
                ParticleCountText = ((EditText)a.Source).Text;
            };
        }

        static void AssignUISelectionNumber(UIElement uiRoot, string name, int number)
        {
            var button = uiRoot.FindVisualChildOfType<Button>(name);
            button.ClickMode = ClickMode.Press;
            button.Click += (s, a) =>
            {
                UISelection = number;
            };
        }
    }
}
