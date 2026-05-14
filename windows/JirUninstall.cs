using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace JirUninstall
{
    public class UninstallForm : Form
    {
        private Button uninstallButton;
        private Button cancelButton;
        private Label statusLabel;
        private string installDir;

        public UninstallForm()
        {
            installDir = Application.StartupPath;

            Text = "jir Uninstaller";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(500, 320);
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;

            Label title = new Label();
            title.Text = "Uninstall jir";
            title.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(206, 32, 41);
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.Location = new Point(0, 42);
            title.Size = new Size(500, 56);
            Controls.Add(title);

            Label description = new Label();
            description.Text = "This will remove jir and clean environment variables.";
            description.ForeColor = Color.FromArgb(130, 120, 120);
            description.TextAlign = ContentAlignment.MiddleCenter;
            description.Location = new Point(40, 108);
            description.Size = new Size(420, 28);
            Controls.Add(description);

            Label path = new Label();
            path.Text = installDir;
            path.ForeColor = Color.FromArgb(88, 80, 80);
            path.TextAlign = ContentAlignment.MiddleCenter;
            path.Location = new Point(45, 150);
            path.Size = new Size(410, 42);
            Controls.Add(path);

            statusLabel = new Label();
            statusLabel.Text = "";
            statusLabel.ForeColor = Color.FromArgb(206, 32, 41);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Location = new Point(50, 205);
            statusLabel.Size = new Size(400, 34);
            Controls.Add(statusLabel);

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(224, 205, 205);
            cancelButton.BackColor = Color.White;
            cancelButton.ForeColor = Color.FromArgb(84, 70, 70);
            cancelButton.Location = new Point(150, 260);
            cancelButton.Size = new Size(95, 34);
            cancelButton.Click += delegate { Close(); };
            Controls.Add(cancelButton);

            uninstallButton = new Button();
            uninstallButton.Text = "Uninstall";
            uninstallButton.FlatStyle = FlatStyle.Flat;
            uninstallButton.FlatAppearance.BorderSize = 0;
            uninstallButton.BackColor = Color.FromArgb(206, 32, 41);
            uninstallButton.ForeColor = Color.White;
            uninstallButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            uninstallButton.Location = new Point(255, 260);
            uninstallButton.Size = new Size(95, 34);
            uninstallButton.Click += UninstallButton_Click;
            Controls.Add(uninstallButton);
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

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                this,
                "Remove jir from this computer?",
                "Uninstall jir",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            string occupyDir = Path.Combine(Path.Combine(installDir, "home"), "occupy");
            if (NeedsAdminCleanup(installDir, occupyDir) && !IsAdministrator())
            {
                DialogResult elevate = MessageBox.Show(
                    this,
                    "System environment variables point to this jir installation.\r\n\r\nRestart uninstaller as administrator to clean them?",
                    "Administrator permission required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                if (elevate == DialogResult.Yes)
                {
                    RestartAsAdmin();
                    Close();
                    return;
                }
            }

            try
            {
                uninstallButton.Enabled = false;
                cancelButton.Enabled = false;
                statusLabel.Text = "Cleaning...";
                Refresh();

                CleanupEnvironment(installDir, occupyDir, EnvironmentVariableTarget.User);
                if (IsAdministrator())
                {
                    CleanupEnvironment(installDir, occupyDir, EnvironmentVariableTarget.Machine);
                }

                ScheduleDirectoryRemoval(installDir);
                statusLabel.Text = "Uninstalled successfully.";
                MessageBox.Show(this, "jir was removed successfully.", "jir Uninstaller", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Uninstall failed.";
                MessageBox.Show(this, ex.Message, "Uninstall failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                uninstallButton.Enabled = true;
                cancelButton.Enabled = true;
            }
        }

        private static bool NeedsAdminCleanup(string installDir, string occupyDir)
        {
            string machineJavaHome = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine);
            if (IsSamePath(machineJavaHome, occupyDir))
            {
                return true;
            }

            string machinePath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            return ContainsInstallPath(machinePath, installDir, occupyDir);
        }

        private static void CleanupEnvironment(string installDir, string occupyDir, EnvironmentVariableTarget target)
        {
            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME", target);
            if (IsSamePath(javaHome, occupyDir))
            {
                Environment.SetEnvironmentVariable("JAVA_HOME", null, target);
            }

            string path = Environment.GetEnvironmentVariable("Path", target);
            if (path == null)
            {
                return;
            }

            List<string> kept = new List<string>();
            string[] parts = path.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (part.Length == 0)
                {
                    continue;
                }

                if (ShouldRemovePath(part, installDir, occupyDir))
                {
                    continue;
                }
                kept.Add(part);
            }

            Environment.SetEnvironmentVariable("Path", String.Join(";", kept.ToArray()), target);
        }

        private static bool ContainsInstallPath(string path, string installDir, string occupyDir)
        {
            if (path == null)
            {
                return false;
            }
            string[] parts = path.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (ShouldRemovePath(parts[i].Trim(), installDir, occupyDir))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ShouldRemovePath(string entry, string installDir, string occupyDir)
        {
            string normalized = entry.TrimEnd('\\');
            string install = installDir.TrimEnd('\\');
            string occupyBin = Path.Combine(occupyDir, "bin").TrimEnd('\\');

            return
                String.Equals(normalized, install, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(normalized, occupyBin, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(normalized, "%JAVA_HOME%\\bin", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSamePath(string left, string right)
        {
            if (String.IsNullOrEmpty(left) || String.IsNullOrEmpty(right))
            {
                return false;
            }
            return String.Equals(left.TrimEnd('\\'), right.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdmin()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Application.ExecutablePath;
            info.UseShellExecute = true;
            info.Verb = "runas";
            Process.Start(info);
        }

        private static void ScheduleDirectoryRemoval(string dir)
        {
            string args = "/C ping 127.0.0.1 -n 2 > nul & rmdir /S /Q \"" + dir + "\"";
            ProcessStartInfo info = new ProcessStartInfo("cmd.exe", args);
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(info);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UninstallForm());
        }
    }
}
