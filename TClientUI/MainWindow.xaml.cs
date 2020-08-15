using Common.Normal;
using Common.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vector = Common.Protobuf.Vector;

namespace TClientUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 放大的倍数 为了UI显示更清晰
        private readonly int WEIGHT = 3;
        private bool isKeyUp = true;
        public Dictionary<int, Ellipse> DicRole = new Dictionary<int, Ellipse>();
        public static TClient tClient;
        public Role SelfRole = new Role();
        public ConcurrentQueue<MoveStruct> EllipseList = new ConcurrentQueue<MoveStruct>();
        public ConcurrentQueue<MoveStruct> WillBeRemoveEllipseList = new ConcurrentQueue<MoveStruct>();
        public Common.NavAuto.MapData mapData;
        private bool isRunning = true;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tClient = new TClient(this);
            tClient.StartConnect();

            mapData = new Common.NavAuto.MapData(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.Combine("MapData", "101")));
            mapData.Init();

            this.Dispatcher?.Invoke(() =>
            {
                for (var i = 0; i < mapData.Width; ++i)
                {
                    for (var j = 0; j < mapData.Length; ++j)
                    {
                        if (mapData.map[i, j] == 1)
                            RoleMap.Children.Add(CreateEllipse(1, 1, i, j));
                    }
                }
            });


            Task.Run(() =>
            {
                while (isRunning)
                {
                    RefreshWindow();
                    Thread.Sleep(30);
                }
            });
        }

        public void RefreshWindow()
        {
            this.Dispatcher?.Invoke(() =>
            {
                var count = EllipseList.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!EllipseList.TryDequeue(out var e)) continue;
                    if (!RoleMap.Children.Contains(e.ellipse))
                    {
                        RoleMap.Children.Add(e.ellipse);
                    }
                    Canvas.SetLeft(e.ellipse, e.x * WEIGHT);
                    Canvas.SetTop(e.ellipse, e.y * WEIGHT);
                }

                count = WillBeRemoveEllipseList.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!WillBeRemoveEllipseList.TryDequeue(out var e)) continue;
                    RoleMap.Children.Remove(e.ellipse);
                }

                DebugLog.Text = $"cur position ({SelfRole.X},{SelfRole.Y})";
            });
        }

        private Ellipse CreateEllipse(double width, double height, double desiredCenterX, double desireCenterY)
        {
            var ellipse = new Ellipse
            {
                Width = width * WEIGHT,
                Height = height * WEIGHT,
                Margin = new Thickness
                {
                    Left = desiredCenterX * WEIGHT - (width * WEIGHT / 2),
                    Top = desireCenterY * WEIGHT - (height * WEIGHT / 2)
                },
                Fill = new SolidColorBrush(Color.FromScRgb(1f, 255f, 255f, 255f))
            };
            return ellipse;
        }

        public void AddToCanvas(int id, double x, double y)
        {
            this.Dispatcher?.Invoke(() =>
            {
                var e = CreateEllipse(1d, 1d, 0d, 0d);
                DicRole.Add(id, e);
                RoleMap.Children.Add(e);
                Canvas.SetLeft(e, x * WEIGHT);
                Canvas.SetTop(e, y * WEIGHT);
            });
        }

        public void AddToCanvas(Ellipse ellipse)
        {
            this.Dispatcher?.Invoke(() =>
            {
                RoleMap.Children.Add(ellipse);
            });
        }

        public void UpdateCanvas(Ellipse e, double x, double y)
        {
            this.Dispatcher?.Invoke(() =>
            {
                Canvas.SetLeft(e, x * WEIGHT);
                Canvas.SetTop(e, y * WEIGHT);
            });
        }

        public void UpdateRoleListOnCanvas(List<EntityInfo> roleInfoList)
        {
            foreach (var role in roleInfoList)
            {
                if (!DicRole.TryGetValue(role.Id, out var ellipse))
                {
                    if (ellipse == null)
                    {
                        AddToCanvas(role.Id, role.X, role.Y);
                    }
                    else
                    {
                        AddToCanvas(ellipse);
                    }
                    continue;
                }

                if (role.Id == SelfRole.Id)
                {
                    SelfRole.X = role.X;
                    SelfRole.Y = role.Y;
                }

                EllipseList.Enqueue(new MoveStruct { ellipse = ellipse, x = role.X, y = role.Y });
            }
        }

        public void UpdateRoleSight(EntityInfo roleInfo, S2CSight.Types.ESightOpt opt)
        {
            DicRole.TryGetValue(roleInfo.Id, out var ellipse);

            switch (opt)
            {
                case S2CSight.Types.ESightOpt.EnterSight:
                    if (ellipse == null) { AddToCanvas(roleInfo.Id, roleInfo.X, roleInfo.Y); break; }
                    EllipseList.Enqueue(new MoveStruct { ellipse = ellipse, x = roleInfo.X, y = roleInfo.Y });
                    break;
                case S2CSight.Types.ESightOpt.LeaveSight:
                    WillBeRemoveEllipseList.Enqueue(new MoveStruct { ellipse = ellipse, x = roleInfo.X, y = roleInfo.Y });
                    break;
                default: break;
            }
        }

        private void RoleLogin(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Released) return;
            TClient._client.StartSend(new C2SLogin { Name = "tiaoyu", Password = "tiaoyu" });
            LoginBtn.IsEnabled = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!isKeyUp)
            {
                return;
            }
            isKeyUp = false;
            double speed = 10;
            var v = new Vector
            {
                X = 0D,
                Y = 0D,
                Z = 0D
            };
            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                    v.Y = -1D;
                    break;
                case Key.S:
                case Key.Down:
                    v.Y = 1D;
                    break;
                case Key.A:
                case Key.Left:
                    v.X = -1D;
                    break;
                case Key.D:
                case Key.Right:
                    v.X = 1D;
                    break;
                default:
                    return;
            }

            TClient._client.StartSend(new C2SMove
            {
                IsUsePosition = false,
                Speed = speed,
                Orientation = v,
            });
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                case Key.S:
                case Key.Down:
                case Key.A:
                case Key.Left:
                case Key.D:
                case Key.Right:
                    isKeyUp = true;
                    TClient._client.StartSend(new C2SStopMove());
                    break;
            }
        }
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(null);
            TClient._client.StartSend(new C2SNavAuto { X = position.X / WEIGHT, Y = position.Y / WEIGHT });
            DebugLog.Text = position.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isRunning = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isRunning = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (LoginBtn.IsEnabled == false)
            {
                //var position = e.GetPosition(null);
                //TClient._client.StartSend(new C2SNavAuto { X = position.X / 5, Y = position.Y / 5 });
            }
        }


    }

    public class Role
    {
        public int Id;
        public double X;
        public double Y;
    }

    public class MoveStruct
    {
        public Ellipse ellipse;
        public double x;
        public double y;
    }
}
