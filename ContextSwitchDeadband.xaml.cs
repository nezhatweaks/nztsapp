﻿using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    /// <summary>
    /// Interaction logic for ContextSwitchDeadband.xaml
    /// </summary>
    public partial class ContextSwitchDeadband : UserControl
    {
        private MainWindow mainWindow; // Field to store reference to MainWindow

        // Constructor accepting MainWindow reference
        public ContextSwitchDeadband(MainWindow window) // Ensure 'window' is defined here
        {
            InitializeComponent();
            mainWindow = window; // Store the reference
        }

        // Event handler for applying the registry tweak
        private void Optimize2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", true))
                {
                    if (key != null)
                    {
                        key.SetValue("ContextSwitchDeadband", 1, RegistryValueKind.DWord);
                        MessageBox.Show("ContextSwitchDeadband optimization applied!");
                        (Application.Current.MainWindow as MainWindow)?.MarkSettingsApplied();
                    }
                    else
                    {
                        MessageBox.Show("Failed to open registry key for ContextSwitchDeadband.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Event handler for deleting the ContextSwitchDeadband registry value
        private void DeleteContextSwitchDeadband_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("ContextSwitchDeadband", false); // false = do not throw an exception if the value does not exist
                        MessageBox.Show("ContextSwitchDeadband has been deleted!");
                    }
                    else
                    {
                        MessageBox.Show("Failed to open registry key for ContextSwitchDeadband.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}