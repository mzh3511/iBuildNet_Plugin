using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media.Media3D;
using RanplanWireless.Professional.SDK;
using RanplanWireless.Professional.SDK.Extensions;
using RanplanWireless.Professional.SDK.Plugins;
using RanplanWireless.Professional.SDK.Templates;

namespace Ranplan.iBuildNet.NewPluginFrameworkDemo
{
    /// <summary>
    /// This plugin adds a new menu item called 'Add Source' to the floor window.
    /// </summary>
    [Export(typeof(IContextMenuPlugin))]
    public sealed class ContextMenuPluginDemo: ContextMenuPluginBase
    {
        // Declares the context menu and its text.
        public ContextMenuPluginDemo() : base("Add Source")
        { }

        // Execute is called when this context menu gets clicked.
        public override void Execute()
        {
            // Gets the current project.
            var project = Context.CurrentProject;

            // Gets the first template of Source device.
            var sourceTemplate = project.Templates.OfType<ISourceTemplate>().First();

            // Gets B1_F1
            var floor = project
                .Buildings.First(b => b.Name == "B1")
                .Floors.First(f => f.Number == 1);

            // Create a Source device using the first source template and add it to B1_F1.
            var device1 = floor.AddDevice(sourceTemplate, new Point3D(10, 10, 2.4F));

            // Create an Antenna using the first antenna template and add it to B1_F1.
            var device2 = floor.AddDevice(
                project.Templates.OfType<ISourceTemplate>().First(), new Point3D(3, 4, 5));

            // Connect these two devices using a cable that is created from the first cable template.
            project.Network.ConnectDevice(device1, 1, device2, 1,
                project.Templates.OfType<ICableTemplate>().First());
        }

        // This plugin is executable whenever there is a template of Source device.
        public override bool CanExecute
            => Context
                .CurrentProject
                .Templates
                .OfType<ISourceTemplate>()
                .Any();

        // The context menu provided by this plugin is only available on the floor window when it is in Network mode.
        public override IContextType TargetContext => ContextTypes.FloorNetworkDesigner;
    }
}