﻿using Microsoft.Extensions.DependencyInjection;
using StoryCAD.DAL;
using StoryCAD.Services.Backend;
using StoryCAD.Services.Backup;
using StoryCAD.Services.Installation;
using StoryCAD.Services.Logging;
using StoryCAD.Services.Navigation;
using StoryCAD.Services.Preferences;
using StoryCAD.Services.Search;
using StoryCAD.ViewModels.Tools;
using StoryCAD.ViewModels;
using StoryCAD.Models;
using StoryCAD.Models.Tools;

namespace StoryCAD.Services.IoC;

public class ServiceConfigurator
{
    public static ServiceProvider Configure()
    {
        return new ServiceCollection()
             .AddSingleton<PreferencesService>()
             .AddSingleton<NavigationService>()
             .AddSingleton<LogService>()
             .AddSingleton<SearchService>()
             .AddSingleton<InstallationService>()
             .AddSingleton<ControlLoader>()
             .AddSingleton<ListLoader>()
             .AddSingleton<ToolLoader>()
             .AddSingleton<ScrivenerIo>()
             .AddSingleton<StoryReader>()
             .AddSingleton<StoryWriter>()
             .AddSingleton<MySqlIo>()
             .AddSingleton<BackupService>()
             .AddSingleton<AutoSaveService>()
             .AddSingleton<DeletionService>()
             .AddSingleton<BackendService>()
             .AddSingleton<ListData>()
             .AddSingleton<ToolsData>()
             // Register ViewModels 
             .AddSingleton<ShellViewModel>()
             .AddSingleton<OverviewViewModel>()
             .AddSingleton<CharacterViewModel>()
             .AddSingleton<ProblemViewModel>()
             .AddSingleton<SettingViewModel>()
             .AddSingleton<SceneViewModel>()
             .AddSingleton<FolderViewModel>()
             .AddSingleton<WebViewModel>()
             .AddSingleton<TrashCanViewModel>()
             .AddSingleton<UnifiedVM>()
             .AddSingleton<InitVM>()
             .AddSingleton<TreeViewSelection>()
             // Register ContentDialog ViewModels
             .AddSingleton<NewProjectViewModel>()
             .AddSingleton<NewRelationshipViewModel>()
             .AddSingleton<PrintReportDialogVM>()
             .AddSingleton<NarrativeToolVM>()
             // Register Tools ViewModels  
             .AddSingleton<KeyQuestionsViewModel>()
             .AddSingleton<TopicsViewModel>()
             .AddSingleton<MasterPlotsViewModel>()
             .AddSingleton<StockScenesViewModel>()
             .AddSingleton<DramaticSituationsViewModel>()
             .AddSingleton<SaveAsViewModel>()
             .AddSingleton<PreferencesViewModel>()
             .AddSingleton<FlawViewModel>()
             .AddSingleton<TraitsViewModel>()
             .BuildServiceProvider();
    }
}
