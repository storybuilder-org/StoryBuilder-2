﻿using System;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StoryCAD.ViewModels;
using WinRT;
namespace StoryCAD.Services.Dialogs;

public sealed partial class NewProjectPage : Page
{
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
}