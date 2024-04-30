﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StoryCAD.ViewModels;
using WinRT;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCAD.Services.Logging;

namespace StoryCAD.Services.Dialogs;

public sealed partial class NewProjectPage : Page
{
	LogService Logger = Ioc.Default.GetRequiredService<LogService>();

    public NewProjectPage(UnifiedVM vm)
    {
        InitializeComponent();
        UnifiedVM = vm;
    }

    public UnifiedVM UnifiedVM;

    public bool BrowseButtonClicked { get; set; }
    public bool ProjectFolderExists { get; set; }
    public StorageFolder ParentFolder { get; set; }
    public string ParentFolderPath { get; set; }
    public string ProjectFolderPath { get; set; }

    private async void Browse_Click(object sender, RoutedEventArgs e)
    {
        // Find a home for the new project
        FolderPicker folderPicker = new();
        if (Window.Current == null)
        {
            IntPtr hwnd = GetActiveWindow();
            //IntPtr hwnd = GlobalData.WindowHandle;
            IInitializeWithWindow initializeWithWindow = folderPicker.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);
        }
        folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        folderPicker.FileTypeFilter.Add("*");
        StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            ParentFolderPath = folder.Path;
            UnifiedVM.ProjectPath = folder.Path;
        }

    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative
    {
        IntPtr WindowHandle { get; }
    }

    [ComImport]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize(IntPtr hwnd);
    }

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
    public static extern IntPtr GetActiveWindow();

    private void CheckValidity(object sender, RoutedEventArgs e)
    {
	    Logger.Log(LogLevel.Info, $"Testing filename validity for {ProjectFolderPath}\\{ProjectName}");

		//Checks file path validity
		try { Directory.CreateDirectory(ProjectPathName.Text); }
        catch
        {
            ProjectPathName.Text = "";
            ProjectPathName.PlaceholderText = "You can't put files here!";
            return;
        }

        //Checks file name validity
        //NOTE: File.Create() Strips out anything past an illegal character
        //(or throws an exception if it's just an illegal character)
        try
        {
            string testfile = Path.Combine(ProjectPathName.Text, ProjectName.Text);
            var x = Path.GetInvalidPathChars();
            //Check validity
            Regex BadChars = new("[" + Regex.Escape(Path.GetInvalidPathChars().ToString()) + "]");
            if (BadChars.IsMatch(testfile))
            {
                throw new FileNotFoundException("the filename is invalid.");
            }

            //Try creating file and then checking it exists.
            using (FileStream fs = File.Create(testfile)) { }

            if (!File.Exists(testfile))
                throw new FileNotFoundException("the filename was not found.");
	        File.Delete(testfile);

        }
        catch (UnauthorizedAccessException)
        {
	        Logger.Log(LogLevel.Warn, $"User lacks access to {ProjectFolderPath}");
			//Set path to users documents if they try to save to an invalid location
			ProjectFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
	        return;
        }
		catch
        {
            ProjectName.Text = "";
            ProjectName.PlaceholderText = "You can't call your file that!";
            return;
        }

        UnifiedVM.MakeProject();
    }
}