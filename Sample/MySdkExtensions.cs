using RanplanWireless.Professional.SDK;
using RanplanWireless.Professional.SDK.Presentation;

namespace Ranplan.iBuildNet.NewPluginFrameworkDemo
{
    public static class MySdkExtensions
    {
        public static bool IsOnFloorWindow(this IPluginApplicationContext context)
        {
            return context.Presenter.Current is IFloorPresentation;
        }
    }
}