using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using RanplanWireless.Professional.SDK;
using RanplanWireless.Professional.SDK.Events;
using RanplanWireless.Professional.SDK.Extensions;
using RanplanWireless.Professional.SDK.Plugins;
using RanplanWireless.Professional.SDK.Presentation;

namespace Ranplan.iBuildNet.NewPluginFrameworkDemo
{
    [Export(typeof(IRibbonMenuPlugin))]
    public class CustomToolPluginDemo : RibbonMenuPluginBase
    {
        public CustomToolPluginDemo() : base(new RibbonLocation(
            "Huawei",  // 指定插件所在Ribbon面板
            "Plugins", // 指定插件所在的分类
            10,        // Ribbon面板的顺序
            0,         // 分类的顺序
            0          // 按钮的顺序
        ), "Draw Sector")
        {
            // 插件可以通过下面的方式关注一些工程有关的事件。比如当对象IPolygonShapeEntity被删除的时候
            Context.Events.Subscribe<IDeleteItemEvent>(deleteEvent =>
            {
                foreach (var item in deleteEvent.DeletedItems)
                {
                    // 判断是否为扇区多边形
                    var polygon = item as IPolygonShapeEntity;
                    if (polygon.GetCustomProperty<bool>("IsSector"))
                    {
                        // 插件可以做一些额外的处理，比如删除相关联的文本框
                    }
                }
            });
        }

        public override bool CanExecute => true;

        public override void Execute()
        {
            /* 不需要实现这个 */
        }

        public override async Task Execute(IExecutionContext context)
        {
            while (ContinueDrawPolygon())
            {
                // 激活画多边形的工具，完成之后
                var polygon = await context.CreatePolygonShape();

                // 插件绘制显示对话框配置额外的信息
                var form = new Form();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // 插件可以给对象增加额外的属性，值可以为任何类型，只要它能被序列化
                    polygon.SetCustomProperty("IsSector", true);

                    // 之后插件也可以通过下面的方式获取这个值
                    Debug.Assert(polygon.GetCustomProperty<bool>("IsSector"));

                    // 插件可以使用SDK的其它功能，比如这时候插入一个文本框
                    Context.Presenter.FloorPresentations.First().AddText(new Rect(0, 0, 10, 10), "Sector 0");
                }
            }

            // 当插件退出之后，软件会回到原始的状态。比如之前使用的工具为选择工具，那么这个方法退出后就会回到选择工具。
        }

        private bool ContinueDrawPolygon()
        {
            // 插件里面，以前的逻辑应该是判断是否还有pRRU没有分配的。

            return false;
        }
    }
}
