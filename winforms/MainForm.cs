using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Caesar.Gui;

public sealed class MainForm : Form
{
    private readonly TextBox _inputBox;
    private readonly TextBox _outputBox;
    private readonly NumericUpDown _shiftSpinner;
    private readonly RadioButton _decryptRadio;
    private readonly Button _saveButton;
    private readonly Button _saveInputButton;
    private readonly Button _swapButton;

    private string? _inputText;

    public MainForm()
    {
        SuspendLayout();

        Text = "Caesar Cipher";
        Size = new Size(980, 680);
        MinimumSize = new Size(720, 520);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);

        // ── App icon ──────────────────────────────────────────────────────────
        using var iconStream = typeof(MainForm).Assembly
            .GetManifestResourceStream("Caesar.Gui.application_icon.png");
        var iconBitmap = iconStream is not null ? new Bitmap(iconStream) : null;
        if (iconBitmap is not null)
            Icon = Icon.FromHandle(new Bitmap(iconBitmap, 32, 32).GetHicon());

        var iconBox = new PictureBox
        {
            Image = iconBitmap,
            SizeMode = PictureBoxSizeMode.Zoom,
            Anchor = AnchorStyles.None,
            Size = new Size(46, 46),
            BackColor = Color.Transparent,
        };

        // ── App title & subtitle ───────────────────────────────────────────────
        var appTitleLabel = new Label
        {
            Text = "Caesar Cipher",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 40, 90),
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.BottomLeft,
        };

        var appSubLabel = new Label
        {
            Text = "29-character alphabet · Æ Ø Å",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(90, 115, 165),
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 18,
            TextAlign = ContentAlignment.TopLeft,
        };

        // appSubLabel index 0 (front) → docks second (below title)
        // appTitleLabel index 1 (back) → docks first (top strip)
        var titlePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(2, 14, 8, 0),
        };
        titlePanel.Controls.Add(appSubLabel);
        titlePanel.Controls.Add(appTitleLabel);

        var browseButton = new Button
        {
            Text = "Browse…",
        };
        browseButton.Click += OnBrowse;

        // ── Header panel ──────────────────────────────────────────────────────
        // 2 columns: [icon 62px] [title fill]
        var headerTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            AllowDrop = true,
        };
        headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62f));
        headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        headerTable.Controls.Add(iconBox, 0, 0);
        headerTable.Controls.Add(titlePanel, 1, 0);
        headerTable.DragEnter += OnDragEnter;
        headerTable.DragDrop += OnDragDrop;

        var headerSep = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = Color.FromArgb(185, 210, 245),
        };

        // headerTable (Fill) added first; headerSep (Bottom) added second → sep docks first
        var headerOuter = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 246, 255),
            AllowDrop = true,
        };
        headerOuter.Controls.Add(headerTable);
        headerOuter.Controls.Add(headerSep);
        headerOuter.DragEnter += OnDragEnter;
        headerOuter.DragDrop += OnDragDrop;

        // ── Controls bar (between drop zone and text panels) ──────────────────
        var shiftLabel = new Label
        {
            Text = "Shift:",
            AutoSize = true,
            Margin = new Padding(0, 6, 4, 0),
        };

        _shiftSpinner = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 28,
            Value = 1,
            Width = 64,
            TextAlign = HorizontalAlignment.Center,
            Margin = new Padding(0, 4, 14, 0),
        };
        _shiftSpinner.ValueChanged += (_, _) => Recompute();

        var encryptRadio = new RadioButton
        {
            Text = "Encrypt",
            Checked = true,
            AutoSize = true,
            Margin = new Padding(0, 6, 6, 0),
        };
        encryptRadio.CheckedChanged += (_, _) => Recompute();

        _decryptRadio = new RadioButton
        {
            Text = "Decrypt",
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0),
        };

        _saveButton = new Button
        {
            Text = "Save Output…",
            AutoSize = true,
            Enabled = false,
            Margin = new Padding(22, 4, 0, 0),
        };
        _saveButton.Click += OnSave;

        _saveInputButton = new Button
        {
            Text = "Save…",
            AutoSize = true,
            Enabled = false,
        };
        _saveInputButton.Click += OnSaveInput;

        var controlsBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 6, 8, 0),
        };
        controlsBar.Controls.AddRange(new Control[]
        {
            shiftLabel, _shiftSpinner, encryptRadio, _decryptRadio,
        });

        // ── Text panels ───────────────────────────────────────────────────────
        _inputBox = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10f),
            BackColor = Color.White,
            WordWrap = false,
            AllowDrop = true,
            PlaceholderText = "Type or drop a .txt file here…",
        };
        _inputBox.DragEnter += OnDragEnter;
        _inputBox.DragDrop += OnDragDrop;
        _inputBox.TextChanged += (_, _) => { _inputText = _inputBox.Text; Recompute(); };

        _outputBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10f),
            BackColor = Color.FromArgb(244, 254, 244),
            WordWrap = false,
            AllowDrop = true,
            PlaceholderText = "Output will appear here…",
        };
        _outputBox.DragEnter += OnDragEnter;
        _outputBox.DragDrop += OnDragDrop;

        _swapButton = new Button
        {
            Text = "⇄",
            Font = new Font("Segoe UI", 14f),
            Anchor = AnchorStyles.None,
            Size = new Size(38, 38),
            Enabled = false,
        };
        _swapButton.Click += OnSwap;

        var inputPanel = BuildLabeledPanel("Input", _inputBox, browseButton, _saveInputButton);
        var outputPanel = BuildLabeledPanel("Output", _outputBox, _saveButton);

        var splitLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(4, 4, 4, 4),
            AllowDrop = true,
        };
        splitLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        splitLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48f));
        splitLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        splitLayout.Controls.Add(inputPanel, 0, 0);
        splitLayout.Controls.Add(_swapButton, 1, 0);
        splitLayout.Controls.Add(outputPanel, 2, 0);
        splitLayout.DragEnter += OnDragEnter;
        splitLayout.DragDrop += OnDragDrop;

        // ── Root layout ───────────────────────────────────────────────────────
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));    // app header
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));    // controls bar
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // text panels
        root.Controls.Add(headerOuter, 0, 0);
        root.Controls.Add(controlsBar, 0, 1);
        root.Controls.Add(splitLayout, 0, 2);

        Controls.Add(root);

        AllowDrop = true;
        DragEnter += OnDragEnter;
        DragDrop += OnDragDrop;

        ResumeLayout(true);
    }

    // ── Layout helpers ────────────────────────────────────────────────────────

    private static Panel BuildLabeledPanel(string title, Control inner, params Button[] actions)
    {
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0),
        };

        // titleLabel (Fill) added first (index 0).
        // Each action button added after: higher index → processed first → rightmost.
        // Iterating forward gives left-to-right visual order: first arg = leftmost button.
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 28,
            BackColor = Color.FromArgb(235, 235, 235),
            Padding = new Padding(0, 2, 3, 2),
        };
        header.Controls.Add(titleLabel);
        foreach (var action in actions)
        {
            action.AutoSize = false;
            action.Width = 68;
            action.Dock = DockStyle.Right;
            header.Controls.Add(action);
        }

        // inner (Fill) added first; header (Top) added second → header docked first
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(2) };
        panel.Controls.Add(inner);
        panel.Controls.Add(header);
        return panel;
    }

    // ── Drag & drop ───────────────────────────────────────────────────────────

    private static void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] { Length: > 0 } files)
            return;

        LoadFile(files[0]);
    }

    // ── Browse ────────────────────────────────────────────────────────────────

    private void OnBrowse(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Open text file",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "txt",
        };

        if (dlg.ShowDialog() == DialogResult.OK)
            LoadFile(dlg.FileName);
    }

    // ── File loading ──────────────────────────────────────────────────────────

    private void LoadFile(string path)
    {
        if (!Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Only .txt files are supported.", "Unsupported file",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _inputText = File.ReadAllText(path, Encoding.UTF8);
            Text = $"Caesar Cipher — {Path.GetFileName(path)}";
            _inputBox.Text = _inputText;
            Recompute();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not read file:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ── Swap ──────────────────────────────────────────────────────────────────

    private void OnSwap(object? sender, EventArgs e)
    {
        if (_inputText is null) return;

        _inputText = _outputBox.Text;
        _inputBox.Text = _inputText;
        Text = "Caesar Cipher Tool";
        Recompute();
    }

    // ── Cipher ────────────────────────────────────────────────────────────────

    private void Recompute()
    {
        if (_inputText is null) return;

        int shift = (int)_shiftSpinner.Value;
        bool decrypt = _decryptRadio.Checked;
        _outputBox.Text = Cipher.Apply(_inputText, decrypt ? -shift : shift);
        _saveButton.Enabled = true;
        _saveInputButton.Enabled = true;
        _swapButton.Enabled = true;
    }

    // ── Save input ────────────────────────────────────────────────────────────

    private void OnSaveInput(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog
        {
            Title = "Save input",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "txt",
            FileName = "input.txt",
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            File.WriteAllText(dlg.FileName, _inputBox.Text, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save file:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ── Save output ───────────────────────────────────────────────────────────

    private void OnSave(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog
        {
            Title = "Save output",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "txt",
            FileName = "output.txt",
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            File.WriteAllText(dlg.FileName, _outputBox.Text, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save file:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
