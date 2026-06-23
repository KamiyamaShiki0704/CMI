using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace CMI
{
    public sealed partial class AddEventForm : Form
    {
        public AddEventForm(bool editMode = false, CMI.SoundEvent soundEvent = null)
        {
            InitializeComponent();
            CenterToScreen();
            editMode = editMode && soundEvent != null;
            eventNameTextbox.Enabled = !editMode;
            if (!editMode) return;
            Text = $@"Edit {soundEvent.Name}";
            // TODO: Function
            eventNameTextbox.Text = soundEvent.Name;
            soundPathTextbox.Text = Path.GetFileName(soundEvent.SoundPath);
            pointer1Textbox.Text = $@"0x{soundEvent.Pointer1:X}";
            pointer2Textbox.Text = $@"0x{soundEvent.Pointer2:X}";
            startbitTextbox.Text = soundEvent.Startbit.ToString();
            typeTextbox.Text = soundEvent.Type.ToString();
            fadeInSecondsTextbox.Text = soundEvent.FadeInSeconds.ToString("F", CultureInfo.InvariantCulture);
            fadeOutSecondsTextbox.Text = soundEvent.FadeOutSeconds.ToString("F", CultureInfo.InvariantCulture);
            fadeIntoNextTrackCheckbox.Checked = soundEvent.FadeIntoNextTrack;
            loopCheckbox.Checked = soundEvent.Loop;
        }

        private void SoundPathBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = @"Audio Files (*.mp3;*.wav)|*.mp3;*.wav"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            string selectedFilePath = dialog.FileName;
            // TODO: Cleanup
            string targetDirectory = Path.GetFullPath(CMI.modSoundFolderPath);
            string fileName = Path.GetFileName(selectedFilePath);
            string targetFilePath = Path.Combine(targetDirectory, fileName);
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);
            string selectedFileDirectory = Path.GetFullPath(
                Path.GetDirectoryName(selectedFilePath) ?? "");
            if (!string.Equals(selectedFileDirectory, targetDirectory))
                File.Copy(selectedFilePath, targetFilePath, true);
            soundPathTextbox.Text = fileName;
        }

        private void AddEventOKButton_Click(object sender, EventArgs e)
        {
            // TODO: We need a method for showing an error message...
            // TODO: We need checks for the other textboxes as well...
            if (string.IsNullOrEmpty(eventNameTextbox.Text))
            {
                MessageBox.Show(@"You must specify a name for the sound event.");
                return;
            }
            string pointer1 = pointer1Textbox.Text;
            string pointer2 = pointer2Textbox.Text;
            // TODO: Function
            if (pointer1.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                pointer1 = pointer1.Substring(2);
            if (pointer2.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                pointer2 = pointer2.Substring(2);
            CMI.SoundEvent soundEvent = new CMI.SoundEvent
            {
                Name = eventNameTextbox.Text,
                SoundPath = soundPathTextbox.Text,
                // TODO: We need to check that it's actually an int beforehand...
                Pointer1 = int.Parse(pointer1, NumberStyles.AllowHexSpecifier),
                Pointer2 = int.Parse(pointer2, NumberStyles.AllowHexSpecifier),
                Startbit = int.Parse(startbitTextbox.Text),
                Type = int.Parse(typeTextbox.Text),
                FadeInSeconds = float.Parse(fadeInSecondsTextbox.Text),
                FadeOutSeconds = float.Parse(fadeOutSecondsTextbox.Text),
                FadeIntoNextTrack = fadeIntoNextTrackCheckbox.Checked,
                Loop = loopCheckbox.Checked
            };
            CMI.soundEvents.Add(soundEvent);
            CMI.CommitUpdatedSoundEvents();
            Close();
        }
    }
}