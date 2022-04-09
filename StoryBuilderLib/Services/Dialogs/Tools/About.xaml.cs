﻿using System.Data.Common;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using StoryBuilder.Models;
using StoryBuilder.Services.Logging;

namespace StoryBuilder.Services.Dialogs.Tools;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class About : Page
{
    public About()
    {
        InitializeComponent();
        string Revision = System.IO.File.ReadAllText(GlobalData.RootDirectory + "\\RevisionID");
        Version.Text = "Version: " + Windows.ApplicationModel.Package.Current.Id.Version.Major + "." + Windows.ApplicationModel.Package.Current.Id.Version.Minor + "." + Windows.ApplicationModel.Package.Current.Id.Version.Build + "." + Windows.ApplicationModel.Package.Current.Id.Version.Revision;
        Path.Text = "Installation Directory: " + GlobalData.RootDirectory;
    }
    private void OpenPath(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = System.IO.Path.Combine(GlobalData.RootDirectory,"Logs"),
            UseShellExecute = true,
            Verb = "open"
        });;
    }
}
