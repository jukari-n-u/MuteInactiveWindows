namespace MuteInactiveWindows
{
    public partial class FormSettings : Form
    {
        public Settings value { get; set; }
        public FormSettings(Settings settings)
        {
            InitializeComponent();

            listView1.Items.Clear();
            foreach (string s in settings.monitoredApps)
            {
                listView1.Items.Add(s);
            }

            numericUpDown1.Value = settings.updateInterval;
            if (!numericUpDown1.Validate()) numericUpDown1.Value = numericUpDown1.Minimum;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            value = new Settings(listView1.Items.Cast<ListViewItem>().Select(e => e.Text).ToArray(), ((int)numericUpDown1.Value));
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormNewMonitoredApp form = new FormNewMonitoredApp();
            DialogResult dialogResult = form.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                listView1.Items.Add(form.value);
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                var item = listView1.SelectedItems[0];
                FormEditMonitoredApp form = new FormEditMonitoredApp(item.Text);
                DialogResult dialogResult = form.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    item.Text = form.value;
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
                listView1.Items.Remove(listView1.SelectedItems[0]);
        }

    }
}