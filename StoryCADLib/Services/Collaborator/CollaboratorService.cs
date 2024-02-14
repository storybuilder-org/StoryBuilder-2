﻿using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCAD.Models;
using System.Runtime.Loader;
using Microsoft.UI.Windowing;
using StoryCAD.Collaborator.Views;
//using StoryCAD.Collaborator.Views;
using StoryCAD.Services.Backup;
using StoryCAD.ViewModels;
using WinUIEx;

namespace StoryCAD.Services.Collaborator;

public class CollaboratorService
{
    private bool dllExists = false;
    private string dllPath; 
    private AppState State = Ioc.Default.GetRequiredService<AppState>();
    public object Collaborator;
    public IWizardViewModel WizardVM;
    public Assembly CollabAssembly;
    private WindowEx window; // Do not make public, control from the collaborator side.
    public Type CollaboratorType;
    public bool CollaboratorEnabled()
    {
        return State.DeveloperBuild
               && Debugger.IsAttached
               && FindDll();
    }

    private bool FindDll()
    {
        // Get the path to the Documents folder
        //string documentsPath = "C:\\Users\\RARI\\Documents\\Repos\\CADCorp\\CollabApp\\CollaboratorLib\\bin\\x64\\Debug\\net8.0-windows10.0.22621.0";
        string documentsPath = "C:\\dev\\src\\StoryBuilderCollaborator\\CollaboratorLib\\bin\\x64\\Debug\\net8.0-windows10.0.22621.0";
        dllPath = Path.Combine(documentsPath, "CollaboratorLib.dll");

        // Verify that the DLL is present
        dllExists = File.Exists(dllPath);
        return dllExists;
    }

    /// <summary>
    /// This closes, disposes and full removes collaborator from memory.
    /// </summary>
    public void DestroyCollaborator(AppWindow sender, AppWindowClosingEventArgs args)
    {
        //TODO: Absolutely make sure Collaborator is not left in memory after this.
        if (Collaborator != null)
        {
            window.Close(); // Destroy window object

            //Null objects to deallocate them
            CollabAssembly = null;
            Collaborator = null;

            //Run garbage collection to clean up any remnants.
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public void RunCollaborator(CollaboratorArgs args)
    {
        // Prevent the user or timed services from making any changes on the 
        // StoryCAD side while Collaborator is running
        Ioc.Default.GetRequiredService<ShellViewModel>()._canExecuteCommands = false;
        Ioc.Default.GetRequiredService<BackupService>().StopTimedBackup();
        Ioc.Default.GetRequiredService<AutoSaveService>().StopAutoSave();

        // If Collaborator hasn't already been initalised, do so
        if (Collaborator == null)
        {
            // Should we initialize when CollaboratorService is being initialized rather then when attempting to use?

            // Create custom AssemblyLoadContext for StoryCAD's location
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var executingDirectory = Path.GetDirectoryName(assemblyPath);
            var loadContext = AssemblyLoadContext.Default;

            // Use the custom context to load the assembly
            CollabAssembly = loadContext.LoadFromAssemblyPath(dllPath);
            //Create a new WindowEx for collaborator to prevent access errors.
            window = new WindowEx();
            window.AppWindow.Closing += hideCollaborator;
            args.window = window;
            args.onDoneCallback = FinishedCallback;

            // Get the type of the Collaborator class
            CollaboratorType = CollabAssembly.GetType("StoryCollaborator.Collaborator");
            // Create an instance of the Collaborator class
            Collaborator = CollaboratorType!.GetConstructors()[0].Invoke(new object[] { args });
        }

        // Get the 'RunWizard' method that expects a parameter of type 'CollaboratorArgs'
        MethodInfo runMethod = CollaboratorType.GetMethod("RunWizard", new Type[] { typeof(CollaboratorArgs) });
        object[] methodArgs = new object[] { args };
        // ...and invoke it
        runMethod!.Invoke(Collaborator, methodArgs);
        // Display the Collaborator window
        args.window.Show();
    }

    /// <summary>
    /// This will hide the collaborator window.
    /// </summary>
    private void hideCollaborator(AppWindow appWindow, AppWindowClosingEventArgs e)
    {
        e.Cancel = true; // Cancel stops the window from being disposed.
        appWindow.Hide(); // Hide the window instead.
    }

    /// <summary>
    /// This is called when collaborator has finished doing stuff.
    /// Note: This is invoked from the Collaborator side via a Delegate named onDoneCallback
    /// </summary>
    private void FinishedCallback()
    {
        //Reenable Timed Backup if needed.
        if (Ioc.Default.GetRequiredService<AppState>().Preferences.TimedBackup)
        {
            Ioc.Default.GetRequiredService<BackupService>().StartTimedBackup();
        }

        //Reenable auto save if needed.
        if (Ioc.Default.GetRequiredService<AppState>().Preferences.AutoSave)
        {
            Ioc.Default.GetRequiredService<AutoSaveService>().StartAutoSave();
        }

        //Reenable StoryCAD buttons
        Ioc.Default.GetRequiredService<ShellViewModel>()._canExecuteCommands = true;
    }

}