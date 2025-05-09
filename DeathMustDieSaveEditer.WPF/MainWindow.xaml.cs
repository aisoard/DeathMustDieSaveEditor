﻿using DeathMustDieSaveEditor.Core.Logic;
using DeathMustDieSaveEditor.Core.Models;
using DeathMustDieSaveEditor.Core.Models.SaveStructure;
using DeathMustDieSaveEditor.WPF.Helpers;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Label = System.Windows.Controls.Label;
using TextBox = System.Windows.Controls.TextBox;

namespace DeathMustDieSaveEditor.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowHelper mainWindowHelper = new MainWindowHelper();
        private DataManager DataManager = new DataManager();

        private bool AreComponentsLoaded = false;

        private string[] HeroNames = ["Avoron", "Kront", "Merris", "Nixi", "Skadi", "Lorien", "Leaf"];

        private string SelectedClass = "Knight";

        private bool IsHeroUnlocked = false;

        private List<Item> LoadedItems;
        private MainWindowItemAttributeHelper AttributeHelper;

        public MainWindow()
        {
            InitializeComponent();
            foreach (var heroName in HeroNames)
            {
                this.HeroNameComboBox.Items.Add(heroName);
            }

            this.AreComponentsLoaded = true;
            LoadDataManager();

            this.AttributeHelper = new MainWindowItemAttributeHelper(this.AttributeGrid);
            this.AttributeHelper.ItemChanged += AttributeHelper_ItemChanged;
        }

        private void LoadDataManager()
        {
            //string loadedFilePath = this.DataManager.LoadTestSave();
            string loadedFilePath = this.DataManager.TryLoadSaveAlone();
            if (string.IsNullOrEmpty(loadedFilePath))
            {
                this.LoadedFileNameLabel.Content = "No save found. Please click the button to load a save from custom location";
                return;
            }

            var fileName = System.IO.Path.GetFileName(loadedFilePath);
            this.LoadedFileNameLabel.Content = fileName;
            this.LoadInitialValues();
        }

        private void LoadCustomSaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Save Files|*.sav";

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string filePath = openFileDialog.FileName;
            this.DataManager.LoadSave(filePath);

            var fileName = System.IO.Path.GetFileName(filePath);
            this.LoadedFileNameLabel.Content = fileName;

            this.LoadInitialValues();
        }

        private void LoadInitialValues()
        {
            this.GoldTextBox.Text = this.DataManager.GetGold().ToString();
            this.HeroNameComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DataManager.SaveChanges();
            System.Windows.Forms.MessageBox.Show("Changes are saved. Have Fun!", "Saved");
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            string donateUrl = "https://www.paypal.com/donate/?hosted_button_id=QGHZY8WJM3B84";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {donateUrl}") { CreateNoWindow = true });
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void GoldTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AreComponentsLoaded)
                return;

            string stringValue = this.GoldTextBox.Text;
            if (string.IsNullOrEmpty(stringValue))
            {
                stringValue = "0";
                this.GoldTextBox.Text = stringValue;
            }
            else if (int.Parse(stringValue) > 99999999)
            {
                this.GoldTextBox.Text = "99999999";
                stringValue = "99999999";
            }

            var newGoldValue = int.Parse(stringValue);
            this.DataManager.SetGold(newGoldValue);
        }

        private void HeroNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = HeroNameComboBox.SelectedIndex;
            switch (index)
            {
                case 0:
                    this.SelectedClass = "Knight";
                    this.HeroAvatarImg.Source = new BitmapImage(new Uri(@"/Resources/Assets/Avoron.png", UriKind.Relative));
                    break;
                case 1:
                    this.SelectedClass = "Barbarian";
                    this.HeroAvatarImg.Source = new BitmapImage(new Uri(@"/Resources/Assets/Kront.png", UriKind.Relative));
                    break;
                case 2:
                    this.SelectedClass = "Sorceress";
                    this.HeroAvatarImg.Source = new BitmapImage(new Uri(@"/Resources/Assets/Witch.png", UriKind.Relative));
                    break;
                case 3:
                    this.SelectedClass = "Assassin";
                    this.HeroAvatarImg.Source = new BitmapImage(new Uri(@"/Resources/Assets/Nixi.png", UriKind.Relative));
                    break;
                case 4:
                    this.SelectedClass = "Warrior";
                    this.HeroAvatarImg.Source = new BitmapImage(new Uri(@"/Resources/Assets/Skadi.png", UriKind.Relative));
                    break;
                case 5:
                    this.SelectedClass = "Ranger";
                    this.HeroAvatarImg.Source = new BitmapImage(new Uri(@"/Resources/Assets/Lorien.png", UriKind.Relative));
                    break;
                case 6:
                    this.SelectedClass = "Druid";
                    this.HeroAvatarImg.Source = new BitmapImage(new Uri(@"/Resources/Assets/Leaf.png", UriKind.Relative));
                    break;
                default:
                    break;
            }

            if (this.DataManager.IsHeroUnlocked(this.SelectedClass))
            {
                this.IsHeroUnlocked = true;
                if (this.AttributeHelper is not null)
                    this.AttributeHelper.DeleteAllAttributeLines();

                this.LoadEquipment();
            }
            else
            {
                this.IsHeroUnlocked = false;
            }
        }

        private void LoadEquipment()
        {
            this.LoadedItems = this.DataManager.GetItems(this.SelectedClass)
                .ToList();
        }

        private void ItemSelectionImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this.ItemSelectionImg);
            var selection = this.mainWindowHelper.GetClickedType((int)mousePosition.X, (int)mousePosition.Y);
            if (selection == ItemType.NONE)
                return;

            var itemClicked = this.LoadedItems.FirstOrDefault(x => (ItemType)x.Type == selection);
            if (itemClicked != null)
            {
                this.AttributeHelper.InitializeItem(itemClicked);
                this.ItemTypeNameLabel.Content = ((ItemType)itemClicked.Type).ToString();
            }
            else
            {
                this.AttributeHelper.DeleteAllAttributeLines();
                this.ItemTypeNameLabel.Content = "No item is equipped there";
            }
        }

        private void AttributeHelper_ItemChanged(object? sender, EventArgs e)
        {
            this.DataManager.SetItems(this.SelectedClass, this.LoadedItems);
        }

        private void AddNewAffixButton_Click(object sender, RoutedEventArgs e)
        {
            this.AttributeHelper.AddNewAffix();
        }
    }
}