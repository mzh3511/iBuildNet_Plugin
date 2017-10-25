using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Plugin.Demo.Sample.MsmtConvert;
using RanOpt.iBuilding.BLL;
using RanplanWireless.Professional.SDK;
using RanplanWireless.Professional.SDK.Extensions;
using RanplanWireless.Professional.SDK.Plugins;
using RanplanWireless.Professional.SDK.Templates;

namespace Plugin.Demo.Sample
{
    /// <summary>
    /// This plugin adds a menu 'Xml Import' to Ribbon menu under 'My Plugin Tab' - 'Plugins' category.
    /// </summary>
    [Export(typeof(IRibbonMenuPlugin))]
    public sealed class MeasurementRibbonMenuPlugin : RibbonMenuPluginBase
    {
        // Declares the Ribbon menu item and its location and text.
        public MeasurementRibbonMenuPlugin()
            : base(new RibbonLocation(
                "Huawei"//,  // 指定插件所在Ribbon面板
                        //"Plugins", // 指定插件所在的分类
                        //10,        // Ribbon面板的顺序
                        //0,         // 分类的顺序
                        //0          // 按钮的顺序
                ), "Measurement Convert")
        { }

        public override void Execute()
        {
            var project = Project.Current;
            if (project != null)
            {
                new FormChooseMsmt(project.PredictionDataSet, project.Measurements).ShowDialog();
            }
            else
            {
                MessageBox.Show("There is no project opened! Please open a project before using this tool.");
            }
        }

        // This menu item is executable only when there is a project opened.
        public override bool CanExecute { get; } = true;

        // Overrides the icon of this menu item.
        public override Image Icon => PluginResources.PluginIcon;
    }
}