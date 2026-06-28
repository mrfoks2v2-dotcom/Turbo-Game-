using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;

namespace UltimateSecurityApp
{
    internal static class Program
    {
        internal const string Password = "2341";
        internal static readonly DateTime StartedAt = DateTime.Now;

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (LoginForm login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new PremiumForm());
                }
            }
        }
    }

    internal sealed class LoginForm : Form
    {
        private readonly TextBox passwordBox;

        public LoginForm()
        {
            Text = "Ultimate Security - вход";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 300);
            BackColor = Color.FromArgb(12, 16, 24);
            ForeColor = Color.White;
            Font = Ui.Font(10f, FontStyle.Regular);
            Icon = AppImages.AppIcon();

            GradientPanel top = new GradientPanel();
            top.Location = new Point(0, 0);
            top.Size = new Size(520, 118);
            top.StartColor = Color.FromArgb(10, 16, 28);
            top.EndColor = Color.FromArgb(92, 64, 22);
            Controls.Add(top);

            PictureBox avatar = new PictureBox();
            avatar.Image = AppImages.Avatar(76);
            avatar.SizeMode = PictureBoxSizeMode.CenterImage;
            avatar.Location = new Point(28, 20);
            avatar.Size = new Size(84, 84);
            top.Controls.Add(avatar);

            Label title = new Label();
            title.Text = "Ultimate SecurityApp";
            title.Font = Ui.Font(22f, FontStyle.Bold);
            title.AutoSize = true;
            title.ForeColor = Color.White;
            title.Location = new Point(125, 28);
            top.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Приветствую вас. Введите пароль для авторизации";
            subtitle.Font = Ui.Font(10f, FontStyle.Regular);
            subtitle.ForeColor = Color.FromArgb(238, 211, 151);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(129, 70);
            top.Controls.Add(subtitle);

            Label passwordLabel = new Label();
            passwordLabel.Text = "Пароль";
            passwordLabel.AutoSize = true;
            passwordLabel.ForeColor = Color.FromArgb(210, 217, 229);
            passwordLabel.Location = new Point(58, 158);
            Controls.Add(passwordLabel);

            passwordBox = new TextBox();
            passwordBox.Location = new Point(61, 184);
            passwordBox.Size = new Size(290, 28);
            passwordBox.UseSystemPasswordChar = true;
            Controls.Add(passwordBox);

            Button loginButton = Ui.GoldButton("Войти", 368, 181, 102);
            loginButton.Click += LoginButtonClick;
            Controls.Add(loginButton);

            Label note = new Label();
            note.Text = "Ultimate-версия просит права администратора для максимально полного просмотра ПК.";
            note.ForeColor = Color.FromArgb(143, 154, 171);
            note.AutoSize = true;
            note.Location = new Point(58, 235);
            Controls.Add(note);

            AcceptButton = loginButton;
            Shown += delegate { passwordBox.Focus(); };
        }

        private void LoginButtonClick(object sender, EventArgs e)
        {
            if (passwordBox.Text == Program.Password)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MessageBox.Show(this, "Неверный пароль", "Вход", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            passwordBox.SelectAll();
            passwordBox.Focus();
        }
    }

    internal sealed class PremiumForm : Form
    {
        private const int ProcessPathColumn = 10;
        private const int FilePathColumn = 5;

        private readonly ImageList imageList;
        private readonly Label statusLabel;
        private readonly Label processCountLabel;
        private readonly Label cpuLabel;
        private readonly Label ramLabel;
        private readonly Label tempLabel;
        private readonly TextBox detailsBox;
        private readonly ListView processList;
        private readonly ListView fileList;
        private readonly ListView sensorList;
        private readonly ListView scanList;
        private readonly TextBox aboutBox;
        private readonly Button refreshButton;
        private readonly Button fullScanButton;
        private readonly Button openFolderButton;
        private readonly Button killButton;
        private readonly Button deleteButton;
        private readonly System.Windows.Forms.Timer timer;

        public PremiumForm()
        {
            Text = "Ultimate SecurityApp";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1240, 760);
            Size = new Size(1400, 820);
            BackColor = Color.FromArgb(9, 13, 20);
            Font = Ui.Font(9.5f, FontStyle.Regular);
            Icon = AppImages.AppIcon();

            GradientPanel header = new GradientPanel();
            header.Dock = DockStyle.Top;
            header.Height = 108;
            header.StartColor = Color.FromArgb(7, 12, 20);
            header.EndColor = Color.FromArgb(100, 72, 25);
            Controls.Add(header);

            PictureBox avatar = new PictureBox();
            avatar.Image = AppImages.Avatar(68);
            avatar.SizeMode = PictureBoxSizeMode.CenterImage;
            avatar.Location = new Point(24, 18);
            avatar.Size = new Size(74, 74);
            header.Controls.Add(avatar);

            Label title = new Label();
            title.Text = "Ultimate SecurityApp";
            title.Font = Ui.Font(25f, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.AutoSize = true;
            title.Location = new Point(112, 20);
            header.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Максимальная проверка ПК, цветной риск, датчики, процессы и файловая активность";
            subtitle.Font = Ui.Font(10.5f, FontStyle.Regular);
            subtitle.ForeColor = Color.FromArgb(238, 211, 151);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(116, 62);
            header.Controls.Add(subtitle);

            statusLabel = new Label();
            statusLabel.Text = "Готово";
            statusLabel.AutoSize = true;
            statusLabel.Font = Ui.Font(10f, FontStyle.Bold);
            statusLabel.ForeColor = Color.FromArgb(255, 224, 154);
            statusLabel.Location = new Point(1050, 42);
            header.Controls.Add(statusLabel);

            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 68;
            toolbar.BackColor = Color.FromArgb(14, 20, 31);
            Controls.Add(toolbar);

            Button taskButton = Ui.DarkButton("Диспетчер задач", 20, 16, 155);
            taskButton.Click += delegate { Process.Start("taskmgr.exe"); };
            toolbar.Controls.Add(taskButton);

            Button explorerButton = Ui.DarkButton("Проводник", 188, 16, 120);
            explorerButton.Click += delegate { Process.Start("explorer.exe"); };
            toolbar.Controls.Add(explorerButton);

            refreshButton = Ui.GoldButton("Обновить", 322, 16, 118);
            refreshButton.Click += delegate { RefreshAll(); };
            toolbar.Controls.Add(refreshButton);

            fullScanButton = Ui.GoldButton("Полная проверка", 454, 16, 150);
            fullScanButton.Click += delegate { RunFullScan(); };
            toolbar.Controls.Add(fullScanButton);

            openFolderButton = Ui.DarkButton("Открыть папку", 620, 16, 130);
            openFolderButton.Enabled = false;
            openFolderButton.Click += delegate { OpenSelectedFolder(); };
            toolbar.Controls.Add(openFolderButton);

            killButton = Ui.DarkButton("Завершить", 764, 16, 110);
            killButton.Enabled = false;
            killButton.Click += delegate { KillSelectedProcess(); };
            toolbar.Controls.Add(killButton);

            deleteButton = Ui.DangerButton("Удалить", 888, 16, 105);
            deleteButton.Enabled = false;
            deleteButton.Click += delegate { DeleteSelectedApp(); };
            toolbar.Controls.Add(deleteButton);

            Button uninstallButton = Ui.DarkButton("Программы Windows", 1008, 16, 165);
            uninstallButton.Click += delegate { Process.Start("appwiz.cpl"); };
            toolbar.Controls.Add(uninstallButton);

            Panel main = new Panel();
            main.Dock = DockStyle.Fill;
            main.Padding = new Padding(18);
            main.BackColor = Color.FromArgb(9, 13, 20);
            Controls.Add(main);
            main.BringToFront();

            Panel dashboard = new Panel();
            dashboard.Dock = DockStyle.Top;
            dashboard.Height = 92;
            dashboard.BackColor = Color.FromArgb(9, 13, 20);
            main.Controls.Add(dashboard);

            processCountLabel = AddMetric(dashboard, "Процессы", "0", 0);
            cpuLabel = AddMetric(dashboard, "CPU", "нет данных", 1);
            ramLabel = AddMetric(dashboard, "RAM", "нет данных", 2);
            tempLabel = AddMetric(dashboard, "Температура", "нет данных", 3);

            imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(24, 24);
            imageList.Images.Add("default", SystemIcons.Application.ToBitmap());

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Font = Ui.Font(10f, FontStyle.Regular);
            main.Controls.Add(tabs);
            tabs.BringToFront();

            TabPage processesTab = NewTab("Все активные приложения");
            tabs.TabPages.Add(processesTab);

            processList = CreateList();
            processList.SmallImageList = imageList;
            processList.Columns.Add("Приложение", 190);
            processList.Columns.Add("PID", 70);
            processList.Columns.Add("Риск", 65);
            processList.Columns.Add("CPU", 70);
            processList.Columns.Add("RAM", 90);
            processList.Columns.Add("Сеть", 70);
            processList.Columns.Add("Диск/с", 90);
            processList.Columns.Add("Размер", 95);
            processList.Columns.Add("Видим с", 130);
            processList.Columns.Add("В ПК с", 125);
            processList.Columns.Add("Путь", 360);
            processList.Columns.Add("Причина", 430);
            processList.SelectedIndexChanged += ProcessListSelectedIndexChanged;
            processList.MouseClick += ProcessListMouseClick;
            processesTab.Controls.Add(processList);

            detailsBox = new TextBox();
            detailsBox.Dock = DockStyle.Bottom;
            detailsBox.Height = 118;
            detailsBox.Multiline = true;
            detailsBox.ReadOnly = true;
            detailsBox.ScrollBars = ScrollBars.Vertical;
            detailsBox.BackColor = Color.FromArgb(17, 24, 36);
            detailsBox.ForeColor = Color.FromArgb(226, 232, 240);
            detailsBox.BorderStyle = BorderStyle.FixedSingle;
            detailsBox.Font = Ui.Font(9f, FontStyle.Regular);
            detailsBox.Text = "Выберите процесс. Клик по пути сразу откроет папку приложения.";
            processesTab.Controls.Add(detailsBox);
            detailsBox.BringToFront();

            TabPage filesTab = NewTab("Файлы и изменения");
            tabs.TabPages.Add(filesTab);

            fileList = CreateList();
            fileList.SmallImageList = imageList;
            fileList.Columns.Add("Приложение", 190);
            fileList.Columns.Add("Всего с запуска", 130);
            fileList.Columns.Add("Сейчас диск/с", 120);
            fileList.Columns.Add("Операций", 95);
            fileList.Columns.Add("PID", 70);
            fileList.Columns.Add("Путь", 520);
            fileList.Columns.Add("Что делает", 520);
            fileList.MouseClick += FileListMouseClick;
            filesTab.Controls.Add(fileList);

            TabPage scanTab = NewTab("Полная проверка");
            tabs.TabPages.Add(scanTab);

            scanList = CreateList();
            scanList.SmallImageList = imageList;
            scanList.Columns.Add("Статус", 125);
            scanList.Columns.Add("Приложение", 190);
            scanList.Columns.Add("PID", 70);
            scanList.Columns.Add("Риск", 65);
            scanList.Columns.Add("CPU", 70);
            scanList.Columns.Add("RAM", 90);
            scanList.Columns.Add("Файлы с запуска", 130);
            scanList.Columns.Add("Путь", 430);
            scanList.Columns.Add("Почему найдено", 520);
            scanList.MouseClick += ScanListMouseClick;
            scanTab.Controls.Add(scanList);

            TabPage sensorsTab = NewTab("Датчики и характеристики");
            tabs.TabPages.Add(sensorsTab);

            sensorList = CreateList();
            sensorList.Columns.Add("Группа", 170);
            sensorList.Columns.Add("Параметр", 260);
            sensorList.Columns.Add("Значение", 360);
            sensorList.Columns.Add("Состояние", 500);
            sensorsTab.Controls.Add(sensorList);

            TabPage aboutTab = NewTab("О разработчике");
            tabs.TabPages.Add(aboutTab);

            aboutBox = new TextBox();
            aboutBox.Dock = DockStyle.Fill;
            aboutBox.Multiline = true;
            aboutBox.ReadOnly = true;
            aboutBox.ScrollBars = ScrollBars.Vertical;
            aboutBox.BackColor = Color.FromArgb(13, 19, 30);
            aboutBox.ForeColor = Color.FromArgb(226, 232, 240);
            aboutBox.BorderStyle = BorderStyle.FixedSingle;
            aboutBox.Font = Ui.Font(11f, FontStyle.Regular);
            aboutBox.Text =
                "Ultimate SecurityApp" + Environment.NewLine + Environment.NewLine +
                "Описание программы:" + Environment.NewLine +
                "Приложение показывает все активные процессы, оценивает их по нагрузке и подозрительным признакам, " +
                "следит за файловой активностью с момента запуска, показывает доступные датчики и характеристики ПК, " +
                "а также помогает открыть папку приложения, завершить процесс или удалить выбранный файл в корзину." + Environment.NewLine + Environment.NewLine +
                "Зеленый цвет - тихая и довольно безопасная программа." + Environment.NewLine +
                "Желтый цвет - стоит посмотреть внимательнее." + Environment.NewLine +
                "Красный цвет - программа выглядит опасной или сильно влияет на ПК." + Environment.NewLine + Environment.NewLine +
                "Связаться с создателем через Gmail:" + Environment.NewLine +
                "fokav080@gmail.com";
            aboutTab.Controls.Add(aboutBox);

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 12000;
            timer.Tick += delegate { RefreshAll(); };

            Shown += delegate
            {
                BeginInvoke(new MethodInvoker(delegate
                {
                    RefreshAll();
                    timer.Start();
                }));
            };
        }

        private static Label AddMetric(Control parent, string caption, string value, int index)
        {
            Panel card = new Panel();
            card.Location = new Point(index * 260, 0);
            card.Size = new Size(244, 76);
            card.BackColor = Color.FromArgb(17, 24, 36);
            parent.Controls.Add(card);

            Label title = new Label();
            title.Text = caption;
            title.ForeColor = Color.FromArgb(148, 163, 184);
            title.AutoSize = true;
            title.Location = new Point(18, 13);
            card.Controls.Add(title);

            Label data = new Label();
            data.Text = value;
            data.ForeColor = Color.FromArgb(255, 224, 154);
            data.Font = Ui.Font(17f, FontStyle.Bold);
            data.AutoSize = true;
            data.Location = new Point(18, 36);
            card.Controls.Add(data);
            return data;
        }

        private static TabPage NewTab(string title)
        {
            TabPage tab = new TabPage(title);
            tab.BackColor = Color.FromArgb(9, 13, 20);
            tab.Padding = new Padding(0, 10, 0, 0);
            return tab;
        }

        private static ListView CreateList()
        {
            ListView list = new ListView();
            list.Dock = DockStyle.Fill;
            list.View = View.Details;
            list.FullRowSelect = true;
            list.GridLines = false;
            list.HideSelection = false;
            list.BackColor = Color.FromArgb(13, 19, 30);
            list.ForeColor = Color.FromArgb(226, 232, 240);
            list.BorderStyle = BorderStyle.FixedSingle;
            list.Font = Ui.Font(9f, FontStyle.Regular);
            return list;
        }

        private void RefreshAll()
        {
            if (!refreshButton.Enabled)
            {
                return;
            }

            refreshButton.Enabled = false;
            Cursor = Cursors.WaitCursor;
            statusLabel.Text = "Сканирование...";
            Application.DoEvents();

            try
            {
                List<ProcessInfo> processes = ProcessScanner.GetSnapshot();
                HardwareReport hardware = HardwareMonitor.Read();
                FillProcesses(processes);
                FillFiles(processes);
                FillSensors(hardware);
                FillMetrics(processes, hardware);
                statusLabel.Text = "Обновлено " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch
            {
                statusLabel.Text = "Ошибка";
                MessageBox.Show(this, "Не получилось обновить Ultimate-панель.", "Ultimate Security", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                refreshButton.Enabled = true;
            }
        }

        private void RunFullScan()
        {
            fullScanButton.Enabled = false;
            Cursor = Cursors.WaitCursor;
            statusLabel.Text = "Полная проверка...";
            scanList.Items.Clear();
            Application.DoEvents();

            try
            {
                List<ProcessInfo> processes = ProcessScanner.GetSnapshot();
                processes.Sort(delegate(ProcessInfo left, ProcessInfo right)
                {
                    int byScore = right.Score.CompareTo(left.Score);
                    if (byScore != 0) return byScore;
                    return right.DiskBytesSinceSeen.CompareTo(left.DiskBytesSinceSeen);
                });

                int found = 0;
                foreach (ProcessInfo info in processes)
                {
                    if (!IsFullScanFinding(info))
                    {
                        continue;
                    }

                    ListViewItem row = new ListViewItem(RiskLabel(info.Score));
                    row.ImageKey = AddIcon(info.Path);
                    row.Tag = info;
                    row.SubItems.Add(info.Name);
                    row.SubItems.Add(info.ProcessId.ToString());
                    row.SubItems.Add(info.Score.ToString("0"));
                    row.SubItems.Add(info.CpuPercent.ToString("0.0") + "%");
                    row.SubItems.Add(info.MemoryMb.ToString("0.0") + " МБ");
                    row.SubItems.Add(FormatSize(info.DiskBytesSinceSeen));
                    row.SubItems.Add(info.PathText);
                    row.SubItems.Add(info.Reason + "; файлы: " + info.FileActivityReason);
                    ApplyRiskColor(row, info.Score);
                    scanList.Items.Add(row);
                    found++;
                }

                statusLabel.Text = "Полная проверка: найдено " + found;
            }
            catch
            {
                statusLabel.Text = "Ошибка проверки";
                MessageBox.Show(this, "Не получилось выполнить полную проверку.", "Ultimate Security", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                fullScanButton.Enabled = true;
            }
        }

        private static bool IsFullScanFinding(ProcessInfo info)
        {
            if (info.Score >= 18d) return true;
            if (info.CpuPercent >= 15d) return true;
            if (info.NetworkConnections >= 6) return true;
            if (info.DiskBytesPerSecond >= 5L * 1024L * 1024L) return true;
            if (info.DiskBytesSinceSeen >= 30L * 1024L * 1024L) return true;
            if (!String.IsNullOrEmpty(info.Path))
            {
                string lower = info.Path.ToLowerInvariant();
                if (lower.Contains("\\appdata\\local\\temp\\") || lower.Contains("\\downloads\\") || lower.Contains("\\recycle.bin\\") || lower.Contains("\\public\\"))
                {
                    return true;
                }
            }

            return false;
        }

        private void FillProcesses(List<ProcessInfo> processes)
        {
            processList.Items.Clear();
            processes.Sort(delegate(ProcessInfo left, ProcessInfo right)
            {
                int byScore = right.Score.CompareTo(left.Score);
                if (byScore != 0) return byScore;
                return String.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });

            foreach (ProcessInfo info in processes)
            {
                ListViewItem row = new ListViewItem(info.Name);
                row.ImageKey = AddIcon(info.Path);
                row.Tag = info;
                row.SubItems.Add(info.ProcessId.ToString());
                row.SubItems.Add(info.Score.ToString("0"));
                row.SubItems.Add(info.CpuPercent.ToString("0.0") + "%");
                row.SubItems.Add(info.MemoryMb.ToString("0.0") + " МБ");
                row.SubItems.Add(info.NetworkConnections.ToString());
                row.SubItems.Add(FormatSize(info.DiskBytesPerSecond) + "/с");
                row.SubItems.Add(FormatSize(info.FileSize));
                row.SubItems.Add(info.FirstSeenText);
                row.SubItems.Add(info.FileCreatedText);
                row.SubItems.Add(info.PathText);
                row.SubItems.Add(info.Reason);
                ApplyRiskColor(row, info.Score);
                processList.Items.Add(row);
            }

            if (processList.Items.Count > 0)
            {
                processList.Items[0].Selected = true;
                processList.Select();
            }
        }

        private void FillFiles(List<ProcessInfo> processes)
        {
            fileList.Items.Clear();
            List<ProcessInfo> active = new List<ProcessInfo>();
            foreach (ProcessInfo info in processes)
            {
                if (info.DiskBytesSinceSeen > 0 || info.DiskBytesPerSecond > 0 || info.FileOperationsSinceSeen > 0)
                {
                    active.Add(info);
                }
            }

            active.Sort(delegate(ProcessInfo left, ProcessInfo right)
            {
                int byTotal = right.DiskBytesSinceSeen.CompareTo(left.DiskBytesSinceSeen);
                if (byTotal != 0) return byTotal;
                return right.DiskBytesPerSecond.CompareTo(left.DiskBytesPerSecond);
            });

            foreach (ProcessInfo info in active)
            {
                ListViewItem row = new ListViewItem(info.Name);
                row.ImageKey = AddIcon(info.Path);
                row.Tag = info;
                row.SubItems.Add(FormatSize(info.DiskBytesSinceSeen));
                row.SubItems.Add(FormatSize(info.DiskBytesPerSecond) + "/с");
                row.SubItems.Add(info.FileOperationsSinceSeen.ToString());
                row.SubItems.Add(info.ProcessId.ToString());
                row.SubItems.Add(info.PathText);
                row.SubItems.Add(info.FileActivityReason);
                ApplyRiskColor(row, Math.Max(info.Score, info.DiskBytesPerSecond / 1024d / 1024d));
                fileList.Items.Add(row);
            }
        }

        private static void ApplyRiskColor(ListViewItem row, double score)
        {
            if (score >= 45d)
            {
                row.BackColor = Color.FromArgb(92, 20, 28);
                row.ForeColor = Color.FromArgb(255, 225, 225);
            }
            else if (score >= 18d)
            {
                row.BackColor = Color.FromArgb(83, 62, 22);
                row.ForeColor = Color.FromArgb(255, 240, 190);
            }
            else
            {
                row.BackColor = Color.FromArgb(18, 64, 45);
                row.ForeColor = Color.FromArgb(210, 255, 230);
            }
        }

        private static string RiskLabel(double score)
        {
            if (score >= 45d) return "опасно";
            if (score >= 18d) return "подозрительно";
            return "безопасно";
        }

        private void FillSensors(HardwareReport report)
        {
            sensorList.Items.Clear();
            foreach (HardwareItem item in report.Items)
            {
                ListViewItem row = new ListViewItem(item.Group);
                row.SubItems.Add(item.Name);
                row.SubItems.Add(item.Value);
                row.SubItems.Add(item.Status);
                sensorList.Items.Add(row);
            }
        }

        private void FillMetrics(List<ProcessInfo> processes, HardwareReport hardware)
        {
            processCountLabel.Text = processes.Count.ToString();
            cpuLabel.Text = hardware.CpuLoadText;
            ramLabel.Text = hardware.RamLoadText;
            tempLabel.Text = hardware.TemperatureText;
        }

        private string AddIcon(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return "default";
            }

            string key = path.ToLowerInvariant();
            if (imageList.Images.ContainsKey(key))
            {
                return key;
            }

            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(path);
                if (icon != null)
                {
                    imageList.Images.Add(key, icon.ToBitmap());
                    return key;
                }
            }
            catch
            {
            }

            return "default";
        }

        private void ProcessListSelectedIndexChanged(object sender, EventArgs e)
        {
            ProcessInfo info = SelectedProcess();
            bool usableFile = info != null && !String.IsNullOrEmpty(info.Path) && File.Exists(info.Path);
            openFolderButton.Enabled = usableFile;
            killButton.Enabled = info != null && info.ProcessId != Process.GetCurrentProcess().Id;
            deleteButton.Enabled = usableFile && IsDeleteAllowed(info.Path);

            if (info == null)
            {
                return;
            }

            detailsBox.Text =
                "Название: " + info.Name + "    PID: " + info.ProcessId + Environment.NewLine +
                "Видим с открытия: " + info.FirstSeenText + "    Запущен с: " + info.StartTimeText + Environment.NewLine +
                "Файл в ПК с: " + info.FileCreatedText + "    Размер: " + FormatSize(info.FileSize) + Environment.NewLine +
                "Файловая активность с запуска приложения: " + FormatSize(info.DiskBytesSinceSeen) +
                "    Сейчас: " + FormatSize(info.DiskBytesPerSecond) + "/с" + Environment.NewLine +
                "Путь: " + info.PathText + Environment.NewLine +
                "Причина: " + info.Reason;
        }

        private void ProcessListMouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = processList.HitTest(e.Location);
            if (hit.Item != null && hit.SubItem != null && hit.Item.SubItems.IndexOf(hit.SubItem) == ProcessPathColumn)
            {
                OpenFolderFor(hit.Item.Tag as ProcessInfo);
            }
        }

        private void FileListMouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = fileList.HitTest(e.Location);
            if (hit.Item != null && hit.SubItem != null && hit.Item.SubItems.IndexOf(hit.SubItem) == FilePathColumn)
            {
                OpenFolderFor(hit.Item.Tag as ProcessInfo);
            }
        }

        private void ScanListMouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = scanList.HitTest(e.Location);
            if (hit.Item != null && hit.SubItem != null && hit.Item.SubItems.IndexOf(hit.SubItem) == 7)
            {
                OpenFolderFor(hit.Item.Tag as ProcessInfo);
            }
        }

        private ProcessInfo SelectedProcess()
        {
            if (processList.SelectedItems.Count == 0)
            {
                return null;
            }

            return processList.SelectedItems[0].Tag as ProcessInfo;
        }

        private void OpenSelectedFolder()
        {
            OpenFolderFor(SelectedProcess());
        }

        private static void OpenFolderFor(ProcessInfo info)
        {
            if (info == null || String.IsNullOrEmpty(info.Path) || !File.Exists(info.Path))
            {
                return;
            }

            Process.Start("explorer.exe", "/select,\"" + info.Path + "\"");
        }

        private void KillSelectedProcess()
        {
            ProcessInfo info = SelectedProcess();
            if (info == null)
            {
                return;
            }

            if (info.ProcessId == Process.GetCurrentProcess().Id)
            {
                MessageBox.Show(this, "Нельзя завершить само приложение.", "Защита", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(this, "Завершить процесс " + info.Name + "?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Process process = Process.GetProcessById(info.ProcessId);
                process.Kill();
                process.WaitForExit(2500);
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не получилось завершить процесс: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedApp()
        {
            ProcessInfo info = SelectedProcess();
            if (info == null || String.IsNullOrEmpty(info.Path) || !File.Exists(info.Path))
            {
                return;
            }

            if (!IsDeleteAllowed(info.Path))
            {
                MessageBox.Show(this, "Этот файл защищен от удаления приложением.", "Защита", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult first = MessageBox.Show(
                this,
                "Удалить файл приложения в корзину?\n\n" + info.Path + "\n\nЕсли приложение запущено, оно будет сначала завершено.",
                "Удаление",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (first != DialogResult.Yes)
            {
                return;
            }

            DialogResult second = MessageBox.Show(this, "Точно удалить выбранный файл в корзину?", "Последнее подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (second != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (info.ProcessId != Process.GetCurrentProcess().Id)
                {
                    try
                    {
                        Process process = Process.GetProcessById(info.ProcessId);
                        process.Kill();
                        process.WaitForExit(3000);
                    }
                    catch
                    {
                    }
                }

                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                    info.Path,
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                MessageBox.Show(this, "Файл отправлен в корзину.", "Удаление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не получилось удалить файл: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsDeleteAllowed(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            string full = Path.GetFullPath(path).TrimEnd('\\');
            string own = Path.GetFullPath(Application.ExecutablePath).TrimEnd('\\');
            if (String.Equals(full, own, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (!String.IsNullOrEmpty(windows) && full.StartsWith(Path.GetFullPath(windows), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string system = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (!String.IsNullOrEmpty(system) && full.StartsWith(Path.GetFullPath(system), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 0)
            {
                return "нет данных";
            }

            if (bytes >= 1024L * 1024L * 1024L)
            {
                return (bytes / 1024d / 1024d / 1024d).ToString("0.00") + " ГБ";
            }

            if (bytes >= 1024L * 1024L)
            {
                return (bytes / 1024d / 1024d).ToString("0.0") + " МБ";
            }

            if (bytes >= 1024L)
            {
                return (bytes / 1024d).ToString("0.0") + " КБ";
            }

            return bytes + " Б";
        }
    }

    internal static class ProcessScanner
    {
        private static readonly Dictionary<int, DateTime> FirstSeen = new Dictionary<int, DateTime>();
        private static readonly Dictionary<int, IoCounters> FirstIo = new Dictionary<int, IoCounters>();

        public static List<ProcessInfo> GetSnapshot()
        {
            Dictionary<int, TimeSpan> cpuStart = new Dictionary<int, TimeSpan>();
            Dictionary<int, IoCounters> ioStart = new Dictionary<int, IoCounters>();

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    cpuStart[process.Id] = process.TotalProcessorTime;
                    IoCounters counters;
                    if (NativeMethods.GetProcessIoCounters(process.Handle, out counters))
                    {
                        ioStart[process.Id] = counters;
                    }
                }
                catch
                {
                }
            }

            const double seconds = 1.0d;
            Thread.Sleep((int)(seconds * 1000d));

            Dictionary<int, int> network = NativeMethods.GetTcpConnectionCounts();
            Dictionary<int, string> wmiPaths = GetProcessPathsFromWmi();
            HashSet<int> live = new HashSet<int>();
            List<ProcessInfo> items = new List<ProcessInfo>();
            DateTime now = DateTime.Now;
            int processorCount = Math.Max(1, Environment.ProcessorCount);

            foreach (Process process in Process.GetProcesses())
            {
                live.Add(process.Id);
                ProcessInfo info = new ProcessInfo();
                info.Name = process.ProcessName;
                info.ProcessId = process.Id;
                info.Path = GetProcessPath(process, wmiPaths);
                info.PathText = String.IsNullOrEmpty(info.Path) ? "нет данных" : info.Path;
                info.MemoryMb = SafeWorkingSet(process) / 1024d / 1024d;
                info.NetworkConnections = network.ContainsKey(process.Id) ? network[process.Id] : 0;
                info.StartTimeText = GetStartTimeText(process);

                if (!FirstSeen.ContainsKey(process.Id))
                {
                    FirstSeen[process.Id] = now;
                }

                info.FirstSeen = FirstSeen[process.Id];
                info.FirstSeenText = info.FirstSeen.ToString("dd.MM.yyyy HH:mm:ss");

                TimeSpan endCpu = TimeSpan.Zero;
                try { endCpu = process.TotalProcessorTime; } catch {}
                if (cpuStart.ContainsKey(process.Id))
                {
                    double cpu = (endCpu - cpuStart[process.Id]).TotalMilliseconds / (seconds * 1000d) / processorCount * 100d;
                    info.CpuPercent = Math.Max(0d, cpu);
                }

                FillIoInfo(process, info, ioStart, seconds);
                FillFileInfo(info);
                Score(info, now);
                items.Add(info);
            }

            Cleanup(live);
            return items;
        }

        private static void FillIoInfo(Process process, ProcessInfo info, Dictionary<int, IoCounters> ioStart, double seconds)
        {
            info.FileActivityReason = "нет заметной активности";
            IoCounters endCounters;
            try
            {
                if (!NativeMethods.GetProcessIoCounters(process.Handle, out endCounters))
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            if (!FirstIo.ContainsKey(process.Id))
            {
                FirstIo[process.Id] = endCounters;
            }

            if (ioStart.ContainsKey(process.Id))
            {
                ulong readNow = SafeSubtract(endCounters.ReadTransferCount, ioStart[process.Id].ReadTransferCount);
                ulong writeNow = SafeSubtract(endCounters.WriteTransferCount, ioStart[process.Id].WriteTransferCount);
                info.DiskBytesPerSecond = ToLong((ulong)((readNow + writeNow) / seconds));
            }

            IoCounters first = FirstIo[process.Id];
            ulong read = SafeSubtract(endCounters.ReadTransferCount, first.ReadTransferCount);
            ulong write = SafeSubtract(endCounters.WriteTransferCount, first.WriteTransferCount);
            ulong ops = SafeSubtract(endCounters.ReadOperationCount, first.ReadOperationCount) + SafeSubtract(endCounters.WriteOperationCount, first.WriteOperationCount);
            info.DiskBytesSinceSeen = ToLong(read + write);
            info.FileOperationsSinceSeen = ToLong(ops);

            if (write > read && write > 0)
            {
                info.FileActivityReason = "больше пишет/изменяет файлы";
            }
            else if (read > 0 || write > 0)
            {
                info.FileActivityReason = "читает или записывает файлы";
            }
        }

        private static void Cleanup(HashSet<int> live)
        {
            List<int> remove = new List<int>();
            foreach (int id in FirstSeen.Keys)
            {
                if (!live.Contains(id)) remove.Add(id);
            }

            foreach (int id in remove)
            {
                FirstSeen.Remove(id);
                FirstIo.Remove(id);
            }
        }

        private static Dictionary<int, string> GetProcessPathsFromWmi()
        {
            Dictionary<int, string> paths = new Dictionary<int, string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessId, ExecutablePath FROM Win32_Process"))
                {
                    foreach (ManagementObject row in searcher.Get())
                    {
                        object id = row["ProcessId"];
                        object path = row["ExecutablePath"];
                        if (id != null && path != null)
                        {
                            paths[Convert.ToInt32(id)] = Convert.ToString(path);
                        }
                    }
                }
            }
            catch
            {
            }

            return paths;
        }

        private static string GetProcessPath(Process process, Dictionary<int, string> wmiPaths)
        {
            try
            {
                if (process.MainModule != null && !String.IsNullOrEmpty(process.MainModule.FileName))
                {
                    return process.MainModule.FileName;
                }
            }
            catch
            {
            }

            return wmiPaths.ContainsKey(process.Id) ? wmiPaths[process.Id] : null;
        }

        private static long SafeWorkingSet(Process process)
        {
            try { return process.WorkingSet64; } catch { return 0; }
        }

        private static string GetStartTimeText(Process process)
        {
            try { return process.StartTime.ToString("dd.MM.yyyy HH:mm"); } catch { return "нет данных"; }
        }

        private static void FillFileInfo(ProcessInfo info)
        {
            info.FileSize = -1;
            info.FileCreated = DateTime.MinValue;
            info.FileCreatedText = "нет данных";

            if (String.IsNullOrEmpty(info.Path) || !File.Exists(info.Path))
            {
                return;
            }

            try
            {
                FileInfo file = new FileInfo(info.Path);
                info.FileSize = file.Length;
                info.FileCreated = file.CreationTime;
                info.FileCreatedText = file.CreationTime.ToString("dd.MM.yyyy HH:mm");
            }
            catch
            {
            }
        }

        private static void Score(ProcessInfo info, DateTime now)
        {
            double score = 0d;
            List<string> reasons = new List<string>();

            if (info.CpuPercent >= 25d) { score += Math.Min(55d, info.CpuPercent * 2.2d); reasons.Add("сильно грузит CPU"); }
            else if (info.CpuPercent >= 8d) { score += Math.Min(25d, info.CpuPercent * 1.4d); reasons.Add("заметная нагрузка CPU"); }

            if (info.MemoryMb >= 1200d) { score += 28d; reasons.Add("очень много RAM"); }
            else if (info.MemoryMb >= 600d) { score += 14d; reasons.Add("много RAM"); }

            if (info.NetworkConnections >= 12) { score += 30d; reasons.Add("много сетевых соединений"); }
            else if (info.NetworkConnections >= 4) { score += 12d; reasons.Add("активная сеть"); }

            if (info.DiskBytesPerSecond >= 50L * 1024L * 1024L) { score += 26d; reasons.Add("сильно работает с файлами"); }
            else if (info.DiskBytesPerSecond >= 10L * 1024L * 1024L) { score += 12d; reasons.Add("активно читает/пишет файлы"); }

            if (!String.IsNullOrEmpty(info.Path))
            {
                string lower = info.Path.ToLowerInvariant();
                if (lower.Contains("\\appdata\\local\\temp\\") || lower.Contains("\\downloads\\") || lower.Contains("\\recycle.bin\\") || lower.Contains("\\public\\"))
                {
                    score += 24d;
                    reasons.Add("запущено из необычной папки");
                }

                string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                bool windowsPath = !String.IsNullOrEmpty(windows) && lower.StartsWith(windows.ToLowerInvariant());
                if (!windowsPath && !HasCertificate(info.Path))
                {
                    score += 12d;
                    reasons.Add("нет цифровой подписи");
                }
            }

            if (info.FileCreated != DateTime.MinValue && info.FileCreated > now.AddDays(-7d))
            {
                score += 9d;
                reasons.Add("файл появился недавно");
            }

            if (score <= 0d)
            {
                score = Math.Min(10d, info.CpuPercent * 0.6d + info.MemoryMb / 300d + info.NetworkConnections * 1.5d + info.DiskBytesPerSecond / 1024d / 1024d);
            }

            info.Score = score;
            info.Reason = reasons.Count == 0 ? "обычная нагрузка" : String.Join(", ", reasons.ToArray());
        }

        private static bool HasCertificate(string path)
        {
            try { return X509Certificate.CreateFromSignedFile(path) != null; } catch { return false; }
        }

        private static ulong SafeSubtract(ulong value, ulong baseline)
        {
            return value >= baseline ? value - baseline : 0;
        }

        private static long ToLong(ulong value)
        {
            return value > Int64.MaxValue ? Int64.MaxValue : (long)value;
        }
    }

    internal sealed class ProcessInfo
    {
        public string Name;
        public int ProcessId;
        public string Path;
        public string PathText;
        public double CpuPercent;
        public double MemoryMb;
        public int NetworkConnections;
        public long DiskBytesPerSecond;
        public long DiskBytesSinceSeen;
        public long FileOperationsSinceSeen;
        public string FileActivityReason;
        public long FileSize;
        public DateTime FileCreated;
        public string FileCreatedText;
        public DateTime FirstSeen;
        public string FirstSeenText;
        public string StartTimeText;
        public double Score;
        public string Reason;
    }

    internal static class HardwareMonitor
    {
        public static HardwareReport Read()
        {
            HardwareReport report = new HardwareReport();
            report.Items = new List<HardwareItem>();
            report.CpuLoadText = "нет данных";
            report.RamLoadText = "нет данных";
            report.TemperatureText = "нет данных";

            AddSystem(report);
            AddCpu(report);
            AddMemory(report);
            AddThermalZones(report);
            AddTemperatureProbes(report);
            AddFans(report);
            AddBattery(report);
            AddDisks(report);
            AddGpu(report);
            return report;
        }

        private static void AddSystem(HardwareReport report)
        {
            Add(report, "Система", "Приложение запущено", Program.StartedAt.ToString("dd.MM.yyyy HH:mm:ss"), "с этого момента считается мониторинг");
            Add(report, "Система", "Windows", Environment.OSVersion.ToString(), "ОС");
            Add(report, "Система", "Имя ПК", Environment.MachineName, "устройство");
            Add(report, "Система", "Права", IsAdmin() ? "администратор" : "обычный запуск", IsAdmin() ? "расширенный доступ включен" : "лучше запускать от администратора");
        }

        private static void AddCpu(HardwareReport report)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, LoadPercentage, CurrentClockSpeed, MaxClockSpeed FROM Win32_Processor"))
                {
                    foreach (ManagementObject cpu in searcher.Get())
                    {
                        string name = Convert.ToString(cpu["Name"]);
                        string cores = Convert.ToString(cpu["NumberOfCores"]);
                        string threads = Convert.ToString(cpu["NumberOfLogicalProcessors"]);
                        string load = Convert.ToString(cpu["LoadPercentage"]);
                        string current = Convert.ToString(cpu["CurrentClockSpeed"]);
                        string max = Convert.ToString(cpu["MaxClockSpeed"]);
                        report.CpuLoadText = String.IsNullOrEmpty(load) ? "нет данных" : load + "%";
                        Add(report, "CPU", "Процессор", name, "ядер: " + cores + ", потоков: " + threads);
                        Add(report, "CPU", "Нагрузка", report.CpuLoadText, LoadStatus(load, 75, 90));
                        Add(report, "CPU", "Частота", current + " / " + max + " МГц", "текущая / максимальная");
                    }
                }
            }
            catch
            {
                Add(report, "CPU", "Процессор", Environment.ProcessorCount + " потоков", "WMI не отдал подробности");
            }
        }

        private static void AddMemory(HardwareReport report)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        long totalKb = Convert.ToInt64(os["TotalVisibleMemorySize"]);
                        long freeKb = Convert.ToInt64(os["FreePhysicalMemory"]);
                        double used = totalKb > 0 ? (totalKb - freeKb) * 100d / totalKb : 0d;
                        report.RamLoadText = used.ToString("0.0") + "%";
                        Add(report, "RAM", "Всего", FormatSize(totalKb * 1024L), "свободно: " + FormatSize(freeKb * 1024L));
                        Add(report, "RAM", "Нагрузка", report.RamLoadText, used >= 90d ? "очень высокая" : used >= 75d ? "высокая" : "нормально");
                    }
                }
            }
            catch
            {
                Add(report, "RAM", "Память", "нет данных", "WMI не отдал данные");
            }
        }

        private static void AddThermalZones(HardwareReport report)
        {
            bool found = false;
            try
            {
                ManagementScope scope = new ManagementScope(@"root\WMI");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT InstanceName, CurrentTemperature FROM MSAcpi_ThermalZoneTemperature")))
                {
                    foreach (ManagementObject sensor in searcher.Get())
                    {
                        double celsius = Convert.ToDouble(sensor["CurrentTemperature"]) / 10d - 273.15d;
                        string name = Convert.ToString(sensor["InstanceName"]);
                        report.TemperatureText = celsius.ToString("0.0") + " °C";
                        Add(report, "Температура", String.IsNullOrEmpty(name) ? "Thermal Zone" : name, report.TemperatureText, TemperatureStatus(celsius));
                        found = true;
                    }
                }
            }
            catch
            {
            }

            if (!found)
            {
                Add(report, "Температура", "ACPI thermal zone", "нет данных", "Windows не отдала этот датчик");
            }
        }

        private static void AddTemperatureProbes(HardwareReport report)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, CurrentReading, Status FROM Win32_TemperatureProbe"))
                {
                    bool any = false;
                    foreach (ManagementObject sensor in searcher.Get())
                    {
                        any = true;
                        Add(report, "Датчик", Convert.ToString(sensor["Name"]), Convert.ToString(sensor["CurrentReading"]), Convert.ToString(sensor["Status"]));
                    }
                    if (!any) Add(report, "Датчик", "Win32_TemperatureProbe", "нет данных", "датчиков нет или драйвер не отдал");
                }
            }
            catch
            {
                Add(report, "Датчик", "Win32_TemperatureProbe", "нет данных", "WMI недоступен");
            }
        }

        private static void AddFans(HardwareReport report)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, DesiredSpeed, Status FROM Win32_Fan"))
                {
                    bool any = false;
                    foreach (ManagementObject fan in searcher.Get())
                    {
                        any = true;
                        Add(report, "Вентилятор", Convert.ToString(fan["Name"]), Convert.ToString(fan["DesiredSpeed"]), Convert.ToString(fan["Status"]));
                    }
                    if (!any) Add(report, "Вентилятор", "Win32_Fan", "нет данных", "Windows не отдала датчики вентиляторов");
                }
            }
            catch
            {
                Add(report, "Вентилятор", "Win32_Fan", "нет данных", "WMI недоступен");
            }
        }

        private static void AddBattery(HardwareReport report)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, EstimatedChargeRemaining, BatteryStatus FROM Win32_Battery"))
                {
                    bool any = false;
                    foreach (ManagementObject battery in searcher.Get())
                    {
                        any = true;
                        Add(report, "Батарея", Convert.ToString(battery["Name"]), Convert.ToString(battery["EstimatedChargeRemaining"]) + "%", "статус: " + Convert.ToString(battery["BatteryStatus"]));
                    }
                    if (!any) Add(report, "Батарея", "Аккумулятор", "нет данных", "скорее всего стационарный ПК");
                }
            }
            catch
            {
            }
        }

        private static void AddDisks(HardwareReport report)
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (!drive.IsReady) continue;
                    double free = drive.TotalSize > 0 ? drive.AvailableFreeSpace * 100d / drive.TotalSize : 0d;
                    Add(report, "Диск", drive.Name, FormatSize(drive.TotalSize), "свободно: " + FormatSize(drive.AvailableFreeSpace) + " (" + free.ToString("0.0") + "%)");
                }
            }
            catch
            {
                Add(report, "Диск", "Диски", "нет данных", "ошибка чтения");
            }
        }

        private static void AddGpu(HardwareReport report)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController"))
                {
                    foreach (ManagementObject gpu in searcher.Get())
                    {
                        string ram = gpu["AdapterRAM"] == null ? "нет данных" : FormatSize(Convert.ToInt64(gpu["AdapterRAM"]));
                        Add(report, "GPU", Convert.ToString(gpu["Name"]), "память: " + ram, "драйвер: " + Convert.ToString(gpu["DriverVersion"]));
                    }
                }
            }
            catch
            {
            }
        }

        private static void Add(HardwareReport report, string group, string name, string value, string status)
        {
            report.Items.Add(new HardwareItem(group, name, value, status));
        }

        private static bool IsAdmin()
        {
            try
            {
                return new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent())
                    .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private static string LoadStatus(string value, int high, int critical)
        {
            int number;
            if (!Int32.TryParse(value, out number)) return "нет данных";
            if (number >= critical) return "очень высокая";
            if (number >= high) return "высокая";
            return "нормально";
        }

        private static string TemperatureStatus(double celsius)
        {
            if (celsius >= 90d) return "опасно горячо";
            if (celsius >= 80d) return "высокая";
            return "нормально";
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 0) return "нет данных";
            if (bytes >= 1024L * 1024L * 1024L) return (bytes / 1024d / 1024d / 1024d).ToString("0.00") + " ГБ";
            if (bytes >= 1024L * 1024L) return (bytes / 1024d / 1024d).ToString("0.0") + " МБ";
            if (bytes >= 1024L) return (bytes / 1024d).ToString("0.0") + " КБ";
            return bytes + " Б";
        }
    }

    internal sealed class HardwareReport
    {
        public List<HardwareItem> Items;
        public string CpuLoadText;
        public string RamLoadText;
        public string TemperatureText;
    }

    internal sealed class HardwareItem
    {
        public readonly string Group;
        public readonly string Name;
        public readonly string Value;
        public readonly string Status;

        public HardwareItem(string group, string name, string value, string status)
        {
            Group = group;
            Name = name;
            Value = value;
            Status = status;
        }
    }

    internal static class NativeMethods
    {
        private const int AF_INET = 2;
        private const int TCP_TABLE_OWNER_PID_ALL = 5;
        private const int NO_ERROR = 0;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetProcessIoCounters(IntPtr processHandle, out IoCounters ioCounters);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr tcpTable, ref int tcpTableLength, bool sort, int ipVersion, int tableClass, uint reserved);

        public static Dictionary<int, int> GetTcpConnectionCounts()
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            int bufferLength = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferLength, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
            IntPtr buffer = Marshal.AllocHGlobal(bufferLength);

            try
            {
                uint result = GetExtendedTcpTable(buffer, ref bufferLength, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
                if (result != NO_ERROR) return counts;

                int rowCount = Marshal.ReadInt32(buffer);
                IntPtr rowPtr = new IntPtr(buffer.ToInt64() + 4);
                int rowSize = Marshal.SizeOf(typeof(MibTcpRowOwnerPid));
                for (int i = 0; i < rowCount; i++)
                {
                    MibTcpRowOwnerPid row = (MibTcpRowOwnerPid)Marshal.PtrToStructure(rowPtr, typeof(MibTcpRowOwnerPid));
                    if (row.State == 5 && row.OwningPid > 0)
                    {
                        int id = unchecked((int)row.OwningPid);
                        if (!counts.ContainsKey(id)) counts[id] = 0;
                        counts[id]++;
                    }
                    rowPtr = new IntPtr(rowPtr.ToInt64() + rowSize);
                }
            }
            catch
            {
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return counts;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IoCounters
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpRowOwnerPid
    {
        public uint State;
        public uint LocalAddr;
        public uint LocalPort;
        public uint RemoteAddr;
        public uint RemotePort;
        public uint OwningPid;
    }

    internal sealed class GradientPanel : Panel
    {
        public Color StartColor = Color.Black;
        public Color EndColor = Color.FromArgb(100, 72, 25);

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, StartColor, EndColor, LinearGradientMode.ForwardDiagonal))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            base.OnPaint(e);
        }
    }

    internal static class Ui
    {
        public static Font Font(float size, FontStyle style)
        {
            return new Font("Segoe UI", size, style);
        }

        public static Button DarkButton(string text, int x, int y, int width)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new Point(x, y);
            button.Size = new Size(width, 36);
            button.BackColor = Color.FromArgb(30, 41, 59);
            button.ForeColor = Color.FromArgb(226, 232, 240);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
            return button;
        }

        public static Button GoldButton(string text, int x, int y, int width)
        {
            Button button = DarkButton(text, x, y, width);
            button.BackColor = Color.FromArgb(191, 139, 48);
            button.ForeColor = Color.FromArgb(18, 18, 18);
            button.FlatAppearance.BorderColor = Color.FromArgb(255, 224, 154);
            return button;
        }

        public static Button DangerButton(string text, int x, int y, int width)
        {
            Button button = DarkButton(text, x, y, width);
            button.BackColor = Color.FromArgb(142, 39, 39);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(248, 113, 113);
            return button;
        }
    }

    internal static class AppImages
    {
        public static Icon AppIcon()
        {
            try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { return SystemIcons.Shield; }
        }

        public static Bitmap Avatar(int size)
        {
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (SolidBrush outer = new SolidBrush(Color.FromArgb(7, 12, 20)))
                using (SolidBrush gold = new SolidBrush(Color.FromArgb(191, 139, 48)))
                using (SolidBrush blue = new SolidBrush(Color.FromArgb(31, 111, 235)))
                using (Pen white = new Pen(Color.White, Math.Max(2, size / 18)))
                {
                    g.FillEllipse(outer, 1, 1, size - 2, size - 2);
                    Point[] shield = new Point[]
                    {
                        new Point(size / 2, size / 7),
                        new Point(size * 4 / 5, size / 4),
                        new Point(size * 3 / 4, size * 2 / 3),
                        new Point(size / 2, size * 5 / 6),
                        new Point(size / 4, size * 2 / 3),
                        new Point(size / 5, size / 4)
                    };
                    g.FillPolygon(gold, shield);
                    g.DrawPolygon(white, shield);
                    g.FillEllipse(blue, size * 3 / 8, size * 5 / 16, size / 4, size / 4);
                    g.FillPie(blue, size / 3, size / 2, size / 3, size / 3, 180, 180);
                }
            }
            return bitmap;
        }
    }
}
