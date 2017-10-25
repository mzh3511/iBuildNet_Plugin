using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Ranplan.iBuildNet.NewPluginFrameworkDemo;
using RanplanWireless.Professional.SDK;
using RanplanWireless.Professional.SDK.Commands;
using RanplanWireless.Professional.SDK.Extensions;
using RanplanWireless.Professional.SDK.Plugins;
using RanplanWireless.Professional.SDK.Presentation;
using RanplanWireless.Professional.SDK.Templates;
using Point = System.Windows.Point;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Plugin.Demo.Sample
{
    /// <summary>
    /// This plugin adds a menu 'Xml Import' to Ribbon menu under 'My Plugin Tab' - 'Plugins' category.
    /// </summary>
    [Export(typeof(IRibbonMenuPlugin))]
    public sealed class RibbonMenuPluginDemo : RibbonMenuPluginBase
    {
        // Declares the Ribbon menu item and its location and text.
        public RibbonMenuPluginDemo()
            : base(new RibbonLocation(
                "Huawei"//,  // 指定插件所在Ribbon面板
                //"Plugins", // 指定插件所在的分类
                //10,        // Ribbon面板的顺序
                //0,         // 分类的顺序
                //0          // 按钮的顺序
                ), "Xml Import")
        { }

        public override void Execute()
        {
            var project = Context.CurrentProject;

            // Finds the template of Source device named 'LTE 2600'
            var sourceTemplate = project
                .Templates
                .OfType<ISourceTemplate>()
                .FirstOrDefault(t => t.DisplayName.Contains("LTE 2600"));

            // Create a new Source using template 'LTE 2600' and add it to the project. Note position of this
            // device has not specified.
            var device = project.Network.AddDevice(sourceTemplate);
            Debug.Assert(device != null, "Device should be created and added.");
        }

        // This menu item is executable only when there is a project opened.
        private bool _canExecute;
        public override bool CanExecute => _canExecute;

        // Overrides the icon of this menu item.
        public override Image Icon => PluginResources.PluginIcon;

        public void Requirements_2017_08_23()
        {
            // 基础数据访问示例
            var project = Context.CurrentProject;
            var network = project.Network;
            var background = project.Backgrounds.First(b => b.Name == "B1_F1.png");
            var building = project.Buildings.First(b => b.Name == "B1");
            var floor = building.Floors.First(f => f.Name == "B1_F1");
            var sourceTemplate = project.Templates.OfType<ISourceTemplate>().First();
            var antennaTemplate = project.Templates.OfType<IAntennaTemplate>().First();
            var cableTemplate = project.Templates.OfType<ICableTemplate>().First();
            var buildingPlaneTemplate = project.Templates.OfType<IBuildingPlaneTemplate>().First(t => t.Type == PlaneType.Wall);
            var cable = Context.CurrentProject.Network.Devices.OfType<ICable>().First();

            //1.保存项目
            project.Save();

            //2.线缆两端的设备信息如何获取
            Debug.WriteLine($"线缆链接的设备为{cable.SourceDevice.Name}和{cable.MobileDevice.Name}");

            //3.插件按钮启用、置灰
            _canExecute = true;  // true to enable, false to disable
            RaiseCanExecuteChanged();  // notifies the application that CanExecute is changed.

            //4.addDevice时设备高度如何设置？
            //通过INetwork.AddDevice的时候暂不支持设置高度，类似于添加设备到NSD窗口。
            //通过IFloor.AddDevice的时候，可以通过第二个参数指定高度信息。
            var newSource = floor.AddDevice(sourceTemplate, new Point3D(0, 0, 0.5));  // the device is put at (0,0) and 0.5 metre high
            var newAntenna = floor.AddDevice(antennaTemplate, new Point(10, 10));  // the device is put at (10,10) and default high specifiyed by template

            //5.穿墙点如何获取，ID 、坐标
            var corners = cable.Corners.ToList();
            var cornerHeight = corners[0].Z;
            foreach (var corner in corners)
            {
                // 线缆拐点坐标
                Debug.WriteLine($"Corner location is ({corner.X}, {corner.Y})");
                // 穿墙点
                if (corner.Z != cornerHeight)
                {
                    Debug.WriteLine("There will be a floor cross here because the height is different than the previous corner.");
                    cornerHeight = corner.Z;
                }
                // 根据之前的讨论，穿墙点ID不提供
            }

            //6.设备ID如何获取
            // 根据之前的讨论，穿墙点ID不提供

            //7.线缆跨楼层时，中间点该如何处理.
            var newCable = network.ConnectDevice(newSource, 1, newAntenna, 1, cableTemplate);
            foreach (var point3D in new[]
            {
                new Point3D(1, 1, 0),
                new Point3D(2, 2, 0),
                new Point3D(3, 3, 0)
            })
            {
                newCable = newCable.InsertCornerAt(point3D, 0, InsertionSide.After);
            }

            //8.线缆长度取值、赋值
            cable.SetCustomLength(30);  // set custom length to 30 metre, and cable length mode to UserMode.

            //9.线缆存在穿墙点，该线缆的拐点怎么获取
            // 看前面第5条的例子

            //10.线缆所连设备的端口号
            Debug.WriteLine($"线缆连接的起始端端口是{cable.SourcePort.Number},末端端口是{cable.MobilePort.Number}");

            //11.项目名称
            Debug.WriteLine($"Project name is {project.Name}");

            //12.建筑经纬度
            //将会增加IBuilding.Location属性来获取
            Debug.WriteLine($"Building location is {building.Location}");

            //13.bsm、NSD文件如何获取
            //    文件作为工程内部数据存储形式，不希望公开给插件使用。请提供这个需求的使用场景。
            //我们对接的工具中需要这两个文件，这个在之前讨论过
            //    你们是需要获取文件的路径还是里面的内容？如果是内容的话，是文件的XML数据还是需要我们提供对象模型？如果是对象模型，需要的是能表征所有数据的完整对象模型吗？ 
            Debug.WriteLine("根据之前的讨论，我们不提供直接访问文件的接口");

            //14.如何导出背景图以及背景图所在楼层信息（以前的方法在导出背景图的同时会导出一个楼层XML，包含如下信息）：
            Debug.WriteLine($"背景图图片为{background.Image}");
            Debug.WriteLine($"背景图显示位置为{background.Location}，单位米");
            Debug.WriteLine($"背景图显示大小为{background.Width}x{background.Height}，单位米");
            Debug.WriteLine("根据之前的讨论，由插件自己根据IBackground的信息输出XML文件");
            

            //15.项目中单位像素还是50么？如何获取
            Debug.WriteLine("不再需要这个值，所有数据都使用统一的单位'米'");

            //16.PlaneType获取
            //现在还没有提供API来获取具体的楼层内部建筑物结构。请提供使用场景。
            //我们需要传入一组点坐标来建模，3点、4点来建模，插件的一个很重要的功能（这个问题最好开会讨论一下，因为提供的建模接口必须和以前一样，否则插件的算法需要推到重做）
            var wall = floor.AddBuildingPlane(buildingPlaneTemplate, new PolygonPlaneShape(new[]
            {
                new Point3D(0, 0, 0),
                new Point3D(10, 0, 0),
                new Point3D(10, 0, 3),
                new Point3D(0, 0, 3)
            }));
            Debug.WriteLine($"添加了一个新的{wall.Item2.Type}");

            //17.不同视图（BSM / FLD / NSD）按钮的启动、置灰
            //    是指插件自己提供的按钮吗？见第3条。其他按钮的话，插件为什么要去启用禁用它们呢？应该是软件本身负责更新它们的状态
            //    有些插件在不同的视图下不需要启用，或者是必须在某个视图下才能用，以此来限制用户的操作
            // -> 在CanExecute返回的时候，可以判断当前的view是什么来决定

            //18.获取材质信息IMaterialLibrary、IMaterial
            Debug.WriteLine($"材质库中的第一个材质是{project.MaterialLibrary.Materials.First()}");

            //19.获取当前显示的建筑、楼层
            Debug.WriteLine($"上一次打开的楼层是{Context.LastActivatedFloor}");

            //20.获取当前楼层的建筑建模信息

            Debug.WriteLine($"当前楼层一共有{Context.LastActivatedFloor.BuildingPlanes.Count()}个对象。");

            //21.如何建模
            //暂时需要创建墙体，地板，天花板
            // -> 类似第16条的实现，第一个参数指定材质，第二个参数指定墙体类型。

            //22.删除当前楼层建模
            building.RemoveFloor(floor);
        }

        public void Common()
        {
            //1、插件按钮对象如何获取（需要控制是否启用）
            // -> 如果是提供这个按钮的插件本身的话，只要设置CanExecute并且通过调用触发CanExecuteChanged事件就可以通知主程序去更新按钮状态了。如果是想获取其它插件提供的按钮，目前还不支持。

            //2、在一个.cs文件中如何添加两个插件
            // -> 将插件分开到不同类中

            //3、连线时，如果设备之间是直连的corners如何设置？有中间点时，corners是否要包含设备坐标，设备端口坐标？(不需要)
            // -> INetwork.ConnectDevice 方法的corners参数不需要包括两头所连设备的端点。

            //4、如何区分全向 / 定向天线的设备模板
            // -> 现在每一个模板都会有自己的接口类型，比如IAntennaTemplate和IDirectionalAntennaTemplate
            var antennaTemplate = Context.CurrentProject.Templates.OfType<IAntennaTemplate>().First();
            if (antennaTemplate is IDirectionalAntennaTemplate)
                Debug.WriteLine("这是一个定向天线的模板");
            else
                Debug.WriteLine("这是一个全向天线模板");

            //5、Iproject.Network.Device类型不再试Idevice而是INetworkComponent，之前的属性都没了，如何查看？
            // -> 现在每一个设备都有自己的接口类型，部分常用属性会放在各类设备的接口中，如果缺失的话，请告知。我们会加上。
            var cable1 = Context.CurrentProject.Network.Devices.OfType<ICable>().First();
            // IcablePosition属性为什么删除
            // -> 因为Cable有可能夸多个楼层
            // 如何查看cable所属楼层、建筑？（没有楼层的线缆）
            Debug.WriteLine($"这条Cable所在楼层为{cable1.Floors.First()}");

            //Idevice,ICable 为什么删除Model属性
            Debug.WriteLine($"可以通过模板里查到{cable1.Template.Model}");

            //BSM、FLD、NSD视图控制插件是否启用
            // -> 当切换这些窗口的时候，软件会再一次查询CanExecute来刷新按钮状态的。
            Debug.WriteLine($"在调用CanExecute的时候可以通过{Context.Presenter.Current is IFloorPresentation}来判断是否为BSM/FLD视图");
            Debug.WriteLine($"或者{(Context.Presenter.Current as IFloorPresentation)?.DisplayMode}来判断是否为BSM还是FLD模式");
            Debug.WriteLine($"或者{Context.Presenter.Current is INetworkPresentation}来判断是否为NSD视图");

            //Network.RemoveDevice能否增加批量删除
            // TODO 暂不支持批量删除器件
        }

        public void HW_Smart_IBS_XMLDocument()
        {
            //1、设备所属建筑
            var floor = Context.CurrentProject.Buildings.First().Floors.First();
            Debug.WriteLine($"{floor.Name}所属的建筑为{floor.ParentBuilding.Name}");

            //2、导出背景图需要返回背景图名称，现在Export方法是void类型
            Context.CurrentProject.Backgrounds.First().Image.Save("test.jpg", ImageFormat.Jpeg);

            //3、背景图缩放比如何获取，之前导出楼层XML中zoom属性* Project.Current.SystemDesign.LayoutLayer.PixelPerUint得出比例尺
            // -> 之前讨论过，将不再需要这个比例尺。插件这边获取到的不管是设备的位置，建筑的坐标，背景图的位置等等信息都将是统一的坐标系，都是以米为单位。比如IBackground.Image.Size为800x600像素，而IBackground.Width=80，IBackground.Height=60。那就代表背景图是80x60米的。也就是说1米对应图片中的10像素。

            //4、导出背景图增加是按照code导出还是别名导出
            // -> 不再支持导出

            //5、device.Device.FrequencyBands[0].LinkProperties设备属性怎么读取
            // -> 不同类型的设备有不同的接口类型，一些常用参数已经定义为接口的属性，如功分器的CouplerLoss
            var device = Context.CurrentProject.Network.Devices.OfType<ICoupler>().First();
            var firstBand = device.FrequencyBands.First();
            Debug.WriteLine($"{device.Name}的第一个频段的CouplerLoss属性为{device.GetCouplingLoss(firstBand)}");
        }

        public void HW_Smart_IBS_XMLImport()
        {
            //1、建筑别名赋值
            Context.CurrentProject.Buildings.First().SetName("custom building name");

            //2、获取设备模板后如何得知模板的端口个数、以及Model，之前是通过方法(IDevice)Project.Current.DeviceLibrary.FindDeviceByGuid(GUID); 读到设备再读到端口以及Model
            var antennaTemplate = Context.CurrentProject.Templates.OfType<IAntennaTemplate>().First();
            Debug.WriteLine($"{antennaTemplate.DisplayName}所用设备的型号有{antennaTemplate.Ports.Count()}个端口");
            Debug.WriteLine($"{antennaTemplate.DisplayName}所用设备的型号为{antennaTemplate.Model}");

            //3、楼层别名、设备别名赋值
            Context.CurrentProject.Buildings.First().Floors.First().SetName("custom floor name");
            Context.CurrentProject.Network.Devices.First().SetName("custom device name");

            //4、定向天线下倾角方位角只读无法赋值
            var directionalAntenna = Context.CurrentProject.Network.Devices.OfType<IDirectionalAntenna>().First();
            directionalAntenna.SetAzimuth(10);
            directionalAntenna.SetDownTilt(20);
        }

        public void HW_Smart_IBS_StadiumModeling()
        {
            //1、matLibrary.FindMaterialById(int id)
            // -> 根据讨论，插件可以不需要使用ID。只要通过IProject.MaterialLibrary.Materials罗列出所有材质对象后给用户选择即可。当选择完成后传给其它API使用。

            //2、材质名称如何获取
            Debug.WriteLine($"获取材质名称:{Context.CurrentProject.MaterialLibrary.Materials.First().Name}");

            //4、读取配置文件（我们需要将数据库密码存在配置文件中，之前是 ICS Designer.dat.config的ConnectionString节点下）
            // -> 由插件使用自己的config文件 参考：https://stackoverflow.com/questions/5190539/equivalent-to-app-config-for-a-library-dll

            //为什么Ground材质必须是Concrete (Heavy)而不能是Concrete (Medium)
            //希望建筑类型与材质能自由组合
            // -> 经过讨论，我们决定暂时通过额外的方法来创建这样的墙，而不需要插件指定墙体模板
            var wallMaterial = Context.CurrentProject.MaterialLibrary.Materials
                .First(m => m.Name == "my wall material");
            Context.CurrentProject.Buildings.First().Floors.First().AddBuildingPlane(PlaneType.Wall, wallMaterial, new[]
            {
                new Point3D(0, 0, 0),
                new Point3D(10, 0, 0),
                new Point3D(10, 0, 3),
                new Point3D(0, 0, 3)
            });
        }

        public void HW_Smart_IBS_CopyFloorsDevices()
        {
            //1、建筑别名如何获取？Name是否建筑别名
            // -> 注意现在建筑有两个名字，从软件界面中看到的是 Building Name和 Building Code。它们分别可以在模板的命名空间规则定义中以 {BuildingName} 和 {BuildingCode} 引用到。
            var building = Context.CurrentProject.Buildings.First();
            Debug.WriteLine($"{building.Name}为软件界面中看到的Building Name，界面中可编辑，插件中可编辑");
            Debug.WriteLine($"{building.Code}为软件界面中看到的Building Code，界面中可编辑");
            Debug.WriteLine($"{building.ID}为Building的唯一ID，不可编辑");

            //2、设备、线缆、穿墙点标签如何获取，并赋值（复制楼层设备至其他楼层，需要将设备本身、标签位置，旋转角度、隐藏等位置信息一并复制）
            var copyToCommandArgs = new CopyToCommandArgs(
                Context.CurrentProject.Buildings.First().Floors.Where(f => f.Number == 2 || f.Number == 3),  // 拷贝到2楼和3楼
                Context.Presenter.FloorPresentations.First(f => f.Floor.Number == 1).GetSelectedEntities()); // 将1楼选择的对象拷贝过去
            Context.CommandFactory
                .CreateCommand<ICopyToCommand, CopyToCommandArgs>(copyToCommandArgs)  // 根据上面的参数创建命令
                .Execute(); // 执行这条命令后将1楼选择的对象拷贝到2楼和3楼

            // -> 另外一种方案
            var f1 = Context.Presenter.FloorPresentations.First(f => f.Floor.Number == 1);
            var f2 = Context.Presenter.FloorPresentations.First(f => f.Floor.Number == 1);
            var clonedEntities = f1.Entities
                .OfType<INetworkComponentEntity<IDevice>>()
                .Select(entity =>
                {
                    var cloned = entity.CloneTo(new DevicePosition(
                        Context.CurrentProject.Buildings.First().Floors.First(f => f.Number == 3),
                        entity.GetNetworkComponent().Position.Location));
                    return Tuple.Create(entity, cloned);
                }).ToList();
            clonedEntities = f2.Entities
                .OfType<INetworkComponentEntity<IDevice>>()
                .Select(entity =>
                {
                    var cloned = entity.CloneTo(new DevicePosition(
                        Context.CurrentProject.Buildings.First().Floors.First(f => f.Number == 4),
                        entity.GetNetworkComponent().Position.Location));
                    return Tuple.Create(entity, cloned);
                }).Union(clonedEntities).ToList();

            foreach (var cableEntity in f1.Entities.OfType<ICableEntity>().Union(f2.Entities.OfType<ICableEntity>()))
            {
                var cloned = cableEntity.Clone();
                cloned.GetNetworkComponent().SetMobileDevice(clonedEntities
                    .First(t => t.Item1.GetNetworkComponent() == cableEntity.GetNetworkComponent().MobileDevice).Item2.GetNetworkComponent());
                cloned.GetNetworkComponent().SetSourceDevice(clonedEntities
                    .First(t => t.Item1.GetNetworkComponent() == cableEntity.GetNetworkComponent().SourceDevice).Item2.GetNetworkComponent());
            }

            //3、设备布放高度如何获取（devices[0].Position.Location.Z）
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();
            Debug.WriteLine($"{device.Name}的高度为{device.Position.Location.Z}");
        }

        public void HW_Smart_IBS_Survey1Import()
        {
            //当前界面是建筑视图、网络视图或者网络系统设计视图如何判断
            // -> 参考上面 BSM、FLD、NSD视图控制插件是否启用

            //背景图如何导入？CreateBackground方法没有设置楼层属性，如何将背景图导入指定楼层
            Context.CurrentProject.Buildings.First().Floors.First().SetBackground(
                Context.CurrentProject.Backgrounds.First());

            //LayoutPolygonRegion 添加区域
            var floorView = Context.Presenter.FloorPresentations.First();
            var newRegion = floorView
                .AddRegion(new Point(0, 0), new Point(10, 0), new Point(10, 10), new Point(0, 10))
                .region;  // 获取到新添加的Region
            newRegion.SetName("区域的自定义名称");  // 设置区域的自定义名称

            //LayoutLine 添加线
            floorView.AddLine(new Point(10, 10), new Point(50, 50), String.Empty);

            //LayoutText 添加文本框
            var text = floorView.AddText(new Rect(0, 0, 10, 10), "some text").textEntity;
            // 设置文本框字体
            text.SetFont(SystemFonts.MenuFont);

            //LayoutShapeLine 添加线
            // -> 不再区分LayoutLine和LayoutShapeLine，而是插件用下面的方式自己调整样式
            floorView.AddLine(new Point(10, 10), new Point(50, 50),
                LineShape.Normal // 样式为普通线
                    .WithLineWidth(3) // 宽度为3像素
                    .WithLineStyle(LineStyle.Dot) // 线型为点线
                    .WithColor(Colors.Red));  // 线的颜色

            //Line添加remark
            floorView.AddLine(new Point(10, 10), new Point(50, 50), LineShape.Normal, "remark text");

            //LayoutPointDisp  添加点
            var point = floorView.AddPoint(new Point(10, 10)).point;
            point.SetName("custom point name");

            //当前选中的对象
            Debug.WriteLine($"当前选中{floorView.GetSelectedEntities().Count()}个对象");
            Debug.WriteLine($"当前选中的第一个设备的ID为{floorView.GetSelectedEntities().First().ID}");

            //FLD、NSD界面右键加菜单
            // -> 参考ContextMenuPluginDemo
            
            //fldView.AddRegion、fldView.AddText、floorView.AddLine、floorView.AddPoint 需要返回值，返回添加的区域、文本框、线、点对象；该区域、文本框、线、点如何设置建筑、楼层、自定义名称属性
            // -> 现在这些方法返回一个元祖，分别为新的楼层窗口（IFloorPresentation）和新添加的对象

            //LengthConverter.Instance.Display2Pixel（找个方法作用是什么，如何替换）转化实际坐标
            // -> 不在需要
        }

        public (IFloorPresentation floor, IEntity entity) Find(IFloorPresentation floor, IDevice device)
        {
            var entity = floor.Find(device);
            return (floor, entity);
        }

        public void HW_Smart_IBS_ExportOptimizer()
        {
            //获取天线、pRRU功率
            var pRRU = Context.CurrentProject.Network.Devices.OfType<ISource>().First();
            var antenna = Context.CurrentProject.Network.Devices.OfType<ISignalTransmitter>().First();
            antenna.GetOutputPower(pRRU.SignalSources.First(), pRRU.Ports.First(), PowerType.DownlinkChannelPower);

            // 查找这个设备上每一个端口上连接的线缆，返回一个<端口，线缆>的元祖列表
            var portCablePairs = pRRU.GetConnectedCables()
                .Select(cable =>
                {
                    var portOnThisDevice = cable.SourceDevice == pRRU ? cable.SourcePort : cable.MobilePort;
                    return Tuple.Create(portOnThisDevice, cable);
                });
            
            // 查找这个设备的1号端口是否连到下一个设备的0号端口
            if (portCablePairs
                .First(tuple => tuple.Item1.Number == 1)    // 这个设备1号端口及链接它的线缆
                .Item2                                      // 拿到这根线缆
                .SourcePort                                 // 因为1号端口是Mobile端的，所以下一个设备肯定是线缆的Source端。下一个设备上所连接的端口
                .Number == 0)                               // 判断这个端口是否为0号端口
                Debug.WriteLine($"{pRRU.Name}的1号端口连接到了下个器件的0号端口上了。");


            //获取下行频点
            Debug.WriteLine($"第一个系统的下行频点为{pRRU.SignalSources.First().Cells.First().ChannelNumber}");

            //仿真PassLoss如何获取
            var pathlossProvider = Context.CurrentProject.GetPathloss(
                pRRU.SignalSources.First().SystemTemplate.System,
                antenna,
                (antenna as IDevice).Position.Floor.ParentBuilding);
            Debug.WriteLine($"{antenna.Name}在（10,10,2.5）位置的路损为{pathlossProvider.AtPoint(new Point3D(10, 10, 2.5))}");

            //仿真分辨率如何获取
            //天线分辨率如何获取
            // -> 不需要

            //获取区域
            // -> 需要转成IFloorPresentation或者INetworkPresentation才可以获取所有的对象。
            Debug.WriteLine($"当前楼层中共有{(Context.Presenter.Current as IFloorPresentation)?.Entities.OfType<IRegionEntity>().Count()}个区域");

            //获取信源的所有制式名称
            //bbu.SignalSourceList；SystemTemplateName
            Debug.WriteLine($"{pRRU.Name}有${pRRU.SignalSources.Count()}个制式");
            Debug.WriteLine($"{pRRU.SignalSources.First().SystemTemplate.DisplayName}为第一个制式所使用的系统名称");

            //Ants[0].DeviceHeight
            Debug.WriteLine($"{pRRU.Position.Location.Z}");

            //pRRU.PortConnectArray[0]取第几个端口的线缆该如何实现
            var cableOnPort0 = pRRU.GetConnectedCables().FirstOrDefault(cable => cable.SourceDevice == pRRU
                ? cable.SourcePort.Number == 0
                : cable.MobilePort.Number == 0);
            Debug.WriteLine($"{cableOnPort0}");

            //    获取区域名称Name属性
            var region = Context.Presenter.FloorPresentations.First().Entities.OfType<IRegionEntity>().First();
            Debug.WriteLine($"区域的名字为{region.Name}");

            //"const string powerTypeName = ""ReferencePower"";
            //return antenna.GetAntPowerIntensityBySource(system, powerTypeName, true).Any();
            //天线是否输出功率"
            // -> 返回的类型为OutputPower
            var power = antenna.GetOutputPower(pRRU.SignalSources.First(), pRRU.Ports.First(), PowerType.ReferencePower);
            if (power != Double.NaN)
                Debug.WriteLine($"输出功率为{power}");
            else
                Debug.WriteLine($"无有效的输出功率");
        }

        public void HW_Smart_IBS_CMEAbutment()
        {
            //1.Project.Current.FastSimulation:是否选中快速仿真
            // -> 不需要

            //"2.RanOpt.iBuilding.LayoutModel.LayoutGroup:分组类
            Debug.WriteLine($"通过{Context.Presenter.GroupManager}来访问");

            //Project.Current.SystemDesign.LayoutLayer.GetGroupList();:获取分组"
            Debug.WriteLine($"通过{Context.Presenter.GroupManager.Groups}来访问");

            //3.dev.PortConnectArray.Any(p => p != null):设备端口集合，存放所连接的线缆集合【端口未占用时，对应项设置为null】
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();
            Debug.WriteLine($"通过{device.GetConnectedCables()}来访问");

            //4.layoutCable.Selected = true;:选中线缆
            var floorWindow = Context.Presenter.Current as IFloorPresentation;
            var antennaInFloorWindow = floorWindow.Find(device);  // 找到这个设备在楼层视图中所对应的对象。
            floorWindow.SetSelectionState(antennaInFloorWindow, SelectionState.Selected);  // 在这个楼层中选择这个设备

            //"5.LayoutSource：类
            //Project.Current.SystemDesign.layoutLayer.GetDevListByLaySource(sour);:获取信号源后所有设备集合"
            // ->  如果device为信源设备的话
            device.GetConnectedDevices(ConnectorSide.Mobile);

            //6.dev.ID:设备对应的ID属性 (Use Case: Plugin needs to show assoicated resources, e.g. an installation image for a device instance.)
            // -> 器件接口(INetworkComponent)已经有一个ID
            var deviceID = floorWindow.Entities
                .OfType<INetworkComponentEntity<INetworkComponent>>().First()  
                .GetNetworkComponent()  // 拿到窗口里的对象后也可以获取到它关联的器件
                .ID;                    // 然后可以拿到这个器件的ID。
            Debug.WriteLine($"这个器件的ID为{deviceID}");
            

            //7.fiber.Clone()；//复制一根线缆
            // TODO 等待华为提供用例

            //"8.rHUB.GetLinkCable(1, out cable, out nextrHUB, out port):
            // ->  找到所连接的所有线缆，判断哪一条是和1号端口连接的。
            var connectedCable = device.GetConnectedCables()
                .First(cable => cable.SourceDevice == device && cable.SourcePort.Number == 1 ||
                                cable.MobileDevice == device && cable.MobilePort.Number == 1);

            //获取设备端口连接的线缆及下一个设备及端口
            //public bool GetLinkCable(int id, out LayoutCable layCable, out LayoutDevice layDev, out int port); "
            // -> 同上，如果找到cable之后
            Debug.WriteLine($"{connectedCable.SourceDevice}和{connectedCable.MobileDevice}两个有一个为当前设备，有一个为下一个设备");

            //9.public List<LayoutObject> NotifyList { get; }：dev.NotifyList 
            // -> 不需要调用

            //11.public List<string> GroupID { get; set; }: pRRU.GroupID 获取某个prru对应的所有分组的ID;
            Debug.WriteLine($"通过{Context.Presenter.GroupManager.FindGroups(device)}查找所在的分组，一个设备可以在多个组里");

            //"2.LayoutDevice.PIM
            //LayoutCable.PIM
            //public string PIM { get; set; }
            //获取设备获线缆的PIM值【dev.PIM.IndexOf('@');】"
            // -> 自定义属性的获取使用下面的方法
            Debug.WriteLine($"当设备库里存在名称为PIM属性时可以通过{device.GetProperty<float>("PIM")}访问");

            //"3. //根据PartGUID查找设备库对应的设备
            //Project.Current.DeviceLibrary.FindDeviceByGuid(obj.PartGUID).Ports[nodeLink.linkIndex];"
            //"5.//根据频带数值，获取当前设备的频带对象
            //IFrequencyBand frequencyBand = ((Device)(dev.Device)).GetRange(band);
            //var per100Loss = frequencyBand.LinkProperties.FirstOrDefault(prop => prop.Unit == RanOpt.iBuilding.DBM.Units.dBPer100Meters &&
            //prop.Name == ""Loss"").Value;"
            //"6.//把frequencyBand强转成RanOpt.iBuilding.DBM.Device.DeviceBand类型
            //DeviceBand deviceBand = (DeviceBand)frequencyBand;
            ////如果deviceBand不为空，其频带公式集合为DeviceBand.PortFunctions属性，请根据输入端口号和输出端口号去确定一个合适的公式对象：
            //IPortFunction currentFunction = deviceBand.PortFunctions.FirstOrDefault(cond => cond.In == inPort && cond.Out == outPort);
            ////这里的输入端口输出端口是指的IPort.Index，获取如IDevice.Device.Ports[0].Index"
            // -> 上面这一堆实际上就是计算两个端口间的损耗插值
            var loss = device.GetLoss(device.Ports.First(p => p.ConnectorSide == ConnectorSide.Source),
                device.Ports.First(p => p.ConnectorSide == ConnectorSide.Mobile));
            Debug.WriteLine($"{device}第一个输入端口到第一个输出端口间的损耗为{loss}");

            //PIM值修改
            // -> 注意数据库中PIM属性必须配置为可ReadWrite
            var pimProperty = device.GetProperty<string>("PIM");
            device.UpdateProperty(pimProperty, "asdfasd");

            //7.LayoutObject :代表线缆，设备的基类<替换?>
            // -> 不需要
        }

        public void HW_Smart_IBS_CoverLinkBudget()
        {
        }

        public void HW_Smart_IBS_CustomFormsReport()
        {
            //1.Project.Current.CreateDate
            //Project.Current.Designer;
            //Project.Current.DesignCompany;
            Debug.WriteLine($"{Context.CurrentProject.CreateDate}");
            Debug.WriteLine($"{Context.CurrentProject.Designer}");
            Debug.WriteLine($"{Context.CurrentProject.DesignCompany}");

            //2.获取打印页【区分NSD，FLD】
            //PrintExport export = new PrintExport();
            //Dictionary<PrintViewType, Dictionary<string, string>> dicPrint = export.ExportAllPrint(dicPrintPath, ImageFormat.Png);
            Debug.WriteLine(
                $"通过{Context.Presenter.NetworkPresentation.Entities.OfType<ITitlePageEntity>()}访问NSD里的打印页");
            Debug.WriteLine(
                $"通过{Context.Presenter.FloorPresentations.First().Entities.OfType<ITitlePageEntity>()}访问NSD里的打印页");
            foreach (var titlePageEntity in Context.Presenter.FloorPresentations.First().Entities.OfType<ITitlePageEntity>())
            {
                using (var bitmap = new Bitmap(500, 500))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        titlePageEntity.Draw(graphics, new Rect(0, 0, 500, 500));
                    }
                    bitmap.Save(Path.GetTempFileName());
                    Debug.WriteLine($"已将{titlePageEntity}保存保存为图片");
                }
            }

            //3.ComprehensiveReportData reportData = new ComprehensiveReportData();
            //DateTable 平面安装图 = reportData.LoadPlaneDiagramAll("".png"", true);
            //DateTable 系统图 = reportData.LoadSchematicDiagramAll("".png"", true);
            //DateTable 仿真图 = reportData.LoadCalcAll();
            //DateTable 物料清单 = reportData.LoadEquipmentData();
            //DateTable AntEIRP = reportData.LoadEIRPDataTable(); "
            var commandArgs = new GenerateComprehensiveReportDataCommandArgs(Context.CurrentProject);
            var command = Context.CommandFactory
                .CreateCommand<IGenerateComprehensiveReportDataCommand, GenerateComprehensiveReportDataCommandArgs>(commandArgs);
            command.Execute();
            Debug.WriteLine($@"报表数据在{command.Result}中");
        }

        public void HW_Smart_IBS_GuidingPointPlacement()
        {
            // -> 这一块还需要更多的时间考虑
            //TODO 鼠标点击事件，获取坐标
            //LayoutToolType.Pointer
            //fldView.Gsc2Goc(e.Location);
            //Dialogs.ShowInfo(message);
        }

        public void HW_Smart_IBS_ToolAddExtender()
        {
            //断开线缆连接 cable.DisconnectLink(0);
            var cable = Context.CurrentProject.Network.Devices.OfType<ICable>().First();
            var source = Context.CurrentProject.Network.Devices.OfType<ISource>().First();
            cable.ConnectToSource(source, source.Ports.First());

            //线缆接头LayoutConnector
            Debug.WriteLine($"Source端的接头为{cable.SourceConnector},mobile端的接头为{cable.MobileConnector}");

            //layoutObject.Device.GenProperties（读取属性）
            Debug.WriteLine($"当设备库里存在名称为 XYZ 属性时可以通过{source.GetProperty<int>("XYZ")}访问");

            //LayoutObject
            // -> 不支持

            //线缆增加Floor
            Debug.WriteLine($"这条Cable所在楼层为{cable.Floors.First()}");
        }

        public void HW_Smart_IBS_PublicFunction()
        {
            //线缆接头是否匹配
            var cable = Context.CurrentProject.Network.Devices.OfType<ICable>().First();
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();
            Debug.WriteLine(
                $"{cable.Name}与{device.Name}的端口{device.Ports.First()}是否匹配? {cable.MobileConnector.Type.Equals(device.Ports.First().ConnectorType)}");

            //buildingPlane.Subdivisions是否buildingPlane.SubPlanes
            // -> 是的
 
            var floor = Context.CurrentProject.Buildings.First().Floors.First();
            var circleArcPlane = floor.AddCircleArcPlane(
                Context.CurrentProject.Templates.OfType<IBuildingPlaneTemplate>().First(),
                5,
                0,
                30,
                2,
                new Point3D(1, 1, 1));

            //CircleArcPlane，circlePlane.Rasterize()
            //原有的ElementPlane结构,对应现在为IBuildingPlane<IPolygonPlaneShape>
            var rasterized = circleArcPlane.Rasterize();

            //CultureInfo.CurrentCulture会不会获取到操作系统的中英文？
            // -> 不会，软件启动后会根据语言设置来设置正确的值

            //ISignalSource signalSource = null;
            //signalSource.InitFromSystemTemplate(Project.Current.GetSystemTemplateByName(iss.SystemTemplateName));
            //signalSource.Parent = sourceCopy;
            //signalSource.Power = iss.Power;
            //signalSource.NoiseFigure = iss.NoiseFigure;
            //sourceCopy.SignalSources.ToList().Add(signalSource);

            // 获取SignalSource的功率
            var signalSource = Context.CurrentProject.Network.Devices.OfType<ISource>().First()
                .SignalSources.First();
            Debug.WriteLine($"发射功率{signalSource.Power}");
            Debug.WriteLine($"噪声系数{signalSource.NoiseFigure}");
            signalSource.SetPower(30);
            signalSource.SetNoiseFigure(5);
        }

        public void HW_Smart_IBS_WireDeployment()
        {
            //ISignalSource天线配平功能
            var sourceDevice = Context.CurrentProject.Network.Devices.OfType<ISource>().First();
            var itoCommandArgs = new RunTopologyOptimizationCommandArgs();
            itoCommandArgs.SignalSource = sourceDevice.SignalSources.First();  // 设置要配平的制式
            itoCommandArgs.TargetRmsError = 10;  // 配置目标误差为10dB
            itoCommandArgs.TargetAntennas =
                sourceDevice.GetConnectedDevices(ConnectorSide.Mobile).OfType<IAntenna>()
                    .Select(ant => Tuple.Create(ant, 0D));  // 配置每个相连的天线的目标功率为 0dBm
            itoCommandArgs.CandidatePrimaryCable = Context.CurrentProject.Templates.OfType<ICableTemplate>().First();  // 选择主要线缆
            itoCommandArgs.CandidateSecondaryCable = Context.CurrentProject.Templates.OfType<ICableTemplate>().ElementAt(2);  // 选择次要线缆
            itoCommandArgs.PrimaryCableLengthThreshold = 5; // 线缆长度小于等于5米时使用主要线缆，否则使用次要线缆。
            itoCommandArgs.CandidateComponents =
                Context.CurrentProject.Templates.OfType<ICouplerTemplate>().Cast<ITemplate>()  // 使用所有的Coupler模板
                    .Concat(Context.CurrentProject.Templates.OfType<ISplitterTemplate>());  // 以及Splitter模板

            var topologyOptimizer = Context.CommandFactory
                .CreateCommand<IRunTopologyOptimizationCommand, RunTopologyOptimizationCommandArgs>(itoCommandArgs);  // 根据上面的参数创建命令
            topologyOptimizer.Execute();  // 执行自动配平

            // 配平时需要设置信源的功率
            // -> 调用下面的方法会把配平的直接提交，插件不需要另外去设置功率
            topologyOptimizer.Commit(); // 将结果提交

            //LogManager.GetCurrentClassLogger()
            // -> 插件自己引用NLog.dll，这是一个开源组件

            //ant.GetPortList(RanOpt.iBuilding.DBM.ConnectorSide.Source)查找输出端口个数"
            var antenna = Context.CurrentProject.Network.Devices.OfType<IAntenna>().First();
            Debug.WriteLine($"{antenna.Name}的信源端的端口数为{antenna.Ports.Count(a => a.ConnectorSide == ConnectorSide.Source)}");


            // 桥架PenWidth属性
            var floorWindow = Context.Presenter.FloorPresentations.First();
            var cableTray = floorWindow.AddCableTray(
                new Point(10, 10), new Point(20, 20),
                LineStyle.DashDot, Colors.DarkGray, 50).cableTrayEntity;
            cableTray.SetName("custom cable tray name");
            Debug.WriteLine($"桥架的ID为{cableTray.ID}");
            Debug.WriteLine($"桥架的线宽为{cableTray.Width}");

            // 删除桥架
            foreach (var cableEntity in floorWindow.Entities.OfType<ICableTrayEntity>())
            {
                floorWindow.RemoveCableTrayEntity(cableEntity);
            }

            /*
string templateGUID = templateLine.Paramters["DeviceGUID"].Value;
IDevice device = (IDevice)Project.Current.DeviceLibrary.FindDeviceByGuid(templateGUID);
IList<IFrequencyBand> bands = device.FrequencyBands;
IProperty coupledSun = null;
IProperty couplingSun = null;
foreach (var band in bands)
{
    couplingSun = device.GetFrequencyProperty(band, "Coupling");
    coupledSun = device.GetFrequencyProperty(band, "Coupled");
if (coupledSun != null && couplingSun != null)
    break;
}
if (couplingSun != null && coupledSun != null && !CoupleLoss.Keys.Contains<int>(Convert.ToInt32(couplingSun.DisplayValue)))
{
    int couplingSunDisplayValue = Convert.ToInt32(couplingSun.DisplayValue);
    float coupledSunSunDisplayValue = Convert.ToSingle(coupledSun.DisplayValue);
    CoupleLoss.Add(couplingSunDisplayValue, coupledSunSunDisplayValue);
}
             */
            sourceDevice.GetProperty<float>("Coupling", sourceDevice.FrequencyBands.First(), LinkDirection.Down);
            sourceDevice.GetProperty<float>("Coupled", sourceDevice.FrequencyBands.First(), LinkDirection.Up);
        }

        public void HW_Smart_IBS_NetPlugin()
        {
            var cable = Context.CurrentProject.Network.Devices.OfType<ICable>().First();
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();

            //5.rHub.GetValidPortNum(ConnectorSide.Mobile) + 2;
            Debug.WriteLine($"等效于{device.GetConnectedCables().Count(cab => cab.SourceDevice != null && cab.MobileDevice != null)}");

            //6.桥架
            //LayoutLine：类 IsBridge【标志】【所属楼层，建筑】
            //[StartPoint起始点, EndPoint终点]
            //获取界面所有架信息"
            var cableTrays = Context.Presenter.FloorPresentations.First().Entities.OfType<ICableTrayEntity>();
            Debug.WriteLine($"共有{cableTrays.Count()}个桥架");
            Debug.WriteLine($"桥架的ID为{nameof(ICableTrayEntity.ID)}");

            //7.当线缆通过穿墙点连接器件时，向线缆添加中间关键点ChangeCabCrossPtList
            // -> 没有穿墙点的感念，当线缆定点的Z值不一样的时候，可以认为就有个穿墙点了。
            // -> 通过前面的例子插入顶点到线缆中。

            //8.layoutCable.IsMultiFloor() //线穿过多楼层
            Debug.WriteLine($"线缆是否跨楼层:{cable.Floors.Count() > 1}");

            //9.BaseLayoutView viewA = Project.Current.SystemDesign.GetLayoutByName(_buildingName, DevA.AllocatedFloorID)：获取视图
            Debug.WriteLine($"楼层窗口B1_F1为{Context.Presenter.FloorPresentations.First(f => f.Floor.ParentBuilding.Name == "B1" && f.Floor.Number == 1)}");

            //10.改变线缆中点点【a,...b】[a,b为原线缆的起点，终点cab.NotifyMovement();
            // -> 通过前面的例子插入顶点到线缆中。

            //11.如何判断区分当前视图
            // -> 参考前面的例子。
        }

        public void HW_Smart_IBS_NetOptimize8ConnectRhubToBBUOrDCU()
        {
            //BindingNamePre
            // -> 从你的调用中发现应该是去查找没有分配楼层的对象
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();
            Debug.WriteLine($"{device.Position.Floor == null}就表示BindingNamePre为空");

            //HasLinkObject
            //for (int i = 2; i < rHUB.Ports.Count(); i++)
            //{
            //    if (!rHUB.HasLinkObject(i))
            foreach (var devicePort in device.Ports)
            {
                if (device.GetConnectedCables()
                    .Any(cable => cable.SourceDevice == device
                        ? cable.SourcePort == devicePort
                        : cable.MobilePort == devicePort))
                {
                    Debug.WriteLine($"{device.Name}的{devicePort.Number}号端口上有线缆连着");
                }
            }
        }

        public void HW_Smart_IBS_NetOptimize7DividingCellEquipment()
        {
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();
            //设备添加至分组

            var groupManager = Context.Presenter.GroupManager;
            IGroup group1, group2;
            (groupManager, group1) = groupManager.CreateGroup();
            (groupManager, group2) = groupManager.CreateGroup();

            group1.Add(device);
            group2.Add(device);

            Debug.WriteLine($"小区ID为{group1.CellID}");
            Debug.WriteLine($"选择的制式为{group1.SystemNetwork}");

            Debug.WriteLine($"这个设备在{groupManager.FindGroups(device).Count()}个组中");

            //删除分组
            groupManager.RemoveGroup(group1);

            // 导航界面如何增加插件
            // TODO 暂不支持导航界面，所以无法增加插件

            //设备是否锁定 是否显示，group 是否锁定 是否显示 dev.ObjIsLock = layoutGroup.IsLocked; dev.ObjIsVisible = layoutGroup.ObjIsVisible;
            // -> 这一条是不是把dev放到那个group就可以？
        }

        public void HW_Smart_IBS_NetOptimize()
        {
            //1.设备【NameIndex】属性，以及与设备相连的线缆及下一设备，端口
            var rHubs = Context.CurrentProject.Network.Devices.OfType<IDevice>()
                .Where(d => d.Template.DisplayName.Contains("rHub")) // 筛选所有使用rHub模板的设备，即所有的 rHubs
                .OrderBy(d => d.NameIndex)
                .ToList();
            foreach (var cable in rHubs[0].GetConnectedCables())
            {
                if (cable.SourceDevice == rHubs[0])
                    Debug.WriteLine($"这条线缆的Source端连的是这个rHub的{cable.SourcePort}端口");
                else if (cable.MobileDevice == rHubs[0])
                    Debug.WriteLine($"这条线缆的Source端连的是这个rHub的{cable.MobilePort}端口");
            }
        }

        public void HW_Smart_IBS_NetOptimize1NetInitialSetting()
        {
            // 【LayoutView 类】【视图】:
            // -> 我没找到LayoutView上面有CurrentLayoutView这个属性，是不是搞错了？
            // -> 如果想获取当前的视图，可以使用
            Debug.WriteLine($"当前的视图为{Context.Presenter.Current}");

            //【LayoutPolygonShape 类】【LayoutText 类】 
            //((LayoutPolygonShape) layoutObject).TagString = "IsSector";//这是小区
            //((LayoutPolygonShape) layoutObject).UpdateTessellation();
            //IList<Point3D> _PolygonShapeVertexs = null;
            //_PolygonShapeVertexs = polyShape.PolyRegion.Contour.FirstOrDefault().Vertex
            //【获取扇区 扇区文本】
            //List<LayoutObject> oLst = CurrentLayoutView.LayoutLayer.LayList;【获取当前视图下所有的设备】
            //foreach (var o in oLst)
            //{
            //    if (o is LayoutPolygonShape && o.TagString != null && o.TagString.Contains("IsSector"))
            //    {}
            //    else if (o is LayoutText && o.Text != null && o.Text.Contains("Sector"))
            //    {}
            //    }
            // TODO 等待华为明确需求及用例
        }

        public void HW_Smart_IBS_NetOptimize2CellDividing()
        {
            //1.LayoutView fldView = activeControl as LayoutView;
            // fldView.LayMode == LayoutMode.LayoutLD
            if (Context.Presenter.Current is IFloorPresentation)
            {
                // 代表的当前窗口是楼层窗口
                Debug.WriteLine($"当前楼层窗口的编辑模式为{((IFloorPresentation)Context.Presenter.Current).DisplayMode}");
            }

            //2.是否可以提供直接的接口判断当前视图是楼层视图
            //foreach (var device in fldView.LayoutLayer.LayList)
            ////判断是否处于楼层视图下，其他视图不允许画扇区【电梯视图下也不能画扇区】
            //var SdLayoutView = _Host.GetActiveView();
            //var sdView = SdLayoutView as SDLayoutView;
            // if (SdLayoutView.GetType().Name == ""SdEditControl"" && sdView != null && !sdView.IsElevator)
            //{return;}
            //
            //var fldViewTypeName = fldView.GetType().Name;
            //if (fldViewTypeName != ""FldEditControl"" && fldViewTypeName != ""SdEditControl"")
            //{return;}
            // -> 如果觉得目前的方式不方便，插件可以定义自己的扩展方法来实现，比如
            Debug.WriteLine($"当前是否为楼层视图:{Context.IsOnFloorWindow()}");

            //3.//激活工具Tool
            //fldView.ActiveTool = LayoutToolType.PolygonShape;
            ////准备截获
            //PrepareIntercept(fldView);
            // TODO 等待华为明确需求及用例：需要实现什么功能？

            //4.视图对应的事件
            //LayoutView.CurrentDeviceToolChanged -= LayoutView_CurrentDeviceToolChanged;
            //LayoutView.CurrentDeviceToolChanged += LayoutView_CurrentDeviceToolChanged;
            // TODO 暂不支持在工具切换的时候通知插件

            //5.
            //#region 触发多边形删除事件辅助机制
            //        /// <summary>
            //        /// 项目关闭事件，反注册LayoutChanged事件
            //        /// </summary>
            //        /// <param name=""sender""></param>
            //        /// <param name=""e""></param>
            //        private void context_ProjectClosing(object sender, EventArgs e)
            //        {
            //            Project.Current.SystemDesign.LayoutLayer.LayoutChanged -= LayoutLayer_LayoutChanged;
            //            Project.Current.SystemDesign.LayoutLayer.LayUndoRedo.CommandDone -= LayUndoRedo_CommandDone;
            //        }
            //        private void LayUndoRedo_CommandDone(object sender, DejaVu.CommandDoneEventArgs e)
            //        {
            //            if (e.CommandDoneType == DejaVu.CommandDoneType.Undo)
            //            {
            //                return;
            //            }
            //        }
            //        /// <summary>
            //        /// 项目打开事件，注册LayoutChanged事件
            //        /// </summary>
            //        /// <param name=""sender""></param>
            //        /// <param name=""e""></param>
            //        private void context_ProjectOpened(object sender, EventArgs e)
            //        {
            //            Project.Current.SystemDesign.LayoutLayer.LayoutChanged += LayoutLayer_LayoutChanged;
            //            Project.Current.SystemDesign.LayoutLayer.LayUndoRedo.CommandDone += LayUndoRedo_CommandDone;
            //        }
            //#endregion
            // TODO 等待华为明确需求及用例

            //6.增删改设备对应事件需平台提供的接口：
            //void LayoutLayer_LayoutChanged(object sender, LayoutArgs e)
            //{
            //            Control activeControl = _Host.GetActiveView();
            //            LayoutView fldView = activeControl as LayoutView;
            //            LayoutLayer layoutLayer = Project.Current.SystemDesign.LayoutLayer;
            //            if (e.Action == LayoutEventType.Add)
            //            {...}
            //            else if (e.Action == LayoutEventType.Delete)
            //            {
            //                ...
            //                if (fldView != null)
            //                {
            //                    fldView.ActiveTool = LayoutToolType.Pointer;
            //                }
            //            }
            //            else if (e.Action == LayoutEventType.Move)
            //            {
            //                var cells = e.Items.Where(item => item.TypeCode.Contains(""LayoutPolygonShape"")).ToList();
            //            }
            //        }
            // TODO 暂不支持在对象增删改等操作时通知插件

            //7.
            //所有pRRU已划分扇区，再按ctrl+z回退操作，再次移动pRRU，​所有扇区中的pRRU不能移动
            ////需要切换一下鼠标工具才能解决，
            ////移动界面设备，再做Ctrl+Z操作，进行回退，当回退触发layoutChanged事件后，
            ////如果当前界面失去焦点，则界面设备被“锁定”，不能移动。当界面再次获得焦点时，设备被“解锁”，功能恢复正常
            ////期待润普解决，暂时的解决办法是切换一个鼠标工具
            ////20160412 zwx328057
            //
            //fldView.ActiveTool = LayoutToolType.MoveHand;
            //fldView.ActiveTool = LayoutToolType.Pointer;
            //
            // 
            //layObj is RanOpt.iBuilding.LayoutModel.LayoutAnt
            //                    && layObj.Device != null && layObj.Device.Model.Contains(""pRRU"")
            // -> 上面的工具切换是为了解决上面所说的bug，暂时先不处理。等bug重现的时候再考虑怎么解决。

            //8.【LayoutAnt】
            //if(layObj is RanOpt.iBuilding.LayoutModel.LayoutAnt
            //&& layObj.Device != null && layObj.Device.Model.Contains(""pRRU""))
            //{}
            Debug.WriteLine($"{Context.CurrentProject.Network.Devices.OfType<IAntenna>().Where(ant => ant.Template.Model.Contains("pRRU"))}");
        }

        public void HW_Smart_IBS_NetOptimize4ConnectPrrusToRhub()
        {
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();
            //1.【LayoutActive】
            //if (device is LayoutActive)
            Debug.WriteLine($"this device is a Fiber Repeater: {device.Template is IFiberRepeaterTemplate}");
            
            var group = Context.Presenter.GroupManager.Groups.First();
            // -> 分组ID目前没有，如果要确定唯一性，可以使用Name
            Debug.WriteLine($"分组的唯一名称为{group.Name}");
            Debug.WriteLine($"小区ID为{group.CellID}");
            Debug.WriteLine($"选择的制式为{group.SystemNetwork}");
            
            //public List<string> GroupID { get; set; }
            Debug.WriteLine($"获取设备所在的分组:{Context.Presenter.GroupManager.FindGroups(device)}");

            //1、判断设备是否被选中 if (!device.Selected)
            var matchingEntity = Context.Presenter.FloorPresentations.Select(f => Find(f, device)).FirstOrDefault();
            if (matchingEntity.entity != null && matchingEntity.entity.Selected == SelectionState.Selected)
            {
                Debug.WriteLine($"{device.Name}在楼层窗口{matchingEntity.floor.Floor.Name}中已经被选中");
            }
        }

        public void HW_Smart_IBS_Survey2Export()
        {
            var cable= Context.CurrentProject.Network.Devices.OfType<ICable>().First();
            var nsdCable = (ICableEntity)Context.Presenter.NetworkPresentation.Find(cable);
            var device = Context.CurrentProject.Network.Devices.OfType<IDevice>().First();
            var fldEntity = Context.Presenter.FloorPresentations.First().Find(device);
            var nsdEntity = Context.Presenter.NetworkPresentation.Find(device);

            //FLD、NSD下设备的旋转角度
            //var FLD = Context.Presenter.FloorPresentations.First(f => f.Floor.Name == floor.Name);
            //var nsd = Context.Presenter.NetworkPresentation; 
            Debug.WriteLine($"{device.Name}在楼层窗口中的旋转角度是{fldEntity.RotationAngle}");
            Debug.WriteLine($"{device.Name}在NSD窗口中的旋转角度是{nsdEntity.RotationAngle}");

            //NSD下设备的坐标item.Rectangles[(int)LayoutMode.LayoutSD].X
            Debug.WriteLine($"{device.Name}在NSD窗口中的旋转角度是{nsdEntity.RotationAngle}");

            //Project.Current.DeviceNameFontNsd.Name标签字体
            Debug.WriteLine($"可以通过{nameof(IEntity.Labels)}访问所有相关的标签，名字等，比如{fldEntity.Labels.First().Font}");

            //干线放大器类型、模板类型
            Debug.WriteLine($"干线放大器的类型为{nameof(ITrunkAmplifier)}，模板类型为{nameof(ITrunkAmplifierTemplate)}");

            //LayoutDevice.Device.DeviceTypeNew; GetType（）
            // -> 不需要，全部使用对应接口类型来处理

            //NSD下线缆的拐点坐标LayoutCable.PointArray[(int)LayoutMode.LayoutSD]
            Debug.WriteLine($"{nsdCable.Corners}");

            //LayoutCable.CableLengthLabelPosition
            var fixedLabel = nsdCable.Labels.OfType<IFixedLabel>().First(label => label.Descriptor == LabelDescriptor.CableLengthLabel);
            Debug.WriteLine($"长度标签的位置为{fixedLabel.HorizontalAlignment}");

            //Project.Current.CableLengthFontSystemMode.Name
            Debug.WriteLine($"长度标签的字体为{fixedLabel.Font}");

            //馈线模板是什么
            Debug.WriteLine($"{typeof(ICableTemplate)}");

            //项目保存事件ProjectSaving
            Debug.WriteLine($"保存完成之后触发的事件{nameof(Context.ProjectSaved)}");
        }

        public void HW_Smart_IBS_RgbGroupProject()
        {
            //VPlaneShape newWallShape = new VPlaneShape(bsmView.Controller.Model);
            //newWallShape.SuspendSync();
            //newWallShape.Type = planType;
            //newWallShape.Thickness = thickness;
            //newWallShape.Height = height;
            //newWallShape.Start = pt1;
            //newWallShape.StartTop = height;
            //newWallShape.StartBottom = 0f;
            //newWallShape.End = pt2;
            //newWallShape.EndTop = height;
            //newWallShape.EndBottom = 0f;
            //newWallShape.Material = material;
            //newWallShape.ResumeSync(false);
            var building = Context.CurrentProject.Buildings.First(b => b.Name == "B1");
            var floor = building.Floors.First(f => f.Name == "B1_F1");
            var material = Context.CurrentProject.MaterialLibrary.Materials.First();
            // 添加墙体
            var wall = floor.AddBuildingPlane(PlaneType.Wall, material, new[]
            {
                new Point3D(0, 0, 0),
                new Point3D(10, 0, 0),
                new Point3D(10, 0, 3),
                new Point3D(0, 0, 3)
            });
            // 墙上添加门
            wall.AddDoor(material, new PolygonPlaneShape(new[]
            {
                new Point3D(4, 0, 0),
                new Point3D(5, 0, 0),
                new Point3D(5, 0, 2),
                new Point3D(4, 0, 2)
            }));
            // 墙上添加窗
            wall.AddWindow(material, new PolygonPlaneShape(new[]
            {
                new Point3D(6, 0, 1),
                new Point3D(7, 0, 1),
                new Point3D(7, 0, 2),
                new Point3D(6, 0, 2)
            }));
        }

        public void HW_Smart_IBS_AntennaDeployment()
        {
            //SignalSources.System.Band; 是否是signal.SystemTemplate.System.Band？
            // -> 是的

            //floor.FldGuid是不是floor的id  是string 类型？
            // -> 这个属性是界面层相关的一个属性，不应该在这个接口上。插件应该是不需要使用它的。

            //signal.System.Type.ToString()
            var systemTemplate = Context.CurrentProject.Templates.OfType<ISystemTemplate>().First();
            Debug.WriteLine($"这个系统模板所使用的系统类型是{systemTemplate.System.Type}");
        }

        public void HW_Smart_IBS_自动连线()
        {
            //移动穿墙点
            var cableCrosses = Context.Presenter.FloorPresentations.First().Entities.OfType<ICableCross>();
            foreach (var cableCross in cableCrosses)
            {
                Debug.WriteLine($"还未链接的穿墙点位置为{cableCross.Position}");
            }
        }
    }
}