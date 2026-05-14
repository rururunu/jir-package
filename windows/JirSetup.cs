using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace JirSetup
{
    public class SetupForm : Form
    {
        private Label titleLabel;
        private Label subtitleLabel;
        private Label pathLabel;
        private TextBox installPathBox;
        private OptionCheckBox addJirPathCheck;
        private OptionCheckBox setJavaHomeCheck;
        private Button installButton;
        private Button cancelButton;
        private Button browseButton;
        private Panel installPathPanel;
        private RedLoadingBar progressBar;
        private Label statusLabel;

        public SetupForm(string[] args)
        {
            Text = "jir Installer";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 430);
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;

            titleLabel = new Label();
            titleLabel.Text = "jir";
            titleLabel.Font = new Font("Segoe UI", 34F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(206, 32, 41);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Location = new Point(0, 38);
            titleLabel.Size = new Size(520, 64);
            Controls.Add(titleLabel);

            subtitleLabel = new Label();
            subtitleLabel.Text = "Manage Java runtimes fast.";
            subtitleLabel.ForeColor = Color.FromArgb(130, 120, 120);
            subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            subtitleLabel.Location = new Point(0, 100);
            subtitleLabel.Size = new Size(520, 24);
            Controls.Add(subtitleLabel);

            pathLabel = new Label();
            pathLabel.Text = "Install location";
            pathLabel.ForeColor = Color.FromArgb(58, 48, 48);
            pathLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            pathLabel.Location = new Point(58, 154);
            pathLabel.Size = new Size(180, 24);
            Controls.Add(pathLabel);

            installPathPanel = new Panel();
            installPathPanel.BackColor = Color.White;
            installPathPanel.Location = new Point(62, 180);
            installPathPanel.Size = new Size(312, 32);
            installPathPanel.Paint += InstallPathPanel_Paint;
            Controls.Add(installPathPanel);

            installPathBox = new TextBox();
            installPathBox.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs\\jir"
            );
            installPathBox.BorderStyle = BorderStyle.None;
            installPathBox.BackColor = Color.White;
            installPathBox.ForeColor = Color.FromArgb(48, 42, 42);
            installPathBox.Location = new Point(9, 7);
            installPathBox.Size = new Size(294, 20);
            installPathPanel.Controls.Add(installPathBox);

            browseButton = new Button();
            browseButton.Text = "Browse...";
            browseButton.FlatStyle = FlatStyle.Flat;
            browseButton.FlatAppearance.BorderColor = Color.FromArgb(224, 205, 205);
            browseButton.BackColor = Color.FromArgb(255, 247, 247);
            browseButton.ForeColor = Color.FromArgb(120, 22, 28);
            browseButton.Location = new Point(384, 180);
            browseButton.Size = new Size(78, 32);
            browseButton.Click += BrowseButton_Click;
            Controls.Add(browseButton);

            addJirPathCheck = new OptionCheckBox();
            addJirPathCheck.Text = "Add jir to PATH";
            addJirPathCheck.Checked = true;
            addJirPathCheck.ForeColor = Color.FromArgb(48, 42, 42);
            addJirPathCheck.Location = new Point(62, 238);
            addJirPathCheck.Size = new Size(400, 26);
            Controls.Add(addJirPathCheck);

            setJavaHomeCheck = new OptionCheckBox();
            setJavaHomeCheck.Text = "Set JAVA_HOME";
            setJavaHomeCheck.Checked = false;
            setJavaHomeCheck.ForeColor = Color.FromArgb(48, 42, 42);
            setJavaHomeCheck.Location = new Point(62, 270);
            setJavaHomeCheck.Size = new Size(400, 26);
            Controls.Add(setJavaHomeCheck);

            progressBar = new RedLoadingBar();
            progressBar.Location = new Point(62, 320);
            progressBar.Size = new Size(400, 8);
            progressBar.Visible = false;
            Controls.Add(progressBar);

            statusLabel = new Label();
            statusLabel.Text = "";
            statusLabel.ForeColor = Color.FromArgb(88, 97, 115);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Location = new Point(62, 336);
            statusLabel.Size = new Size(400, 24);
            Controls.Add(statusLabel);

            installButton = new Button();
            installButton.Text = "Install";
            installButton.FlatStyle = FlatStyle.Flat;
            installButton.FlatAppearance.BorderSize = 0;
            installButton.BackColor = Color.FromArgb(206, 32, 41);
            installButton.ForeColor = Color.White;
            installButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            installButton.Location = new Point(352, 376);
            installButton.Size = new Size(110, 36);
            installButton.Click += InstallButton_Click;
            Controls.Add(installButton);

            cancelButton = new Button();
            cancelButton.Text = "Exit";
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(224, 205, 205);
            cancelButton.BackColor = Color.White;
            cancelButton.ForeColor = Color.FromArgb(84, 70, 70);
            cancelButton.Location = new Point(232, 376);
            cancelButton.Size = new Size(110, 36);
            cancelButton.Click += delegate { Close(); };
            Controls.Add(cancelButton);

            ApplyArgs(args);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void InstallPathPanel_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, installPathPanel.Width - 1, installPathPanel.Height - 1);
            using (Pen pen = new Pen(Color.FromArgb(224, 205, 205)))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Choose jir install location";
            dialog.SelectedPath = installPathBox.Text;
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                installPathBox.Text = dialog.SelectedPath;
            }
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            string installDir = installPathBox.Text.Trim();
            if (installDir.Length == 0)
            {
                MessageBox.Show(this, "Please choose an install location.", "jir Installer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool addJirToPath = addJirPathCheck.Checked;
            bool setJavaHome = setJavaHomeCheck.Checked;
            string occupyDir = Path.Combine(Path.Combine(installDir, "home"), "occupy");

            if (setJavaHome && RequiresAdminJavaFix(occupyDir) && !IsAdministrator())
            {
                DialogResult result = MessageBox.Show(
                    this,
                    "System Java settings were found.\r\n\r\nTo make `java` use jir reliably, the installer needs administrator permission to update system JAVA_HOME and remove old JDK entries from the system PATH.\r\n\r\nRestart installer as administrator?",
                    "Administrator permission required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                if (result == DialogResult.Yes)
                {
                    RestartAsAdmin(installDir, addJirToPath, setJavaHome);
                    Close();
                    return;
                }
            }

            ShowInstallingState("Installing jir...");

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate {
                InstallJir(installDir, addJirToPath, setJavaHome);
            };
            worker.RunWorkerCompleted += delegate(object completedSender, RunWorkerCompletedEventArgs args) {
                if (args.Error != null)
                {
                    ShowFinishedState("Installation failed.\r\n" + args.Error.Message, true);
                    return;
                }

                ShowFinishedState("Installation successful.\r\nThanks for using jir.", false);
            };
            worker.RunWorkerAsync();
        }

        private void ShowInstallingState(string status)
        {
            titleLabel.Visible = false;
            subtitleLabel.Visible = false;
            pathLabel.Visible = false;
            installPathPanel.Visible = false;
            browseButton.Visible = false;
            addJirPathCheck.Visible = false;
            setJavaHomeCheck.Visible = false;
            installButton.Visible = false;
            cancelButton.Visible = false;

            progressBar.Location = new Point(80, 198);
            progressBar.Size = new Size(360, 10);
            progressBar.Visible = true;
            progressBar.Start();

            statusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            statusLabel.ForeColor = Color.FromArgb(88, 80, 80);
            statusLabel.Location = new Point(60, 222);
            statusLabel.Size = new Size(400, 42);
            statusLabel.Text = status;

            Refresh();
        }

        private void ShowFinishedState(string message, bool isError)
        {
            progressBar.Stop();
            progressBar.Visible = false;

            statusLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            statusLabel.ForeColor = isError ? Color.FromArgb(206, 32, 41) : Color.FromArgb(206, 32, 41);
            statusLabel.Location = new Point(60, 152);
            statusLabel.Size = new Size(400, 96);
            statusLabel.Text = message;
            statusLabel.Visible = true;

            cancelButton.Text = "Close";
            cancelButton.Location = new Point(205, 310);
            cancelButton.Size = new Size(110, 36);
            cancelButton.Visible = true;
            cancelButton.Enabled = true;
            cancelButton.BringToFront();

            Refresh();
        }

        private void ApplyArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("--install-dir=", StringComparison.OrdinalIgnoreCase))
                {
                    installPathBox.Text = arg.Substring("--install-dir=".Length);
                }
                else if (arg.StartsWith("--add-path=", StringComparison.OrdinalIgnoreCase))
                {
                    addJirPathCheck.Checked = arg.Substring("--add-path=".Length) == "1";
                }
                else if (arg.StartsWith("--set-java-home=", StringComparison.OrdinalIgnoreCase))
                {
                    setJavaHomeCheck.Checked = arg.Substring("--set-java-home=".Length) == "1";
                }
            }
        }

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool RequiresAdminJavaFix(string occupyDir)
        {
            string machineJavaHome = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine);
            if (!String.IsNullOrEmpty(machineJavaHome) &&
                !String.Equals(machineJavaHome.TrimEnd('\\'), occupyDir.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            return HasConflictingJavaPath(machinePath, occupyDir);
        }

        private static bool HasConflictingJavaPath(string path, string occupyDir)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            string occupyBin = Path.Combine(occupyDir, "bin").TrimEnd('\\');
            string[] parts = path.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim().TrimEnd('\\');
                if (part.Length == 0)
                {
                    continue;
                }

                if (String.Equals(part, "%JAVA_HOME%\\bin", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(part, occupyBin, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (LooksLikeJavaBin(part))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool LooksLikeJavaBin(string path)
        {
            string lower = path.ToLowerInvariant();
            return lower.EndsWith("\\bin") &&
                (lower.Contains("\\jdk") || lower.Contains("\\java") || lower.Contains("openjdk"));
        }

        private static void RestartAsAdmin(string installDir, bool addJirToPath, bool setJavaHome)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Application.ExecutablePath;
            info.UseShellExecute = true;
            info.Verb = "runas";
            info.Arguments =
                "--install-dir=" + QuoteArg(installDir) + " " +
                "--add-path=" + (addJirToPath ? "1" : "0") + " " +
                "--set-java-home=" + (setJavaHome ? "1" : "0");
            Process.Start(info);
        }

        private static string QuoteArg(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static void InstallJir(string installDir, bool addJirToPath, bool setJavaHome)
        {
            string homeDir = Path.Combine(installDir, "home");
            string occupyDir = Path.Combine(homeDir, "occupy");
            string targetExe = Path.Combine(installDir, "jir.exe");
            string uninstallExe = Path.Combine(installDir, "uninstall.exe");

            Directory.CreateDirectory(installDir);
            Directory.CreateDirectory(homeDir);

            ExtractEmbeddedJir(targetExe);
            ExtractEmbeddedResource("uninstall.exe", uninstallExe);

            if (addJirToPath)
            {
                AddPathEntry(EnvironmentVariableTarget.User, installDir, false);
            }

            if (setJavaHome)
            {
                Environment.SetEnvironmentVariable("JAVA_HOME", occupyDir, EnvironmentVariableTarget.User);
                AddPathEntry(EnvironmentVariableTarget.User, "%JAVA_HOME%\\bin", true);

                if (IsAdministrator())
                {
                    Environment.SetEnvironmentVariable("JAVA_HOME", occupyDir, EnvironmentVariableTarget.Machine);
                    FixMachinePath(occupyDir);
                }
            }
        }

        private static void ExtractEmbeddedJir(string targetExe)
        {
            ExtractEmbeddedResource("jir.exe", targetExe);
        }

        private static void ExtractEmbeddedResource(string resourceName, string targetPath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream input = assembly.GetManifestResourceStream(resourceName);
            if (input == null)
            {
                throw new Exception("embedded " + resourceName + " resource was not found.");
            }

            using (input)
            using (FileStream output = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[1024 * 64];
                while (true)
                {
                    int read = input.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                    {
                        break;
                    }
                    output.Write(buffer, 0, read);
                }
            }
        }

        private static void FixMachinePath(string occupyDir)
        {
            string path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            List<string> cleaned = new List<string>();
            string occupyBin = Path.Combine(occupyDir, "bin").TrimEnd('\\');
            bool hasJavaHomeBin = false;

            if (!String.IsNullOrEmpty(path))
            {
                string[] rawParts = path.Split(';');
                for (int i = 0; i < rawParts.Length; i++)
                {
                    string part = rawParts[i].Trim();
                    if (part.Length == 0)
                    {
                        continue;
                    }

                    string normalized = part.TrimEnd('\\');
                    if (String.Equals(normalized, "%JAVA_HOME%\\bin", StringComparison.OrdinalIgnoreCase))
                    {
                        hasJavaHomeBin = true;
                        cleaned.Add("%JAVA_HOME%\\bin");
                        continue;
                    }

                    if (String.Equals(normalized, occupyBin, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (LooksLikeJavaBin(normalized))
                    {
                        continue;
                    }

                    cleaned.Add(part);
                }
            }

            if (!hasJavaHomeBin)
            {
                cleaned.Insert(0, "%JAVA_HOME%\\bin");
            }

            Environment.SetEnvironmentVariable("Path", String.Join(";", cleaned.ToArray()), EnvironmentVariableTarget.Machine);
        }

        private static void AddPathEntry(EnvironmentVariableTarget target, string entry, bool prepend)
        {
            string path = Environment.GetEnvironmentVariable("Path", target);
            if (path == null)
            {
                path = "";
            }

            string[] rawParts = path.Split(';');
            List<string> parts = new List<string>();
            bool exists = false;

            for (int i = 0; i < rawParts.Length; i++)
            {
                string part = rawParts[i].Trim();
                if (part.Length == 0)
                {
                    continue;
                }

                if (String.Equals(part, entry, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                }

                parts.Add(part);
            }

            if (!exists)
            {
                if (prepend)
                {
                    parts.Insert(0, entry);
                }
                else
                {
                    parts.Add(entry);
                }
                Environment.SetEnvironmentVariable("Path", String.Join(";", parts.ToArray()), target);
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm(args));
        }
    }

    public class BrandPanel : Panel
    {
        public BrandPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(215, 28, 42),
                Color.FromArgb(42, 15, 18),
                135F))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            using (SolidBrush circle = new SolidBrush(Color.FromArgb(30, Color.White)))
            {
                e.Graphics.FillEllipse(circle, -80, 300, 220, 220);
                e.Graphics.FillEllipse(circle, 160, -40, 170, 170);
            }

            using (Pen line = new Pen(Color.FromArgb(180, Color.White), 2F))
            {
                e.Graphics.DrawLine(line, 38, 46, 128, 46);
                e.Graphics.DrawLine(line, 38, 390, 214, 390);
            }

            using (Font issueFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (SolidBrush softWhite = new SolidBrush(Color.FromArgb(210, Color.White)))
            {
                e.Graphics.DrawString("JAVA RUNTIME MANAGER", issueFont, softWhite, 38, 58);
                e.Graphics.DrawString("WINDOWS EDITION", issueFont, softWhite, 38, 410);
            }

            using (Font logoFont = new Font("Segoe UI", 44F, FontStyle.Bold))
            using (SolidBrush white = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString("jir", logoFont, white, 34, 104);
            }

            using (Font headlineFont = new Font("Segoe UI", 17F, FontStyle.Bold))
            using (SolidBrush white = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString("INSTALL", headlineFont, white, 38, 184);
                e.Graphics.DrawString("SWITCH", headlineFont, white, 38, 216);
                e.Graphics.DrawString("MANAGE", headlineFont, white, 38, 248);
            }

            using (Font taglineFont = new Font("Segoe UI", 9.5F, FontStyle.Regular))
            using (SolidBrush softWhite = new SolidBrush(Color.FromArgb(225, Color.White)))
            {
                e.Graphics.DrawString("A sharper way to handle JDKs.", taglineFont, softWhite, 40, 292);
            }

            DrawFeature(e.Graphics, "No admin required", 42, 334);
            DrawFeature(e.Graphics, "Local home directory", 42, 362);
        }

        private void DrawFeature(Graphics graphics, string text, int x, int y)
        {
            using (SolidBrush dot = new SolidBrush(Color.White))
            using (SolidBrush label = new SolidBrush(Color.FromArgb(238, Color.White)))
            using (Font font = new Font("Segoe UI", 9.5F))
            {
                graphics.FillEllipse(dot, x, y + 6, 7, 7);
                graphics.DrawString(text, font, label, x + 16, y);
            }
        }
    }

    public class OptionCheckBox : CheckBox
    {
        private bool hovering;
        private readonly Color red = Color.FromArgb(206, 32, 41);
        private readonly Color border = Color.FromArgb(218, 198, 198);
        private readonly Color hover = Color.FromArgb(255, 247, 247);
        private readonly Color text = Color.FromArgb(48, 42, 42);

        public OptionCheckBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            Invalidate();
            base.OnCheckedChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Parent != null ? Parent.BackColor : Color.White);

            if (hovering && Enabled)
            {
                using (SolidBrush hoverBrush = new SolidBrush(hover))
                {
                    e.Graphics.FillRectangle(hoverBrush, new Rectangle(0, 0, Width - 1, Height - 1));
                }
            }

            Rectangle box = new Rectangle(1, (Height - 18) / 2, 18, 18);

            using (SolidBrush fill = new SolidBrush(Checked ? red : Color.White))
            using (Pen stroke = new Pen(Checked ? red : border, 1.4F))
            {
                e.Graphics.FillRectangle(fill, box);
                e.Graphics.DrawRectangle(stroke, box);
            }

            if (Checked)
            {
                using (Pen check = new Pen(Color.White, 2.2F))
                {
                    check.StartCap = LineCap.Round;
                    check.EndCap = LineCap.Round;
                    e.Graphics.DrawLines(check, new Point[] {
                        new Point(box.Left + 4, box.Top + 9),
                        new Point(box.Left + 8, box.Top + 13),
                        new Point(box.Left + 15, box.Top + 5)
                    });
                }
            }

            Color labelColor = Enabled ? text : Color.FromArgb(160, 150, 150);
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                new Rectangle(30, 0, Width - 30, Height),
                labelColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );
        }
    }

    public class RedLoadingBar : Control
    {
        private readonly Timer timer;
        private int offset;

        public RedLoadingBar()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += delegate {
                offset = (offset + 5) % Math.Max(Width, 1);
                Invalidate();
            };
        }

        public void Start()
        {
            offset = 0;
            timer.Start();
            Invalidate();
        }

        public void Stop()
        {
            timer.Stop();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle track = new Rectangle(0, 0, Width - 1, Height - 1);

            using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(255, 232, 234)))
            {
                FillRounded(e.Graphics, trackBrush, track, Height / 2);
            }

            int segmentWidth = Math.Max(Width / 3, 80);
            int x = offset - segmentWidth;
            while (x < Width)
            {
                Rectangle segment = new Rectangle(x, 0, segmentWidth, Height - 1);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    segment,
                    Color.FromArgb(255, 168, 174),
                    Color.FromArgb(206, 32, 41),
                    0F))
                {
                    FillRounded(e.Graphics, brush, segment, Height / 2);
                }
                x += Width;
            }
        }

        private static void FillRounded(Graphics graphics, Brush brush, Rectangle rect, int radius)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
            {
                graphics.FillPath(brush, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
