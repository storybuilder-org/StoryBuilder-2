﻿using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCAD.DAL;
using StoryCAD.Models;
using StoryCAD.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace StoryCADTests;

[TestClass]
public class FileTests
{

    /// <summary>
    /// This creates a new STBX File to assure file creation works.
    /// </summary>
    [TestMethod]
    public void FileCreation()
    {
        //Get ShellVM and clear the StoryModel
        StoryModel StoryModel = new()
        {
            ProjectFilename ="TestProject.stbx",
            ProjectPath = Ioc.Default.GetRequiredService<AppState>().RootDirectory
        };

        OverviewModel _overview = new(Path.GetFileNameWithoutExtension("TestProject"), StoryModel)
        { DateCreated = DateTime.Today.ToString("yyyy-MM-dd"), Author = "StoryCAD Tests" };

        StoryNodeItem _overviewNode = new(_overview, null, StoryItemType.StoryOverview) {IsRoot = true };
        StoryModel.ExplorerView.Add(_overviewNode);
        TrashCanModel _trash = new(StoryModel);
        StoryNodeItem _trashNode = new(_trash, null);
        StoryModel.ExplorerView.Add(_trashNode);     // The trashcan is the second root
        FolderModel _narrative = new("Narrative View", StoryModel, StoryItemType.Folder);
        StoryNodeItem _narrativeNode = new(_narrative, null) { IsRoot = true };
        StoryModel.NarratorView.Add(_narrativeNode);

        //Add three test nodes.
        CharacterModel _character = new("TestCharacter", StoryModel);
        ProblemModel _problem = new("TestProblem", StoryModel);
        SceneModel _scene = new(StoryModel) {Name="TestScene" };
        StoryModel.ExplorerView.Add(new(_character, _overviewNode,StoryItemType.Character));
        StoryModel.ExplorerView.Add(new(_problem, _overviewNode,StoryItemType.Problem));
        StoryModel.ExplorerView.Add(new(_scene, _overviewNode,StoryItemType.Scene));


        //Check is loaded correctly
        Assert.IsTrue(StoryModel.StoryElements.Count == 6);
        Assert.IsTrue(StoryModel.StoryElements[0].Type == StoryItemType.StoryOverview);

        //Because we have created a file in this way we must populate ProjectFolder and ProjectFile.
        Directory.CreateDirectory(StoryModel.ProjectPath);

        //Populate file/folder vars.
        StoryModel.ProjectFolder = StorageFolder.GetFolderFromPathAsync(StoryModel.ProjectPath).GetAwaiter().GetResult();
        StoryModel.ProjectFile = StoryModel.ProjectFolder.CreateFileAsync("TestProject.stbx", CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();

		//Write file.
		StoryIO _storyIO = Ioc.Default.GetRequiredService<StoryIO>();
		_storyIO.WriteStory(StoryModel.ProjectFile, StoryModel).GetAwaiter().GetResult();

        //Sleep to ensure file is written.
        Thread.Sleep(10000);

        //Check file was really written to the disk.
        Assert.IsTrue(File.Exists(Path.Combine(StoryModel.ProjectPath, StoryModel.ProjectFilename)));
    }


    /// <summary>
    /// This tests a file load to ensure file creation works.
    /// </summary>
    [TestMethod]
    public async Task FileLoad()
    {
        // Arrange
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestInputs", "OpenTest.stbx"); // Ensure this file exists and is accessible
        Assert.IsTrue(File.Exists(filePath), "Test file does not exist.");

        StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
        StoryIO _rdr = Ioc.Default.GetRequiredService<StoryIO>();

        // Act
        StoryModel storyModel = await _rdr.ReadStory(file);

        // Assert
        Assert.AreEqual(6, storyModel.StoryElements.Count, "Story elements count mismatch."); 
        Assert.AreEqual(5, storyModel.ExplorerView.Count, "Overview Children count mismatch"); 
    }


    [TestMethod]
    public Task InvalidFileAccessTest()
    {
	    string Dir = AppDomain.CurrentDomain.BaseDirectory;
	    UnifiedVM UVM = new()
	    {
		    ProjectName = "TestProject",
		    ProjectPath = Path.Combine(Dir, "TestProject")
	    };

		//Check file path validity
		UVM.CheckValidity(null,null);

	    //Check Project Path was reset to default.
		Assert.IsTrue(UVM.ProjectPath == Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
		return null;
    }


    [TestMethod]
    public Task FullFileTest()
    {
	    string Dir = AppDomain.CurrentDomain.BaseDirectory;
		StorageFile File = StorageFile.GetFileFromPathAsync(Path.Combine(Dir, "TestInputs", "Full.stbx")).GetAwaiter().GetResult();
		StoryModel Model = Ioc.Default.GetRequiredService<StoryIO>().ReadStory(File).GetAwaiter().GetResult();


		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Author == "jake shaw");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).DateCreated == "2025-01-03");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).StoryIdea.Contains("Test"));
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Concept.Contains("Test"));
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Premise.Contains("Test"));
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).StoryType == "Short Story");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Viewpoint.Contains("Limited third person));
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).StoryGenre == "Mainsteam");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).LiteraryDevice == "Metafiction");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Voice == "Third person subjective");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Tense == "Present");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Style == "Mystery");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).StructureNotes.Contains("Test"));
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Tense == "Present");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Tone == "Indignant");
		Assert.IsTrue(((OverviewModel)Model.StoryElements[0]).Notes.Contains("This is a test outline, " +
		"it should have everything populated."));
		return null;
    }
}
